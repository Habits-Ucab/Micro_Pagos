using Pagos_Aplicacion.DTOs;
using Pagos_Dominio.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class PromocionesListaEjemplo : IExamplesProvider<List<PromocionDto>>
{
    public List<PromocionDto> GetExamples()
    {
        return new List<PromocionDto>
        {
            new(
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
            )
        };
    }
}
