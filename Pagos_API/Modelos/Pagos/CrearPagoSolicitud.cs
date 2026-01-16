namespace Pagos_API.Modelos.Pagos;

public class CrearPagoSolicitud
{
    public string IdReserva { get; set; } = string.Empty;
    public string IdUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;

    public string IdEvento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "usd";

    public string IdMetodoPagoStripe { get; set; } = string.Empty;
    public string? CodigoPromocion { get; set; }
}
