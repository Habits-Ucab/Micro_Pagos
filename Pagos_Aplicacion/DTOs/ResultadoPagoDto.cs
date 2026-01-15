using Pagos_Dominio.Enums;

namespace Pagos_Aplicacion.DTOs;

public record ResultadoPagoDto(
    string IdPago,
    EstadoPago Estado,
    decimal MontoFinal,
    string Moneda,
    bool RequiereAccion,
    string? ClientSecret,
    string? IdFactura
);
