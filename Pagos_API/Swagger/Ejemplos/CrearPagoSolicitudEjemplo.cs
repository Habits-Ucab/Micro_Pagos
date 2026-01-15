using Pagos_API.Modelos.Pagos;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class CrearPagoSolicitudEjemplo : IExamplesProvider<CrearPagoSolicitud>
{
    public CrearPagoSolicitud GetExamples()
    {
        return new CrearPagoSolicitud
        {
            IdUsuario = "usr_123",
            EmailUsuario = "usuario@correo.com",
            IdEvento = "evt_456",
            Monto = 150.00m,
            Moneda = "usd",
            IdMetodoPagoStripe = "pm_1ABCDEF234567890",
            CodigoPromocion = "DESCUENTO10"
        };
    }
}
