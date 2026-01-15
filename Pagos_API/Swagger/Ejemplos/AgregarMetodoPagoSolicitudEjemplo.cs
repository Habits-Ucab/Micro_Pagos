using Pagos_API.Modelos.MetodosPago;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class AgregarMetodoPagoSolicitudEjemplo : IExamplesProvider<AgregarMetodoPagoSolicitud>
{
    public AgregarMetodoPagoSolicitud GetExamples()
    {
        return new AgregarMetodoPagoSolicitud
        {
            IdUsuario = "usr_123",
            EmailUsuario = "usuario@correo.com",
            IdMetodoPagoStripe = "pm_1ABCDEF234567890"
        };
    }
}
