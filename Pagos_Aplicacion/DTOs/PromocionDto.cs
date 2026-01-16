using Pagos_Dominio.Enums;

namespace Pagos_Aplicacion.DTOs;

public record PromocionDto(
    string Id,
    string IdEvento,
    string Codigo,
    TipoPromocion Tipo,
    decimal Valor,
    DateTimeOffset FechaInicioUtc,
    DateTimeOffset FechaFinUtc,
    int StockUsos,
    int UsosRealizados,
    bool Activa
);
