using Pagos_Aplicacion.DTOs;

namespace Pagos_Aplicacion.Interfaces;

public interface IServicioStripe
{
    Task<DatosMetodoPagoStripeDto> AgregarMetodoPagoAsync(
        string? idStripeCustomerExistente,
        string emailUsuario,
        string idMetodoPagoStripe,
        CancellationToken ct);

    Task<ResultadoIntentoPagoStripeDto> CrearYConfirmarPagoAsync(
        string idStripeCustomer,
        string idMetodoPagoStripe,
        long montoEnUnidadMenor,
        string moneda,
        string idPago,
        CancellationToken ct);

    Task<ResultadoIntentoPagoStripeDto> ConfirmarPaymentIntentAsync(
        string idPaymentIntent,
        string idMetodoPagoStripe,
        CancellationToken ct);

    Task<string> ObtenerEstadoPaymentIntentAsync(string idPaymentIntent, CancellationToken ct);

    Task DesasociarMetodoPagoAsync(string idMetodoPagoStripe, CancellationToken ct);

    Task EstablecerMetodoPagoPorDefectoAsync(string idStripeCustomer, string? idMetodoPagoStripe, CancellationToken ct);
}
