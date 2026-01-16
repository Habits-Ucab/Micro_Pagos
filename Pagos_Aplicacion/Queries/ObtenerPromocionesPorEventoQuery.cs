using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Queries;

public record ObtenerPromocionesPorEventoQuery(string IdEvento, bool SoloVigentes = true) : IRequest<IReadOnlyList<PromocionDto>>;
