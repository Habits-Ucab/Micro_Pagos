using Pagos_API.Modelos.Promociones;
using Pagos_Dominio.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class CrearPromocionSolicitudEjemplo : IExamplesProvider<CrearPromocionSolicitud>
{
    public CrearPromocionSolicitud GetExamples()
    {
        return new CrearPromocionSolicitud
        {
            IdEvento = "evt_456",
            Codigo = "DESCUENTO10",
            Tipo = TipoPromocion.Porcentaje,
            Valor = 10,
            FechaInicioUtc = DateTimeOffset.UtcNow.AddHours(-1),
            FechaFinUtc = DateTimeOffset.UtcNow.AddDays(7),
            StockUsos = 100
        };
    }
}
