using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Dominio.Excepciones;
using System.Linq;

namespace Pagos_Aplicacion.Handlers.Commands;

public class CrearPagoHandler : IRequestHandler<CrearPagoCommand, ResultadoPagoDto>
{
    private readonly IRepositorioPagos _repositorioPagos;
    private readonly IRepositorioPromociones _repositorioPromociones;
    private readonly IRepositorioMetodosPago _repositorioMetodosPago;
    private readonly IRepositorioFacturas _repositorioFacturas;
    private readonly IServicioStripe _servicioStripe;
    private readonly IServicioFacturas _servicioFacturas;
    private readonly IPublicadorEventos _publicadorEventos;

    public CrearPagoHandler(
        IRepositorioPagos repositorioPagos,
        IRepositorioPromociones repositorioPromociones,
        IRepositorioMetodosPago repositorioMetodosPago,
        IRepositorioFacturas repositorioFacturas,
        IServicioStripe servicioStripe,
        IServicioFacturas servicioFacturas,
        IPublicadorEventos publicadorEventos)
    {
        _repositorioPagos = repositorioPagos;
        _repositorioPromociones = repositorioPromociones;
        _repositorioMetodosPago = repositorioMetodosPago;
        _repositorioFacturas = repositorioFacturas;
        _servicioStripe = servicioStripe;
        _servicioFacturas = servicioFacturas;
        _publicadorEventos = publicadorEventos;
    }

    public async Task<ResultadoPagoDto> Handle(CrearPagoCommand request, CancellationToken cancellationToken)
    {
        var pago = new Pago(request.IdReserva, request.IdUsuario, request.IdEvento, request.Monto, request.Moneda);
        pago.AsociarStripePaymentMethod(request.IdMetodoPagoStripe);

        if (!string.IsNullOrWhiteSpace(request.CodigoPromocion))
        {
            var codigo = request.CodigoPromocion.Trim().ToUpperInvariant();
            var promocion = await _repositorioPromociones.ObtenerPorCodigoYEventoAsync(request.IdEvento, codigo, cancellationToken);
            if (promocion is null)
                throw new PromocionNoValidaExcepcion("El cupón no existe para este evento.");

            var ahora = DateTimeOffset.UtcNow;
            if (!promocion.EstaVigente(ahora))
                throw new PromocionNoValidaExcepcion("El cupón no está vigente.");

            var descuento = promocion.CalcularDescuento(request.Monto);
            pago.AplicarPromocion(promocion.Codigo, descuento);

            var usoRegistrado = await _repositorioPromociones.IntentarRegistrarUsoAsync(promocion.Id, cancellationToken);
            if (!usoRegistrado)
                throw new PromocionNoValidaExcepcion("El cupón no tiene stock disponible.");
        }

        await _repositorioPagos.CrearAsync(pago, cancellationToken);

        var idStripeCustomer = await _repositorioMetodosPago.ObtenerIdStripeCustomerPorUsuarioAsync(request.IdUsuario, cancellationToken);
        var metodosUsuario = await _repositorioMetodosPago.ObtenerPorUsuarioAsync(request.IdUsuario, cancellationToken);

        var metodoYaRegistrado = metodosUsuario.Any(m => string.Equals(m.IdStripePaymentMethod, request.IdMetodoPagoStripe, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(idStripeCustomer) || !metodoYaRegistrado)
        {
            // Si no existe Customer para el usuario o viene pagando con un método nuevo,
            // lo registramos automáticamente (crea Customer si hace falta y adjunta el PaymentMethod).
            var datosMetodo = await _servicioStripe.AgregarMetodoPagoAsync(
                idStripeCustomer,
                request.EmailUsuario,
                request.IdMetodoPagoStripe,
                cancellationToken);

            idStripeCustomer = datosMetodo.IdStripeCustomer;

            var metodoGuardado = new MetodoPagoGuardado(
                request.IdUsuario,
                datosMetodo.IdStripeCustomer,
                datosMetodo.IdStripePaymentMethod,
                datosMetodo.Marca,
                datosMetodo.Ultimos4,
                datosMetodo.MesExp,
                datosMetodo.AnioExp);

            await _repositorioMetodosPago.CrearAsync(metodoGuardado, cancellationToken);
        }

        var montoEnCentavos = ConvertirAMenorUnidad(pago.MontoFinal);

        var intento = await _servicioStripe.CrearYConfirmarPagoAsync(
            idStripeCustomer,
            request.IdMetodoPagoStripe,
            montoEnCentavos,
            pago.Moneda,
            pago.Id,
            cancellationToken);

        pago.AsociarStripePaymentIntent(intento.IdPaymentIntent);

        if (string.Equals(intento.Estado, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            pago.MarcarConfirmado();
            await _repositorioPagos.ActualizarAsync(pago, cancellationToken);

            var pdf = _servicioFacturas.GenerarFacturaPdf(pago);
            var factura = new FacturaDigital(pago.Id, $"factura_{pago.Id}.pdf", pdf);
            await _repositorioFacturas.CrearAsync(factura, cancellationToken);


            await _publicadorEventos.PublicarPagoConfirmadoReservaAsync(
                new Reservas.Aplicacion.Eventos.PagoConfirmadoEvent(
                    Guid.Parse(pago.IdReserva),
                    Guid.Parse(pago.Id),
                    pago.MontoFinal,
                    pago.FechaConfirmacionUtc ?? DateTimeOffset.UtcNow),
                cancellationToken);

            return new ResultadoPagoDto(pago.Id, pago.Estado, pago.MontoFinal, pago.Moneda, false, null, factura.Id);
        }

        if (string.Equals(intento.Estado, "requires_action", StringComparison.OrdinalIgnoreCase)
            || string.Equals(intento.Estado, "requires_payment_method", StringComparison.OrdinalIgnoreCase)
            || string.Equals(intento.Estado, "processing", StringComparison.OrdinalIgnoreCase))
        {
            await _repositorioPagos.ActualizarAsync(pago, cancellationToken);
            return new ResultadoPagoDto(pago.Id, pago.Estado, pago.MontoFinal, pago.Moneda, true, intento.ClientSecret, null);
        }

        pago.MarcarFallido();
        await _repositorioPagos.ActualizarAsync(pago, cancellationToken);
        return new ResultadoPagoDto(pago.Id, pago.Estado, pago.MontoFinal, pago.Moneda, false, null, null);
    }

    private static long ConvertirAMenorUnidad(decimal monto)
    {
        // Nota: esto asume monedas con 2 decimales (USD, EUR, etc.).
        return (long)Math.Round(monto * 100m, 0, MidpointRounding.AwayFromZero);
    }
}
