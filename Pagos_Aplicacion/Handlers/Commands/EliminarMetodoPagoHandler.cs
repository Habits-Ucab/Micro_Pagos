using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Interfaces;

namespace Pagos_Aplicacion.Handlers.Commands;

public class EliminarMetodoPagoHandler : IRequestHandler<EliminarMetodoPagoCommand, bool>
{
    private readonly IRepositorioMetodosPago _repositorioMetodosPago;
    private readonly IServicioStripe _servicioStripe;

    public EliminarMetodoPagoHandler(IRepositorioMetodosPago repositorioMetodosPago, IServicioStripe servicioStripe)
    {
        _repositorioMetodosPago = repositorioMetodosPago;
        _servicioStripe = servicioStripe;
    }

    public async Task<bool> Handle(EliminarMetodoPagoCommand request, CancellationToken cancellationToken)
    {
        var idUsuario = (request.IdUsuario ?? string.Empty).Trim();
        var idPaymentMethod = (request.IdMetodoPagoStripe ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(idUsuario))
            throw new InvalidOperationException("El idUsuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(idPaymentMethod))
            throw new InvalidOperationException("El idMetodoPagoStripe es obligatorio.");

        // Validar que exista en nuestra BD antes de tocar Stripe.
        var metodos = await _repositorioMetodosPago.ObtenerPorUsuarioAsync(idUsuario, cancellationToken);
        var metodo = metodos.FirstOrDefault(m => string.Equals(m.IdStripePaymentMethod, idPaymentMethod, StringComparison.OrdinalIgnoreCase));
        if (metodo is null)
            return false;

        // Desasociar en Stripe (si falla aquí, preferimos no borrar en Mongo).
        await _servicioStripe.DesasociarMetodoPagoAsync(idPaymentMethod, cancellationToken);

        var eliminado = await _repositorioMetodosPago.EliminarPorUsuarioYPaymentMethodAsync(idUsuario, idPaymentMethod, cancellationToken);

        // Si el usuario aún tiene métodos, re-asignar uno como default.
        if (eliminado)
        {
            var restantes = await _repositorioMetodosPago.ObtenerPorUsuarioAsync(idUsuario, cancellationToken);
            var nuevoDefault = restantes
                .OrderByDescending(x => x.FechaRegistroUtc)
                .FirstOrDefault();

            await _servicioStripe.EstablecerMetodoPagoPorDefectoAsync(
                metodo.IdStripeCustomer,
                nuevoDefault?.IdStripePaymentMethod,
                cancellationToken);
        }

        return eliminado;
    }
}
