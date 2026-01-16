using Moq;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Handlers.Commands;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Commands;

public class AgregarMetodoPagoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_guardar_metodo_y_retornar_dto()
    {
        // Preparación
        var repoMetodos = new Mock<IRepositorioMetodosPago>();
        var servicioStripe = new Mock<IServicioStripe>();

        repoMetodos
            .Setup(r => r.ObtenerIdStripeCustomerPorUsuarioAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        servicioStripe
            .Setup(s => s.AgregarMetodoPagoAsync(null, "user@correo.com", "pm_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DatosMetodoPagoStripeDto("cus_1", "pm_1", "visa", "4242", 12, 2030));

        var handler = new AgregarMetodoPagoHandler(repoMetodos.Object, servicioStripe.Object);
        var cmd = new AgregarMetodoPagoCommand("u1", "user@correo.com", "pm_1");

        // Acción
        var resultado = await handler.Handle(cmd, CancellationToken.None);

        // Verificación
        Assert.Equal("u1", resultado.IdUsuario);
        Assert.Equal("cus_1", resultado.IdStripeCustomer);
        Assert.Equal("pm_1", resultado.IdStripePaymentMethod);

        repoMetodos.Verify(r => r.CrearAsync(It.IsAny<MetodoPagoGuardado>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
