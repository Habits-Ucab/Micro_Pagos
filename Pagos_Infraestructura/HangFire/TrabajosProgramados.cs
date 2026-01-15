
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;

namespace Pagos_Infraestructura.HangFire;

public class TrabajosProgramados
{
    private readonly IRepositorioPromociones _repositorioPromociones;
    private readonly IRepositorioPagos _repositorioPagos;
    private readonly IRepositorioFacturas _repositorioFacturas;
    private readonly IServicioStripe _servicioStripe;
    private readonly IServicioFacturas _servicioFacturas;
    private readonly IPublicadorEventos _publicadorEventos;

    public TrabajosProgramados(
        IRepositorioPromociones repositorioPromociones,
        IRepositorioPagos repositorioPagos,
        IRepositorioFacturas repositorioFacturas,
        IServicioStripe servicioStripe,
        IServicioFacturas servicioFacturas,
        IPublicadorEventos publicadorEventos)
    {
        _repositorioPromociones = repositorioPromociones;
        _repositorioPagos = repositorioPagos;
        _repositorioFacturas = repositorioFacturas;
        _servicioStripe = servicioStripe;
        _servicioFacturas = servicioFacturas;
        _publicadorEventos = publicadorEventos;
    }

    public async Task ExpirarPromocionesAsync()
    {
        await _repositorioPromociones.DesactivarExpiradasAsync(DateTimeOffset.UtcNow, CancellationToken.None);
    }

    public async Task ConciliarPagosAsync()
    {
        // Conciliación: revisa pagos pendientes y sincroniza su estado contra Stripe.
        // Usamos una pequeña ventana para evitar carreras inmediatas, pero sin esperar minutos.
        var maxFechaCreacion = DateTimeOffset.UtcNow.AddSeconds(-15);
        var pagos = await _repositorioPagos.ObtenerPendientesParaConciliacionAsync(maxFechaCreacion, maxRegistros: 50, CancellationToken.None);

        foreach (var pago in pagos)
        {
            if (string.IsNullOrWhiteSpace(pago.IdStripePaymentIntent))
                continue;

            var estado = await _servicioStripe.ObtenerEstadoPaymentIntentAsync(pago.IdStripePaymentIntent, CancellationToken.None);

            if (string.Equals(estado, "requires_confirmation", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(pago.IdStripePaymentMethod))
            {
                var reintento = await _servicioStripe.ConfirmarPaymentIntentAsync(
                    pago.IdStripePaymentIntent,
                    pago.IdStripePaymentMethod,
                    CancellationToken.None);

                estado = reintento.Estado;
            }

            if (string.Equals(estado, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                pago.MarcarConfirmado();
                await _repositorioPagos.ActualizarAsync(pago, CancellationToken.None);

                var existente = await _repositorioFacturas.ObtenerPorIdPagoAsync(pago.Id, CancellationToken.None);
                if (existente is null)
                {
                    var pdf = _servicioFacturas.GenerarFacturaPdf(pago);
                    var factura = new FacturaDigital(pago.Id, $"factura_{pago.Id}.pdf", pdf);
                    await _repositorioFacturas.CrearAsync(factura, CancellationToken.None);
                }


                if (!string.IsNullOrWhiteSpace(pago.IdReserva))
                {
                    await _publicadorEventos.PublicarPagoConfirmadoReservaAsync(
                        new Reservas.Aplicacion.Eventos.PagoConfirmadoEvent(
                            Guid.Parse(pago.IdReserva),
                            Guid.Parse(pago.Id),
                            pago.MontoFinal,
                            pago.FechaConfirmacionUtc ?? DateTimeOffset.UtcNow),
                        CancellationToken.None);
                }
            }
        }
    }
}
