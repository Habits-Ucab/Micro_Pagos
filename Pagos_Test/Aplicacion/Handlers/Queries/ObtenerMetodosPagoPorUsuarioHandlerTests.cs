using System.Reflection;
using Moq;
using Pagos_Aplicacion.Handlers.Queries;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;
using Pagos_Dominio.Entidades;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Queries;

public class ObtenerMetodosPagoPorUsuarioHandlerTests
{
    [Fact]
    public async Task Handle_deberia_ordenar_por_fecha_descendente()
    {
        // Preparación
        var repo = new Mock<IRepositorioMetodosPago>();

        var metodoAntiguo = new MetodoPagoGuardado("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028);
        var metodoNuevo = new MetodoPagoGuardado("u1", "cus_1", "pm_2", "mc", "5454", 10, 2029);

        // Ajustamos fechas para validar el orden
        var prop = typeof(MetodoPagoGuardado).GetProperty("FechaRegistroUtc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(metodoAntiguo, DateTimeOffset.UtcNow.AddDays(-2));
        prop?.SetValue(metodoNuevo, DateTimeOffset.UtcNow.AddDays(-1));

        repo.Setup(r => r.ObtenerPorUsuarioAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MetodoPagoGuardado> { metodoAntiguo, metodoNuevo });

        var handler = new ObtenerMetodosPagoPorUsuarioHandler(repo.Object);

        // Acción
        var resultado = await handler.Handle(new ObtenerMetodosPagoPorUsuarioQuery("u1"), CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado.Count);
        Assert.Equal("pm_2", resultado[0].IdStripePaymentMethod);
        Assert.Equal("pm_1", resultado[1].IdStripePaymentMethod);
    }
}
