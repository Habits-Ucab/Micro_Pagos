using Pagos_Dominio.Enums;

namespace Pagos_Aplicacion.DTOs;

public record PagoResumenDto(
    string Id,
    EstadoPago Estado,
    decimal MontoFinal,
    string Moneda,
    string IdEvento,
    DateTimeOffset FechaCreacionUtc,
    DateTimeOffset? FechaConfirmacionUtc);
