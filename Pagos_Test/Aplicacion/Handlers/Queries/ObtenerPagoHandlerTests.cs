using Moq;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Handlers.Queries;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Queries;

public class ObtenerPagoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_retornar_null_si_pago_no_existe()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoFacturas = new Mock<IRepositorioFacturas>();

        repoPagos
            .Setup(r => r.ObtenerPorIdAsync("pago_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pago?)null);

        var handler = new ObtenerPagoHandler(repoPagos.Object, repoFacturas.Object);

        // Acción
        var resultado = await handler.Handle(new ObtenerPagoQuery("pago_1"), CancellationToken.None);

        // Verificación
        Assert.Null(resultado);
    }

    [Fact]
    public async Task Handle_deberia_retornar_detalle_con_factura()
    {
        // Preparación
        var repoPagos = new Mock<IRepositorioPagos>();
        var repoFacturas = new Mock<IRepositorioFacturas>();

        var pago = new Pago("res_1", "u1", "evento_1", 100m, "usd");
        pago.AsociarStripePaymentIntent("pi_1");

        repoPagos
            .Setup(r => r.ObtenerPorIdAsync(pago.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pago);

        repoFacturas
            .Setup(r => r.ObtenerPorIdPagoAsync(pago.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FacturaDigital(pago.Id, "factura.pdf", new byte[] { 1 }));

        var handler = new ObtenerPagoHandler(repoPagos.Object, repoFacturas.Object);

        // Acción
        var resultado = await handler.Handle(new ObtenerPagoQuery(pago.Id), CancellationToken.None);

        // Verificación
        Assert.NotNull(resultado);
        Assert.Equal(pago.Id, resultado!.IdPago);
        Assert.Equal(EstadoPago.Pendiente, resultado.Estado);
        Assert.NotNull(resultado.IdFactura);
    }
}
