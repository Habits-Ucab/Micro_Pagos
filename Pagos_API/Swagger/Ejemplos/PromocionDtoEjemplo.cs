using Pagos_Aplicacion.DTOs;
using Pagos_Dominio.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class PromocionDtoEjemplo : IExamplesProvider<PromocionDto>
{
    public PromocionDto GetExamples()
    {
        return new PromocionDto(
            Id: "promo_123",
            IdEvento: "evt_456",
            Codigo: "DESCUENTO10",
            Tipo: TipoPromocion.Porcentaje,
            Valor: 10,
            FechaInicioUtc: DateTimeOffset.UtcNow.AddHours(-1),
            FechaFinUtc: DateTimeOffset.UtcNow.AddDays(7),
            StockUsos: 100,
            UsosRealizados: 3,
            Activa: true
        );
    }
}
