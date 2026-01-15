namespace Pagos_Dominio.Entidades;

public class MetodoPagoGuardado
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string IdUsuario { get; private set; } = string.Empty;

    public string IdStripeCustomer { get; private set; } = string.Empty;
    public string IdStripePaymentMethod { get; private set; } = string.Empty;

    public string Marca { get; private set; } = string.Empty;
    public string Ultimos4 { get; private set; } = string.Empty;
    public long MesExp { get; private set; }
    public long AñoExp { get; private set; }

    public DateTimeOffset FechaRegistroUtc { get; private set; } = DateTimeOffset.UtcNow;

    private MetodoPagoGuardado()
    {
    }

    public MetodoPagoGuardado(
        string idUsuario,
        string idStripeCustomer,
        string idStripePaymentMethod,
        string marca,
        string ultimos4,
        long mesExp,
        long AñoExp)
    {
        IdUsuario = idUsuario;
        IdStripeCustomer = idStripeCustomer;
        IdStripePaymentMethod = idStripePaymentMethod;
        Marca = marca;
        Ultimos4 = ultimos4;
        MesExp = mesExp;
        this.AñoExp = AñoExp;
    }
}
