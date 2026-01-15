namespace Pagos_Dominio.Entidades;

public class FacturaDigital
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string IdPago { get; private set; } = string.Empty;
    public string NombreArchivo { get; private set; } = string.Empty;
    public byte[] ContenidoPdf { get; private set; } = Array.Empty<byte>();
    public DateTimeOffset FechaCreacionUtc { get; private set; } = DateTimeOffset.UtcNow;

    private FacturaDigital()
    {
    }

    public FacturaDigital(string idPago, string nombreArchivo, byte[] contenidoPdf)
    {
        IdPago = idPago;
        NombreArchivo = nombreArchivo;
        ContenidoPdf = contenidoPdf;
    }
}
