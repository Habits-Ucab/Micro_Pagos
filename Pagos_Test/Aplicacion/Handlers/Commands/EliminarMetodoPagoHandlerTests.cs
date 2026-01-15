using Moq;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Handlers.Commands;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Commands;

public class EliminarMetodoPagoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_lanzar_excepcion_si_id_usuario_vacio()
    {
        // Preparación
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var servicioStripe = new Mock<IServicioStripe>();
        var handler = new EliminarMetodoPagoHandler(repoMetodos.Object, servicioStripe.Object);

        // Acción y verificación
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new EliminarMetodoPagoCommand("", "pm_1"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_deberia_retornar_false_si_metodo_no_existe()
    {
        // Preparación
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var servicioStripe = new Mock<IServicioStripe>();

        repoMetodos
            .Setup(r => r.ObtenerPorUsuarioAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetodoPagoGuardado>());

        var handler = new EliminarMetodoPagoHandler(repoMetodos.Object, servicioStripe.Object);

        // Acción
        var resultado = await handler.Handle(new EliminarMetodoPagoCommand("u1", "pm_1"), CancellationToken.None);

        // Verificación
        Assert.False(resultado);
        servicioStripe.Verify(s => s.DesasociarMetodoPagoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repoMetodos.Verify(r => r.EliminarPorUsuarioYPaymentMethodAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_deberia_eliminar_y_reasignar_default_cuando_corresponde()
    {
        // Preparación
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var servicioStripe = new Mock<IServicioStripe>();

        var metodo = new MetodoPagoGuardado("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028);
        var metodoRestante = new MetodoPagoGuardado("u1", "cus_1", "pm_2", "mc", "5454", 10, 2029);

        repoMetodos
            .SetupSequence(r => r.ObtenerPorUsuarioAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetodoPagoGuardado> { metodo })
            .ReturnsAsync(new List<MetodoPagoGuardado> { metodoRestante });

        repoMetodos
            .Setup(r => r.EliminarPorUsuarioYPaymentMethodAsync("u1", "pm_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new EliminarMetodoPagoHandler(repoMetodos.Object, servicioStripe.Object);

        // Acción
        var resultado = await handler.Handle(new EliminarMetodoPagoCommand("u1", "pm_1"), CancellationToken.None);

        // Verificación
        Assert.True(resultado);
        servicioStripe.Verify(s => s.DesasociarMetodoPagoAsync("pm_1", It.IsAny<CancellationToken>()), Times.Once);
        servicioStripe.Verify(s => s.EstablecerMetodoPagoPorDefectoAsync("cus_1", "pm_2", It.IsAny<CancellationToken>()), Times.Once);
    }
}
