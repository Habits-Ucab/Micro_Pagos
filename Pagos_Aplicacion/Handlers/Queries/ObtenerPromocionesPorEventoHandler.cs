using MediatR;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;

namespace Pagos_Aplicacion.Handlers.Queries;

public class ObtenerPromocionesPorEventoHandler : IRequestHandler<ObtenerPromocionesPorEventoQuery, IReadOnlyList<PromocionDto>>
{
    private readonly IRepositorioPromociones _repositorioPromociones;

    public ObtenerPromocionesPorEventoHandler(IRepositorioPromociones repositorioPromociones)
    {
        _repositorioPromociones = repositorioPromociones;
    }

    public async Task<IReadOnlyList<PromocionDto>> Handle(ObtenerPromocionesPorEventoQuery request, CancellationToken cancellationToken)
    {
        var promos = await _repositorioPromociones.ObtenerPorEventoAsync(request.IdEvento, cancellationToken);
        var ahora = DateTimeOffset.UtcNow;

        if (request.SoloVigentes)
        {
            promos = promos.Where(p => p.EstaVigente(ahora) && p.TieneStock()).ToList();
        }

        return promos
            .OrderByDescending(p => p.Activa)
            .ThenBy(p => p.FechaFinUtc)
            .Select(p => new PromocionDto(
                p.Id,
                p.IdEvento,
                p.Codigo,
                p.Tipo,
                p.Valor,
                p.FechaInicioUtc,
                p.FechaFinUtc,
                p.StockUsos,
                p.UsosRealizados,
                p.Activa))
            .ToList();
    }
}
