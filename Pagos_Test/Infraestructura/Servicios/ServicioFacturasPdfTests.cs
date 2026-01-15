using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.Servicios;
using QuestPDF.Infrastructure;
using Xunit;

namespace Pagos_Test.Infraestructura.Servicios;

public class ServicioFacturasPdfTests
{
    [Fact]
    public void GenerarFacturaPdf_deberia_generar_contenido()
    {
        // Preparación
        QuestPDF.Settings.License = LicenseType.Community;
        var servicio = new ServicioFacturasPdf();
        var pago = new Pago("res_1", "u1", "evento_1", 100m, "usd");

        // Acción
        var bytes = servicio.GenerarFacturaPdf(pago);

        // Verificación
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }
}
