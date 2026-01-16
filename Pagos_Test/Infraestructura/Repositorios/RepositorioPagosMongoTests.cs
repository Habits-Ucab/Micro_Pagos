using MongoDB.Driver;
using Moq;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Infraestructura.Repositorios;
using Xunit;

namespace Pagos_Test.Infraestructura.Repositorios;

public class RepositorioPagosMongoTests
{
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

    private sealed class RepositorioPagosMongoFake : RepositorioPagosMongo
    {
        private readonly IFindFluent<Pago, Pago> _find;

        public RepositorioPagosMongoFake(IMongoCollection<Pago> coleccion, IFindFluent<Pago, Pago> find)
            : base(coleccion)
        {
            _find = find;
        }

        protected override IFindFluent<Pago, Pago> Buscar(FilterDefinition<Pago> filter)
            => _find;
    }

    [Fact]
    public async Task ObtenerPorIdAsync_deberia_retornar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Pago>>();
        var findMock = new Mock<IFindFluent<Pago, Pago>>();

        var pago = new Pago(Guid.NewGuid().ToString(), "u1", "evento_1", 100m, "usd");

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Pago>(new[] { pago }));

        var repo = new RepositorioPagosMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorIdAsync(pago.Id, CancellationToken.None);

        // Verificación
        Assert.NotNull(resultado);
        Assert.Equal(pago.Id, resultado!.Id);
    }

    [Fact]
    public async Task ObtenerPendientesParaConciliacionAsync_deberia_retornar_lista()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Pago>>();
        var findMock = new Mock<IFindFluent<Pago, Pago>>();

        findMock.Setup(f => f.Sort(It.IsAny<SortDefinition<Pago>>())).Returns(findMock.Object);
        findMock.Setup(f => f.Limit(It.IsAny<int>())).Returns(findMock.Object);

        var docs = new List<Pago>
        {
            new(Guid.NewGuid().ToString(), "u1", "evento_1", 50m, "usd"),
            new(Guid.NewGuid().ToString(), "u2", "evento_2", 70m, "usd")
        };

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<Pago>(docs));

        var repo = new RepositorioPagosMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPendientesParaConciliacionAsync(DateTimeOffset.UtcNow, 10, CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task CrearAsync_deberia_insertar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Pago>>();
        var repo = new RepositorioPagosMongo(collectionMock.Object);
        var pago = new Pago(Guid.NewGuid().ToString(), "u1", "evento_1", 100m, "usd");

        // Acción
        await repo.CrearAsync(pago, CancellationToken.None);

        // Verificación
        collectionMock.Verify(c => c.InsertOneAsync(It.IsAny<Pago>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_deberia_reemplazar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<Pago>>();
        var repo = new RepositorioPagosMongo(collectionMock.Object);
        var pago = new Pago(Guid.NewGuid().ToString(), "u1", "evento_1", 100m, "usd");
        pago.MarcarConfirmado();

        // Acción
        await repo.ActualizarAsync(pago, CancellationToken.None);

        // Verificación
        collectionMock.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Pago>>(), It.IsAny<Pago>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
