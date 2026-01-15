using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos_API.Controllers;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Queries;
using Xunit;

namespace Pagos_Test.API.Controlador;

public class MetodosPagoControladorTests
{
    [Fact]
    public async Task ObtenerPorUsuario_deberia_retornar_ok()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var lista = new List<MetodoPagoDto>
        {
            new("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028)
        };

        mediator
            .Setup(m => m.Send(It.IsAny<ObtenerMetodosPagoPorUsuarioQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        var controlador = new MetodosPagoControlador(mediator.Object);

        // Acción
        var resultado = await controlador.ObtenerPorUsuario("u1", CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(lista, ok.Value);
    }

    [Fact]
    public async Task Eliminar_deberia_retornar_ok_cuando_elimina()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EliminarMetodoPagoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controlador = new MetodosPagoControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Eliminar("u1", "pm_1", CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Eliminar_deberia_retornar_notfound_cuando_no_existe()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EliminarMetodoPagoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controlador = new MetodosPagoControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Eliminar("u1", "pm_1", CancellationToken.None);

        // Verificación
        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Eliminar_deberia_retornar_badrequest_cuando_excepcion()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EliminarMetodoPagoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El idUsuario es obligatorio."));

        var controlador = new MetodosPagoControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Eliminar("", "pm_1", CancellationToken.None);

        // Verificación
        Assert.IsType<BadRequestObjectResult>(resultado);
    }
}
