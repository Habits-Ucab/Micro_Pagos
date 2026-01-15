using Pagos_Dominio.Enums;
using Pagos_Dominio.Excepciones;

namespace Pagos_Dominio.Entidades;

public class Promocion
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string IdEvento { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;

    public TipoPromocion Tipo { get; private set; }
    public decimal Valor { get; private set; }

    public DateTimeOffset FechaInicioUtc { get; private set; }
    public DateTimeOffset FechaFinUtc { get; private set; }

    public int StockUsos { get; private set; }
    public int UsosRealizados { get; private set; }

    public bool Activa { get; private set; } = true;

    private Promocion()
    {
    }

    public Promocion(
        string idEvento,
        string codigo,
        TipoPromocion tipo,
        decimal valor,
        DateTimeOffset fechaInicioUtc,
        DateTimeOffset fechaFinUtc,
        int stockUsos)
    {
        if (string.IsNullOrWhiteSpace(idEvento)) throw new PromocionNoValidaExcepcion("El idEvento es obligatorio.");
        if (string.IsNullOrWhiteSpace(codigo)) throw new PromocionNoValidaExcepcion("El código es obligatorio.");
        if (valor <= 0) throw new PromocionNoValidaExcepcion("El valor debe ser mayor a 0.");
        if (fechaFinUtc <= fechaInicioUtc) throw new PromocionNoValidaExcepcion("La fecha fin debe ser posterior a la fecha inicio.");
        if (stockUsos <= 0) throw new PromocionNoValidaExcepcion("El stock de usos debe ser mayor a 0.");

        IdEvento = idEvento;
        Codigo = codigo.Trim().ToUpperInvariant();
        Tipo = tipo;
        Valor = valor;
        FechaInicioUtc = fechaInicioUtc;
        FechaFinUtc = fechaFinUtc;
        StockUsos = stockUsos;
        UsosRealizados = 0;
        Activa = true;
    }

    public bool EstaVigente(DateTimeOffset ahoraUtc)
    {
        return Activa && ahoraUtc >= FechaInicioUtc && ahoraUtc <= FechaFinUtc;
    }

    public bool TieneStock()
    {
        return UsosRealizados < StockUsos;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    public void RegistrarUso()
    {
        if (!TieneStock()) throw new PromocionNoValidaExcepcion("La promoción no tiene stock disponible.");
        UsosRealizados++;
        if (!TieneStock())
        {
            Activa = false;
        }
    }

    public decimal CalcularDescuento(decimal monto)
    {
        if (monto <= 0) return 0;

        return Tipo switch
        {
            TipoPromocion.Porcentaje => Math.Round(monto * (Valor / 100m), 2, MidpointRounding.AwayFromZero),
            TipoPromocion.MontoFijo => Math.Min(monto, Valor),
            _ => 0
        };
    }
}
