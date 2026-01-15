using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Commands;

public record AgregarMetodoPagoCommand(
    string IdUsuario,
    string EmailUsuario,
    string IdMetodoPagoStripe
) : IRequest<MetodoPagoDto>;
