using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Interfaces;

public interface IRepositorioPromociones
{
    Task CrearAsync(Promocion promocion, CancellationToken ct);
    Task<Promocion?> ObtenerPorIdAsync(string idPromocion, CancellationToken ct);
    Task<Promocion?> ObtenerPorCodigoYEventoAsync(string idEvento, string codigo, CancellationToken ct);
    Task<IReadOnlyList<Promocion>> ObtenerPorEventoAsync(string idEvento, CancellationToken ct);
    Task<bool> IntentarRegistrarUsoAsync(string idPromocion, CancellationToken ct);
    Task<int> DesactivarExpiradasAsync(DateTimeOffset ahoraUtc, CancellationToken ct);
}
