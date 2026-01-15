using MediatR;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;

namespace Pagos_Aplicacion.Handlers.Queries;

public class ObtenerPagosPorUsuarioHandler : IRequestHandler<ObtenerPagosPorUsuarioQuery, IReadOnlyList<PagoResumenDto>>
{
    private readonly IRepositorioPagos _repositorioPagos;

    public ObtenerPagosPorUsuarioHandler(IRepositorioPagos repositorioPagos)
    {
        _repositorioPagos = repositorioPagos;
    }

    public async Task<IReadOnlyList<PagoResumenDto>> Handle(ObtenerPagosPorUsuarioQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 200);

        var pagos = await _repositorioPagos.ObtenerPorUsuarioAsync(request.IdUsuario, limit, cancellationToken);

        return pagos
            .Select(p => new PagoResumenDto(
                p.Id,
                p.Estado,
                p.MontoFinal,
                p.Moneda,
                p.IdEvento,
                p.FechaCreacionUtc,
                p.FechaConfirmacionUtc))
            .ToList();
    }
}
