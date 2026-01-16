namespace Pagos_Aplicacion.DTOs;

public record DatosMetodoPagoStripeDto(
    string IdStripeCustomer,
    string IdStripePaymentMethod,
    string Marca,
    string Ultimos4,
    long MesExp,
    long AnioExp
);
