using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pagos_API.Controllers;
using Pagos_API.Modelos.Promociones;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Queries;
using Pagos_Dominio.Enums;
using Xunit;

namespace Pagos_Test.API.Controlador;

public class PromocionesControladorTests
{
    [Fact]
    public async Task Crear_deberia_enviar_comando_y_retornar_ok()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var solicitud = new CrearPromocionSolicitud
        {
            IdEvento = "evento_1",
            Codigo = "PROMO10",
            Tipo = TipoPromocion.Porcentaje,
            Valor = 10m,
            FechaInicioUtc = DateTimeOffset.UtcNow.AddDays(-1),
            FechaFinUtc = DateTimeOffset.UtcNow.AddDays(1),
            StockUsos = 10
        };

        var dto = new PromocionDto("promo_1", "evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m,
            solicitud.FechaInicioUtc, solicitud.FechaFinUtc, 10, 0, true);

        mediator.Setup(m => m.Send(It.IsAny<CrearPromocionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controlador = new PromocionesControlador(mediator.Object);

        // Acción
        var resultado = await controlador.Crear(solicitud, CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(dto, ok.Value);
        mediator.Verify(m => m.Send(It.Is<CrearPromocionCommand>(c =>
            c.IdEvento == solicitud.IdEvento &&
            c.Codigo == solicitud.Codigo &&
            c.Tipo == solicitud.Tipo &&
            c.Valor == solicitud.Valor &&
            c.FechaInicioUtc == solicitud.FechaInicioUtc &&
            c.FechaFinUtc == solicitud.FechaFinUtc &&
            c.StockUsos == solicitud.StockUsos),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorEvento_deberia_retornar_ok()
    {
        // Preparación
        var mediator = new Mock<IMediator>();
        var lista = new List<PromocionDto>
        {
            new("promo_1", "evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m,
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 10, 0, true)
        };

        mediator.Setup(m => m.Send(It.IsAny<ObtenerPromocionesPorEventoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lista);

        var controlador = new PromocionesControlador(mediator.Object);

        // Acción
        var resultado = await controlador.ObtenerPorEvento("evento_1", true, CancellationToken.None);

        // Verificación
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(lista, ok.Value);
    }
}
