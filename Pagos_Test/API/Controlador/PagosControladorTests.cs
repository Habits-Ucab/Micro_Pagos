using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos_API.Controllers;
using Pagos_API.Modelos.Pagos;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Queries;
using Pagos_Dominio.Enums;
using Xunit;

namespace Pagos_Test.API.Controlador;

public class PagosControladorTests
{
    [Fact]
    public async Task ObtenerPorId_deberia_retornar_ok_cuando_existe()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var dto = new PagoDetalleDto("pago_1", EstadoPago.Confirmado, 100m, "usd", "pi_1", "fac_1", "succeeded");

        mediator
            .Setup(m => m.Send(It.IsAny<ObtenerPagoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controlador = new PagosControlador(mediator.Object);

        // Acción
        var resultado = await controlador.ObtenerPorId("pago_1", CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task ObtenerPorId_deberia_retornar_notfound_cuando_no_existe()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ObtenerPagoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagoDetalleDto?)null);

        var controlador = new PagosControlador(mediator.Object);

        // Acción
        var resultado = await controlador.ObtenerPorId("pago_1", CancellationToken.None);

        // Verificación
        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Sincronizar_deberia_retornar_ok_cuando_exito()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var dto = new PagoDetalleDto("pago_1", EstadoPago.Pendiente, 50m, "usd", "pi_1", null, "processing");

        mediator
            .Setup(m => m.Send(It.IsAny<SincronizarPagoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controlador = new PagosControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Sincronizar("pago_1", CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task Sincronizar_deberia_retornar_notfound_cuando_pago_no_existe()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<SincronizarPagoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Pago no encontrado."));

        var controlador = new PagosControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Sincronizar("pago_1", CancellationToken.None);

        // Verificación
        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Checkout_deberia_enviar_comando_y_retornar_ok()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var solicitud = new CrearPagoSolicitud
        {
            IdReserva = Guid.NewGuid().ToString(),
            IdUsuario = "usuario_1",
            EmailUsuario = "test@correo.com",
            IdEvento = "evento_1",
            Monto = 120m,
            Moneda = "usd",
            IdMetodoPagoStripe = "pm_1",
            CodigoPromocion = "PROMO10"
        };

        var dto = new ResultadoPagoDto("pago_1", EstadoPago.Pendiente, 110m, "usd", true, "secret", null);
        mediator
            .Setup(m => m.Send(It.IsAny<CrearPagoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controlador = new PagosControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Checkout(solicitud, CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(dto, ok.Value);
        mediator.Verify(m => m.Send(It.Is<CrearPagoCommand>(c =>
            c.IdReserva == solicitud.IdReserva &&
            c.IdUsuario == solicitud.IdUsuario &&
            c.EmailUsuario == solicitud.EmailUsuario &&
            c.IdEvento == solicitud.IdEvento &&
            c.Monto == solicitud.Monto &&
            c.Moneda == solicitud.Moneda &&
            c.IdMetodoPagoStripe == solicitud.IdMetodoPagoStripe &&
            c.CodigoPromocion == solicitud.CodigoPromocion),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
