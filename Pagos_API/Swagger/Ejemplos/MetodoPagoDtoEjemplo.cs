using Pagos_Aplicacion.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class MetodoPagoDtoEjemplo : IExamplesProvider<MetodoPagoDto>
{
    public MetodoPagoDto GetExamples()
    {
        return new MetodoPagoDto(
            IdUsuario: "usr_123",
            IdStripeCustomer: "cus_ABC123",
            IdStripePaymentMethod: "pm_1ABCDEF234567890",
            Marca: "visa",
            Ultimos4: "4242",
            MesExp: 12,
            AnioExp: 2030
        );
    }
}
