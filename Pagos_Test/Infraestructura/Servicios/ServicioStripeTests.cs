using Pagos_Infraestructura.Servicios;
using Xunit;

namespace Pagos_Test.Infraestructura.Servicios;

public class ServicioStripeTests
{
    [Fact]
    public async Task AgregarMetodoPagoAsync_deberia_lanzar_excepcion_si_email_vacio()
    {
        // Preparación
        var servicio = new ServicioStripe();

        // Acción y verificación
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.AgregarMetodoPagoAsync(null, "", "pm_1", CancellationToken.None));
    }
}
