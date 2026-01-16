using MediatR;
using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Commands;

public record CrearPagoCommand(
    string IdReserva,
    string IdUsuario,
    string EmailUsuario,
    string IdEvento,
    decimal Monto,
    string Moneda,
    string IdMetodoPagoStripe,
    string? CodigoPromocion
) : IRequest<ResultadoPagoDto>;
