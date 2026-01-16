using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Queries;

public record ObtenerPagosPorUsuarioQuery(string IdUsuario, int Limit) : IRequest<IReadOnlyList<PagoResumenDto>>;
