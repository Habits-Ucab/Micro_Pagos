using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Interfaces;

public interface IRepositorioMetodosPago
{
    Task CrearAsync(MetodoPagoGuardado metodo, CancellationToken ct);
    Task<IReadOnlyList<MetodoPagoGuardado>> ObtenerPorUsuarioAsync(string idUsuario, CancellationToken ct);
    Task<string?> ObtenerIdStripeCustomerPorUsuarioAsync(string idUsuario, CancellationToken ct);
    Task<bool> EliminarPorUsuarioYPaymentMethodAsync(string idUsuario, string idStripePaymentMethod, CancellationToken ct);
}
