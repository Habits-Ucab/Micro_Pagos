using Moq;
using Pagos_Aplicacion.Handlers.Queries;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Xunit;

namespace Pagos_Test.Aplicacion.Handlers.Queries;

public class ObtenerPromocionesPorEventoHandlerTests
{
    [Fact]
    public async Task Handle_deberia_filtrar_solo_vigentes_con_stock()
    {
        // Preparación
        var repo = new Mock<IRepositorioPromociones>();
        var ahora = DateTimeOffset.UtcNow;

        var promoVigente = new Promocion("evento_1", "VIG", TipoPromocion.Porcentaje, 10m, ahora.AddHours(-1), ahora.AddHours(1), 2);
        var promoSinStock = new Promocion("evento_1", "SIN", TipoPromocion.MontoFijo, 5m, ahora.AddHours(-1), ahora.AddHours(1), 1);
        promoSinStock.RegistrarUso();

        var promoNoVigente = new Promocion("evento_1", "NOVIG", TipoPromocion.MontoFijo, 5m, ahora.AddHours(-3), ahora.AddHours(-1), 2);

        repo.Setup(r => r.ObtenerPorEventoAsync("evento_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Promocion> { promoVigente, promoSinStock, promoNoVigente });

        var handler = new ObtenerPromocionesPorEventoHandler(repo.Object);

        // Acción
        var resultado = await handler.Handle(new ObtenerPromocionesPorEventoQuery("evento_1", true), CancellationToken.None);

        // Verificación
        Assert.Single(resultado);
        Assert.Equal("VIG", resultado[0].Codigo);
    }

    [Fact]
    public async Task Handle_deberia_retornar_todas_y_ordenar_por_activa_y_fecha()
    {
        // Preparación
        var repo = new Mock<IRepositorioPromociones>();
        var ahora = DateTimeOffset.UtcNow;

        var promoActiva = new Promocion("evento_1", "ACT", TipoPromocion.Porcentaje, 10m, ahora.AddHours(-1), ahora.AddHours(3), 2);
        var promoInactiva = new Promocion("evento_1", "INA", TipoPromocion.MontoFijo, 5m, ahora.AddHours(-1), ahora.AddHours(2), 2);
        promoInactiva.Desactivar();

        repo.Setup(r => r.ObtenerPorEventoAsync("evento_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Promocion> { promoInactiva, promoActiva });

        var handler = new ObtenerPromocionesPorEventoHandler(repo.Object);

        // Acción
        var resultado = await handler.Handle(new ObtenerPromocionesPorEventoQuery("evento_1", false), CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado.Count);
        Assert.Equal("ACT", resultado[0].Codigo);
        Assert.Equal("INA", resultado[1].Codigo);
    }
}
