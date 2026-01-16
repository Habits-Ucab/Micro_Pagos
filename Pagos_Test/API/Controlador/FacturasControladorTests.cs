using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos_API.Controllers;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Xunit;

namespace Pagos_Test.API.Controlador;

public class FacturasControladorTests
{
    [Fact]
    public async Task DescargarPorPago_deberia_retornar_notfound_si_no_existe()
    {
        // Preparación
        var repo = new Mock<IRepositorioFacturas>();
        repo.Setup(r => r.ObtenerPorIdPagoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FacturaDigital?)null);

        var controlador = new FacturasControlador(repo.Object);

        // Acción
        var resultado = await controlador.DescargarPorPago("pago_1", CancellationToken.None);

        // Verificación
        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task DescargarPorPago_deberia_retornar_archivo_pdf()
    {
        // Preparación
        var repo = new Mock<IRepositorioFacturas>();
        var factura = new FacturaDigital("pago_1", "factura.pdf", new byte[] { 1, 2, 3 });

        repo.Setup(r => r.ObtenerPorIdPagoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);

        var controlador = new FacturasControlador(repo.Object);

        // Acción
        var resultado = await controlador.DescargarPorPago("pago_1", CancellationToken.None);

        // Verificación
        var archivo = Assert.IsType<FileContentResult>(resultado);
        Assert.Equal("application/pdf", archivo.ContentType);
        Assert.Equal("factura.pdf", archivo.FileDownloadName);
    }
}
