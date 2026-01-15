using MediatR;
using Pagos_Aplicacion.DTOs;
using Pagos_Dominio.Enums;

namespace Pagos_Aplicacion.Commands;

public record CrearPromocionCommand(
    string IdEvento,
    string Codigo,
    TipoPromocion Tipo,
    decimal Valor,
    DateTimeOffset FechaInicioUtc,
    DateTimeOffset FechaFinUtc,
    int StockUsos
) : IRequest<PromocionDto>;
