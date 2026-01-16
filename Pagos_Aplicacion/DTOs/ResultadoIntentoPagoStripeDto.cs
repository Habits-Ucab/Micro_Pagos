namespace Pagos_Aplicacion.DTOs;

public record ResultadoIntentoPagoStripeDto(
    string IdPaymentIntent,
    string Estado,
    string? ClientSecret
);
