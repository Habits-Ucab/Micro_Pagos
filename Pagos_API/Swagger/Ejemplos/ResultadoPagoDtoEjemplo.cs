using Pagos_Aplicacion.DTOs;
using Pagos_Dominio.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Swagger.Ejemplos;

public class ResultadoPagoDtoEjemplo : IExamplesProvider<ResultadoPagoDto>
{
    public ResultadoPagoDto GetExamples()
    {
        return new ResultadoPagoDto(
            IdPago: "pago_789",
            Estado: EstadoPago.Confirmado,
            MontoFinal: 135.00m,
            Moneda: "usd",
            RequiereAccion: false,
            ClientSecret: null,
            IdFactura: "fact_111"
        );
    }
}
