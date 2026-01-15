using MassTransit;
using Moq;
using Pagos_Infraestructura.MassTransit;
using Reservas.Aplicacion.Eventos;
using Xunit;

namespace Pagos_Test.Infraestructura.MassTransit;

public class PublicadorEventosMassTransitTests
{
    [Fact]
    public async Task PublicarPagoConfirmadoReservaAsync_deberia_publicar_evento()
    {
        // Preparación
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var publicador = new PublicadorEventosMassTransit(publishEndpoint.Object);

        var evento = new PagoConfirmadoEvent(Guid.NewGuid(), Guid.NewGuid(), 100m, DateTimeOffset.UtcNow);

        // Acción
        await publicador.PublicarPagoConfirmadoReservaAsync(evento, CancellationToken.None);

        // Verificación
        publishEndpoint.Verify(p => p.Publish(evento, It.IsAny<CancellationToken>()), Times.Once);
    }
}
