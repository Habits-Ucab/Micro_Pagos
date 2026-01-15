using Pagos_Dominio.Enums;

namespace Pagos_Aplicacion.DTOs;

public record PagoDetalleDto(
    string IdPago,
    EstadoPago Estado,
    decimal MontoFinal,
    string Moneda,
    string? IdStripePaymentIntent,
    string? IdFactura,
    string? EstadoStripe
);
