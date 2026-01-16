using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Infraestructura.Repositorios;
using Xunit;

namespace Pagos_Test.Infraestructura.Repositorios;

public class RepositorioPromocionesMongoTests
{
    private sealed class FakeUpdateResult : UpdateResult
    {
        private readonly long _matchedCount;
        private readonly long _modifiedCount;

        public FakeUpdateResult(long matchedCount, long modifiedCount)
        {
            _matchedCount = matchedCount;
            _modifiedCount = modifiedCount;
        }

        public override bool IsAcknowledged => true;
        public override bool IsModifiedCountAvailable => true;
        public override long MatchedCount => _matchedCount;
        public override long ModifiedCount => _modifiedCount;
        public override BsonValue? UpsertedId => null;
    }

    private sealed class FakeAsyncCursor<T> : IAsyncCursor<T>
    {
        private readonly List<T> _items;
        private bool _moved;

        public FakeAsyncCursor(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        public IEnumerable<T> Current { get; private set; } = Enumerable.Empty<T>();

        public bool MoveNext(CancellationToken cancellationToken = default)
        {
            if (_moved)
                return false;

            _moved = true;
            Current = _items;
            return true;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(MoveNext(cancellationToken));

        public void Dispose() { }
    }

    private sealed class RepositorioPromocionesMongoFake : RepositorioPromocionesMongo
    {
        private readonly IFindFluent<Promocion, Promocion> _find;

        public RepositorioPromocionesMongoFake(IMongoCollection<Promocion> coleccion, IFindFluent<Promocion, Promocion> find)
            : base(coleccion)
        {
            _find = find;
        }

        protected override IFindFluent<Promocion, Promocion> Buscar(FilterDefinition<Promocion> filter)
            => _find;
    }

    [Fact]
    public async Task ObtenerPorIdAsync_deberia_retornar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        var findMock = new Mock<IFindFluent<Promocion, Promocion>>();

        var promo = new Promocion("evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 5);

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Promocion>(new[] { promo }));

        var repo = new RepositorioPromocionesMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorIdAsync(promo.Id, CancellationToken.None);

        // Verificación
        Assert.NotNull(resultado);
        Assert.Equal(promo.Id, resultado!.Id);
    }

    [Fact]
    public async Task ObtenerPorCodigoYEventoAsync_deberia_normalizar_codigo()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        var findMock = new Mock<IFindFluent<Promocion, Promocion>>();

        var promo = new Promocion("evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 5);

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Promocion>(new[] { promo }));

        var repo = new RepositorioPromocionesMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorCodigoYEventoAsync("evento_1", "  promo10 ", CancellationToken.None);

        // Verificación
        Assert.NotNull(resultado);
        Assert.Equal("PROMO10", resultado!.Codigo);
    }

    [Fact]
    public async Task ObtenerPorEventoAsync_deberia_retornar_lista()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        var findMock = new Mock<IFindFluent<Promocion, Promocion>>();

        var promos = new List<Promocion>
        {
            new("evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 5),
            new("evento_1", "PROMO20", TipoPromocion.MontoFijo, 20m, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 5)
        };

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Promocion>(promos));

        var repo = new RepositorioPromocionesMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorEventoAsync("evento_1", CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task IntentarRegistrarUsoAsync_deberia_retornar_false_si_no_existe()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        var findMock = new Mock<IFindFluent<Promocion, Promocion>>();

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Promocion>(Array.Empty<Promocion>()));

        var repo = new RepositorioPromocionesMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.IntentarRegistrarUsoAsync("promo_1", CancellationToken.None);

        // Verificación
        Assert.False(resultado);
    }

    [Fact]
    public async Task IntentarRegistrarUsoAsync_deberia_incrementar_y_desactivar_si_agota_stock()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        var findMock = new Mock<IFindFluent<Promocion, Promocion>>();

        var promo = new Promocion("evento_1", "PROMO10", TipoPromocion.Porcentaje, 10m, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), 1);

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Promocion>(new[] { promo }));

        collectionMock
            .SetupSequence(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Promocion>>(),
                It.IsAny<UpdateDefinition<Promocion>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeUpdateResult(1, 1))
            .ReturnsAsync(new FakeUpdateResult(1, 1));

        var repo = new RepositorioPromocionesMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.IntentarRegistrarUsoAsync(promo.Id, CancellationToken.None);

        // Verificación
        Assert.True(resultado);
        collectionMock.Verify(c => c.UpdateOneAsync(
            It.IsAny<FilterDefinition<Promocion>>(),
            It.IsAny<UpdateDefinition<Promocion>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DesactivarExpiradasAsync_deberia_retornar_modificadas()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Promocion>>();
        collectionMock
            .Setup(c => c.UpdateManyAsync(
                It.IsAny<FilterDefinition<Promocion>>(),
                It.IsAny<UpdateDefinition<Promocion>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeUpdateResult(2, 2));

        var repo = new RepositorioPromocionesMongo(collectionMock.Object);

        // Acción
        var resultado = await repo.DesactivarExpiradasAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado);
    }
}
