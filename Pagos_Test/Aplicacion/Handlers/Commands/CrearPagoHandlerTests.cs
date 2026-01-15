using Moq;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Handlers.Commands;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Dominio.Excepciones;
using Reservas.Aplicacion.Eventos;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Commands;

public class CrearPagoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_lanzar_excepcion_si_promocion_no_existe()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoPromos = new Mock<IRepositorioPromociones>();
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        repoPromos
            .Setup(r => r.ObtenerPorCodigoYEventoAsync("evento_1", "PROMO10", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Promocion?)null);

        var handler = new CrearPagoHandler(
            repoPagos.Object,
            repoPromos.Object,
            repoMetodos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        var cmd = new CrearPagoCommand(
            Guid.NewGuid().ToString(),
            "usuario_1",
            "test@correo.com",
            "evento_1",
            100m,
            "usd",
            "pm_1",
            "PROMO10");

        // Acción y verificación
        await Assert.ThrowsAsync<PromocionNoValidaExcepcion>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_deberia_confirmar_pago_y_publicar_evento_si_stripe_succeeded()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoPromos = new Mock<IRepositorioPromociones>();
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        var idReserva = Guid.NewGuid().ToString();

        repoMetodos
            .Setup(r => r.ObtenerIdStripeCustomerPorUsuarioAsync("usuario_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cus_1");

        repoMetodos
            .Setup(r => r.ObtenerPorUsuarioAsync("usuario_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetodoPagoGuardado>
            {
                new("usuario_1", "cus_1", "pm_1", "visa", "4242", 12, 2028)
            });

        servicioStripe
            .Setup(s => s.CrearYConfirmarPagoAsync("cus_1", "pm_1", It.IsAny<long>(), "usd", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoIntentoPagoStripeDto("pi_1", "succeeded", null));

        servicioFacturas
            .Setup(s => s.GenerarFacturaPdf(It.IsAny<Pago>()))
            .Returns(new byte[] { 1, 2, 3 });

        var handler = new CrearPagoHandler(
            repoPagos.Object,
            repoPromos.Object,
            repoMetodos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        var cmd = new CrearPagoCommand(
            idReserva,
            "usuario_1",
            "test@correo.com",
            "evento_1",
            100m,
            "usd",
            "pm_1",
            null);

        // Acción
        var resultado = await handler.Handle(cmd, CancellationToken.None);

        // Verificación
        Assert.Equal(EstadoPago.Confirmado, resultado.Estado);
        Assert.False(resultado.RequiereAccion);
        Assert.NotNull(resultado.IdFactura);

        repoPagos.Verify(r => r.CrearAsync(It.IsAny<Pago>(), It.IsAny<CancellationToken>()), Times.Once);
        repoPagos.Verify(r => r.ActualizarAsync(It.IsAny<Pago>(), It.IsAny<CancellationToken>()), Times.Once);
        repoFacturas.Verify(r => r.CrearAsync(It.IsAny<FacturaDigital>(), It.IsAny<CancellationToken>()), Times.Once);
        servicioStripe.Verify(s => s.AgregarMetodoPagoAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        publicador.Verify(p => p.PublicarPagoConfirmadoReservaAsync(It.Is<PagoConfirmadoEvent>(e => e.ReservaId == Guid.Parse(idReserva)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_deberia_indicar_requiere_accion_cuando_stripe_lo_pide()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoPromos = new Mock<IRepositorioPromociones>();
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        repoMetodos
            .Setup(r => r.ObtenerIdStripeCustomerPorUsuarioAsync("usuario_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cus_1");

        repoMetodos
            .Setup(r => r.ObtenerPorUsuarioAsync("usuario_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetodoPagoGuardado>
            {
                new("usuario_1", "cus_1", "pm_1", "visa", "4242", 12, 2028)
            });

        servicioStripe
            .Setup(s => s.CrearYConfirmarPagoAsync("cus_1", "pm_1", It.IsAny<long>(), "usd", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoIntentoPagoStripeDto("pi_1", "requires_action", "secret"));

        var handler = new CrearPagoHandler(
            repoPagos.Object,
            repoPromos.Object,
            repoMetodos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        var cmd = new CrearPagoCommand(
            Guid.NewGuid().ToString(),
            "usuario_1",
            "test@correo.com",
            "evento_1",
            100m,
            "usd",
            "pm_1",
            null);

        // Acción
        var resultado = await handler.Handle(cmd, CancellationToken.None);

        // Verificación
        Assert.True(resultado.RequiereAccion);
        Assert.Equal("secret", resultado.ClientSecret);
        repoFacturas.Verify(r => r.CrearAsync(It.IsAny<FacturaDigital>(), It.IsAny<CancellationToken>()), Times.Never);
        publicador.Verify(p => p.PublicarPagoConfirmadoReservaAsync(It.IsAny<PagoConfirmadoEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
