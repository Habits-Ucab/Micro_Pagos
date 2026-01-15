using Moq;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.HangFire;
using Reservas.Aplicacion.Eventos;
using Xunit;

namespace Pagos_Test.Infraestructura.HangFire;

public class TrabajosProgramadosTests
{
    [Fact]
    public async Task ExpirarPromocionesAsync_deberia_llamar_repositorio()
    {
        // Preparación
        var repoPromos = new Mock<IRepositorioPromociones>();
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoFacturas = new Mock<IRepositorioFacturas>();
        var servicioStripe = new Mock<IServicioStripe>();
        var servicioFacturas = new Mock<IServicioFacturas>();
        var publicador = new Mock<IPublicadorEventos>();

        var trabajos = new TrabajosProgramados(
            repoPromos.Object,
            repoPagos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        // Acción
        await trabajos.ExpirarPromocionesAsync();

        // Verificación
        repoPromos.Verify(r => r.DesactivarExpiradasAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConciliarPagosAsync_deberia_confirmar_y_generar_factura_cuando_succeeded()
    {
        // Preparación
        var repoPromos = new Mock<IRepositorioPromociones>();
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
            .Setup(r => r.ObtenerPendientesParaConciliacionAsync(It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Pago> { pago });

        servicioStripe
            .Setup(s => s.ObtenerEstadoPaymentIntentAsync("pi_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("succeeded");

        repoFacturas
            .Setup(r => r.ObtenerPorIdPagoAsync(pago.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FacturaDigital?)null);

        servicioFacturas
            .Setup(s => s.GenerarFacturaPdf(It.IsAny<Pago>()))
            .Returns(new byte[] { 1, 2, 3 });

        var trabajos = new TrabajosProgramados(
            repoPromos.Object,
            repoPagos.Object,
            repoFacturas.Object,
            servicioStripe.Object,
            servicioFacturas.Object,
            publicador.Object);

        // Acción
        await trabajos.ConciliarPagosAsync();

        // Verificación
        repoPagos.Verify(r => r.ActualizarAsync(It.IsAny<Pago>(), It.IsAny<CancellationToken>()), Times.Once);
        repoFacturas.Verify(r => r.CrearAsync(It.IsAny<FacturaDigital>(), It.IsAny<CancellationToken>()), Times.Once);
        publicador.Verify(p => p.PublicarPagoConfirmadoReservaAsync(It.Is<PagoConfirmadoEvent>(e => e.ReservaId == Guid.Parse(idReserva)), It.IsAny<CancellationToken>()), Times.Once);
    }
}
