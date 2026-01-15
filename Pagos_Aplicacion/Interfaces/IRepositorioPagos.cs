using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Interfaces;

public interface IRepositorioPagos
{
    Task CrearAsync(Pago pago, CancellationToken ct);
    Task ActualizarAsync(Pago pago, CancellationToken ct);
    Task<Pago?> ObtenerPorIdAsync(string idPago, CancellationToken ct);
    Task<IReadOnlyList<Pago>> ObtenerPendientesParaConciliacionAsync(DateTimeOffset maxFechaCreacionUtc, int maxRegistros, CancellationToken ct);
}
