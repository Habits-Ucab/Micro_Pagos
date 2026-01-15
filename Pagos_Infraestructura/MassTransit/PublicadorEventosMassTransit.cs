using MassTransit;
using Pagos_Aplicacion.Interfaces;

namespace Pagos_Infraestructura.MassTransit;

public class PublicadorEventosMassTransit : IPublicadorEventos
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PublicadorEventosMassTransit(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }


    public Task PublicarPagoConfirmadoReservaAsync(Reservas.Aplicacion.Eventos.PagoConfirmadoEvent evento, CancellationToken ct)
    {
        return _publishEndpoint.Publish(evento, ct);
    }
}
