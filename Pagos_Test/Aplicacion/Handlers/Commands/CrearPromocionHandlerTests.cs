using Moq;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Handlers.Commands;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Enums;
using Pagos_Dominio.Excepciones;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Commands;

public class CrearPromocionHandlerTests
{
    [Fact]
    public async Task Handle_deberia_lanzar_excepcion_si_codigo_existe()
    {
        // Preparación
        var repoPromos = new Mock<IRepositorioPromociones>();
        repoPromos
            .Setup(r => r.ObtenerPorCodigoYEventoAsync("evento_1", "PROMO10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pagos_Dominio.Entidades.Promocion(
                "evento_1",
                "PROMO10",
                TipoPromocion.Porcentaje,
                10m,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(1),
                5));

        var handler = new CrearPromocionHandler(repoPromos.Object);

        var cmd = new CrearPromocionCommand(
            "evento_1",
            "PROMO10",
            TipoPromocion.Porcentaje,
            10m,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            5);

        // Acción y verificación
        await Assert.ThrowsAsync<PromocionNoValidaExcepcion>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_deberia_crear_promocion_y_retornar_dto()
    {
        // Preparación
        var repoPromos = new Mock<IRepositorioPromociones>();
        repoPromos
            .Setup(r => r.ObtenerPorCodigoYEventoAsync("evento_1", "PROMO10", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pagos_Dominio.Entidades.Promocion?)null);

        var handler = new CrearPromocionHandler(repoPromos.Object);

        var cmd = new CrearPromocionCommand(
            "evento_1",
            "PROMO10",
            TipoPromocion.Porcentaje,
            10m,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            5);

        // Acción
        var resultado = await handler.Handle(cmd, CancellationToken.None);

        // Verificación
        Assert.Equal("PROMO10", resultado.Codigo);
        Assert.Equal("evento_1", resultado.IdEvento);
        repoPromos.Verify(r => r.CrearAsync(It.IsAny<Pagos_Dominio.Entidades.Promocion>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
