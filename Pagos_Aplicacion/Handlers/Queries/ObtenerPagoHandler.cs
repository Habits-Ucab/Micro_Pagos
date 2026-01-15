using MediatR;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;

namespace Pagos_Aplicacion.Handlers.Queries;

public class ObtenerPagoHandler : IRequestHandler<ObtenerPagoQuery, PagoDetalleDto?>
{
    private readonly IRepositorioPagos _repositorioPagos;
    private readonly IRepositorioFacturas _repositorioFacturas;

    public ObtenerPagoHandler(IRepositorioPagos repositorioPagos, IRepositorioFacturas repositorioFacturas)
    {
        _repositorioPagos = repositorioPagos;
        _repositorioFacturas = repositorioFacturas;
    }

    public async Task<PagoDetalleDto?> Handle(ObtenerPagoQuery request, CancellationToken cancellationToken)
    {
        var pago = await _repositorioPagos.ObtenerPorIdAsync(request.IdPago, cancellationToken);
        if (pago is null) return null;

        var factura = await _repositorioFacturas.ObtenerPorIdPagoAsync(pago.Id, cancellationToken);

        return new PagoDetalleDto(
            pago.Id,
            pago.Estado,
            pago.MontoFinal,
            pago.Moneda,
            pago.IdStripePaymentIntent,
            factura?.Id,
            null);
    }
}
