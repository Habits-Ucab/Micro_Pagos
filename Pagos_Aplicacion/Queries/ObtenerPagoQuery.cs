using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Queries;

public record ObtenerPagoQuery(string IdPago) : IRequest<PagoDetalleDto?>;
