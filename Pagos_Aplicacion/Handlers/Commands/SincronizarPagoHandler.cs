using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;

namespace Pagos_Aplicacion.Handlers.Commands;

public class SincronizarPagoHandler : IRequestHandler<SincronizarPagoCommand, PagoDetalleDto>
{
    private readonly IRepositorioPagos _repositorioPagos;
    private readonly IRepositorioFacturas _repositorioFacturas;
    private readonly IServicioStripe _servicioStripe;
    private readonly IServicioFacturas _servicioFacturas;
    private readonly IPublicadorEventos _publicadorEventos;

    public SincronizarPagoHandler(
        IRepositorioPagos repositorioPagos,
        IRepositorioFacturas repositorioFacturas,
        IServicioStripe servicioStripe,
        IServicioFacturas servicioFacturas,
        IPublicadorEventos publicadorEventos)
    {
        _repositorioPagos = repositorioPagos;
        _repositorioFacturas = repositorioFacturas;
        _servicioStripe = servicioStripe;
        _servicioFacturas = servicioFacturas;
        _publicadorEventos = publicadorEventos;
    }

    public async Task<PagoDetalleDto> Handle(SincronizarPagoCommand request, CancellationToken cancellationToken)
    {
        var pago = await _repositorioPagos.ObtenerPorIdAsync(request.IdPago, cancellationToken);
        if (pago is null)
            throw new InvalidOperationException("Pago no encontrado.");

        var estadoStripe = (string?)null;

        if (!string.IsNullOrWhiteSpace(pago.IdStripePaymentIntent))
        {
            estadoStripe = await _servicioStripe.ObtenerEstadoPaymentIntentAsync(pago.IdStripePaymentIntent, cancellationToken);

            if (string.Equals(estadoStripe, "requires_confirmation", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(pago.IdStripePaymentMethod))
            {
                var reintento = await _servicioStripe.ConfirmarPaymentIntentAsync(
                    pago.IdStripePaymentIntent,
                    pago.IdStripePaymentMethod,
                    cancellationToken);

                estadoStripe = reintento.Estado;
            }

            if (string.Equals(estadoStripe, "succeeded", StringComparison.OrdinalIgnoreCase)
                && pago.Estado != Pagos_Dominio.Enums.EstadoPago.Confirmado)
            {
                pago.MarcarConfirmado();
                await _repositorioPagos.ActualizarAsync(pago, cancellationToken);

                var existente = await _repositorioFacturas.ObtenerPorIdPagoAsync(pago.Id, cancellationToken);
                if (existente is null)
                {
                    var pdf = _servicioFacturas.GenerarFacturaPdf(pago);
                    var factura = new Pagos_Dominio.Entidades.FacturaDigital(pago.Id, $"factura_{pago.Id}.pdf", pdf);
                    await _repositorioFacturas.CrearAsync(factura, cancellationToken);
                    existente = factura;
                }
                

                if (!string.IsNullOrWhiteSpace(pago.IdReserva))
                {
                    await _publicadorEventos.PublicarPagoConfirmadoReservaAsync(
                        new Reservas.Aplicacion.Eventos.PagoConfirmadoEvent(
                            Guid.Parse(pago.IdReserva),
                            Guid.Parse(pago.Id),
                            pago.MontoFinal,
                            pago.FechaConfirmacionUtc ?? DateTimeOffset.UtcNow),
                        cancellationToken);
                }

                return new PagoDetalleDto(
                    pago.Id,
                    pago.Estado,
                    pago.MontoFinal,
                    pago.Moneda,
                    pago.IdStripePaymentIntent,
                    existente?.Id,
                    estadoStripe);
            }
        }

        var facturaExistente = await _repositorioFacturas.ObtenerPorIdPagoAsync(pago.Id, cancellationToken);

        return new PagoDetalleDto(
            pago.Id,
            pago.Estado,
            pago.MontoFinal,
            pago.Moneda,
            pago.IdStripePaymentIntent,
            facturaExistente?.Id,
            estadoStripe);
    }
}
