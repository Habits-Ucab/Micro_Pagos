using MediatR;

namespace Pagos_Aplicacion.Commands;

public record EliminarMetodoPagoCommand(string IdUsuario, string IdMetodoPagoStripe) : IRequest<bool>;
