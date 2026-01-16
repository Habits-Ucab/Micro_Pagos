using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class ErrorRespuestaEjemplo : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new
        {
            mensaje = "El cupón no está vigente."
        };
    }
}
