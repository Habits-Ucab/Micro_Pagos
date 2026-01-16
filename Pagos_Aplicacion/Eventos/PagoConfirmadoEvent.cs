using System;

namespace Reservas.Aplicacion.Eventos;

public record PagoConfirmadoEvent(Guid ReservaId, Guid PagoId, decimal MontoPagado, DateTimeOffset FechaPago);
