using Pagos_Dominio.Enums;

namespace Pagos_API.Modelos.Promociones;

public class CrearPromocionSolicitud
{
    public string IdEvento { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;

    public TipoPromocion Tipo { get; set; }
    public decimal Valor { get; set; }

    public DateTimeOffset FechaInicioUtc { get; set; }
    public DateTimeOffset FechaFinUtc { get; set; }

    public int StockUsos { get; set; }
}
