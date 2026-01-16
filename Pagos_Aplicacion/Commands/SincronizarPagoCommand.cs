using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Commands;

public record SincronizarPagoCommand(string IdPago) : IRequest<PagoDetalleDto>;
