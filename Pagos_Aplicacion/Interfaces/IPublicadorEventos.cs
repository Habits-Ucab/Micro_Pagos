

namespace Pagos_Aplicacion.Interfaces;

public interface IPublicadorEventos
{

    Task PublicarPagoConfirmadoReservaAsync(Reservas.Aplicacion.Eventos.PagoConfirmadoEvent evento, CancellationToken ct);
}
