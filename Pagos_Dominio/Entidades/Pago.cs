using Pagos_Dominio.Enums;
using Pagos_Dominio.Excepciones;

namespace Pagos_Dominio.Entidades;

public class Pago
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string IdReserva { get; private set; } = string.Empty;
    public string IdUsuario { get; private set; } = string.Empty;
    public string IdEvento { get; private set; } = string.Empty;

    public decimal MontoOriginal { get; private set; }
    public decimal DescuentoAplicado { get; private set; }
    public decimal MontoFinal { get; private set; }
    public string Moneda { get; private set; } = "usd";

    public EstadoPago Estado { get; private set; } = EstadoPago.Pendiente;
    public string? IdStripePaymentIntent { get; private set; }
    public string? IdStripePaymentMethod { get; private set; }

    public string? CodigoPromocionAplicado { get; private set; }

    public DateTimeOffset FechaCreacionUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaConfirmacionUtc { get; private set; }

    private Pago()
    {
    }

    public Pago(string idReserva, string idUsuario, string idEvento, decimal montoOriginal, string moneda)
    {
        if (string.IsNullOrWhiteSpace(idReserva)) throw new PagoInvalidoExcepcion("El idReserva es obligatorio.");
        if (string.IsNullOrWhiteSpace(idUsuario)) throw new PagoInvalidoExcepcion("El idUsuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(idEvento)) throw new PagoInvalidoExcepcion("El idEvento es obligatorio.");
        if (montoOriginal <= 0) throw new PagoInvalidoExcepcion("El monto debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(moneda)) throw new PagoInvalidoExcepcion("La moneda es obligatoria.");

        IdReserva = idReserva;
        IdUsuario = idUsuario;
        IdEvento = idEvento;
        MontoOriginal = montoOriginal;
        Moneda = moneda.ToLowerInvariant();

        DescuentoAplicado = 0;
        MontoFinal = montoOriginal;
    }

    public void AplicarPromocion(string codigo, decimal descuento)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new PagoInvalidoExcepcion("El código de promoción es obligatorio.");
        if (descuento < 0) throw new PagoInvalidoExcepcion("El descuento no puede ser negativo.");

        CodigoPromocionAplicado = codigo;
        DescuentoAplicado = descuento;
        MontoFinal = Math.Max(0, MontoOriginal - descuento);
    }

    public void AsociarStripePaymentIntent(string idPaymentIntent)
    {
        if (string.IsNullOrWhiteSpace(idPaymentIntent))
            throw new PagoInvalidoExcepcion("El id de PaymentIntent es obligatorio.");

        IdStripePaymentIntent = idPaymentIntent;
    }

    public void AsociarStripePaymentMethod(string idPaymentMethod)
    {
        if (string.IsNullOrWhiteSpace(idPaymentMethod))
            throw new PagoInvalidoExcepcion("El id de PaymentMethod es obligatorio.");

        IdStripePaymentMethod = idPaymentMethod;
    }

    public void MarcarConfirmado()
    {
        Estado = EstadoPago.Confirmado;
        FechaConfirmacionUtc = DateTimeOffset.UtcNow;
    }

    public void MarcarFallido()
    {
        Estado = EstadoPago.Fallido;
    }

    public void MarcarCancelado()
    {
        Estado = EstadoPago.Cancelado;
    }
}
