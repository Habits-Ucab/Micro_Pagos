namespace Pagos_API.Modelos.MetodosPago;

public class AgregarMetodoPagoSolicitud
{
    public string IdUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;

    // Debe venir desde Stripe.js (tokenización), ejemplo: pm_...
    public string IdMetodoPagoStripe { get; set; } = string.Empty;
}
