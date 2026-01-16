using Moq;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Handlers.Commands;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Reservas.Aplicacion.Eventos;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Commands;

public class SincronizarPagoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_lanzar_excepcion_si_pago_no_existe()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        repoPagos
            .Setup(r => r.ObtenerPorIdAsync("pago_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pago?)null);

        var handler = new SincronizarPagoHandler(
            repoPagos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        // Acción y verificación
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new SincronizarPagoCommand("pago_1"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_deberia_confirmar_y_generar_factura_si_stripe_succeeded()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        var idReserva = Guid.NewGuid().ToString();
        var pago = new Pago(idReserva, "u1", "evento_1", 100m, "usd");
        pago.AsociarStripePaymentIntent("pi_1");
        pago.AsociarStripePaymentMethod("pm_1");

        repoPagos
            .Setup(r => r.ObtenerPorIdAsync(pago.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pago);

        servicioStripe
            .Setup(s => s.ObtenerEstadoPaymentIntentAsync("pi_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("succeeded");

        repoFacturas
            .Setup(r => r.ObtenerPorIdPagoAsync(pago.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FacturaDigital?)null);

        servicioFacturas
            .Setup(s => s.GenerarFacturaPdf(It.IsAny<Pago>()))
            .Returns(new byte[] { 9, 9, 9 });

        var handler = new SincronizarPagoHandler(
            repoPagos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        // Acción
        var resultado = await handler.Handle(new SincronizarPagoCommand(pago.Id), CancellationToken.None);

        // Verificación
        Assert.Equal(EstadoPago.Confirmado, resultado.Estado);
        repoPagos.Verify(r => r.ActualizarAsync(It.IsAny<Pago>(), It.IsAny<CancellationToken>()), Times.Once);
        repoFacturas.Verify(r => r.CrearAsync(It.IsAny<FacturaDigital>(), It.IsAny<CancellationToken>()), Times.Once);
        publicador.Verify(p => p.PublicarPagoConfirmadoReservaAsync(It.Is<PagoConfirmadoEvent>(e => e.ReservaId == Guid.Parse(idReserva)), It.IsAny<CancellationToken>()), Times.Once);
    }
}
