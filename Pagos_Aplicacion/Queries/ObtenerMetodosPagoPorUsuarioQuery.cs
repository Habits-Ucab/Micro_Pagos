using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Queries;

public record ObtenerMetodosPagoPorUsuarioQuery(string IdUsuario) : IRequest<IReadOnlyList<MetodoPagoDto>>;
