using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Interfaces;

public interface IRepositorioFacturas
{
    Task CrearAsync(FacturaDigital factura, CancellationToken ct);
    Task<FacturaDigital?> ObtenerPorIdPagoAsync(string idPago, CancellationToken ct);
}
