using MongoDB.Driver;
using Moq;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.Repositorios;
using Xunit;

namespace Pagos_Test.Infraestructura.Repositorios;

public class RepositorioMetodosPagoMongoTests
{
    private sealed class FakeDeleteResult : DeleteResult
    {
        private readonly long _deletedCount;

        public FakeDeleteResult(long deletedCount)
        {
            _deletedCount = deletedCount;
        }

        public override bool IsAcknowledged => true;
        public override long DeletedCount => _deletedCount;
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

    private sealed class RepositorioMetodosPagoMongoFake : RepositorioMetodosPagoMongo
    {
        private readonly IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado> _find;

        public RepositorioMetodosPagoMongoFake(IMongoCollection<MetodoPagoGuardado> coleccion, IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado> find)
            : base(coleccion)
        {
            _find = find;
        }

        protected override IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado> Buscar(FilterDefinition<MetodoPagoGuardado> filter)
            => _find;
    }

    [Fact]
    public async Task ObtenerPorUsuarioAsync_deberia_retornar_lista()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<MetodoPagoGuardado>>();
        var findMock = new Mock<IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado>>();

        var docs = new List<MetodoPagoGuardado>
        {
            new("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028),
            new("u1", "cus_1", "pm_2", "mc", "5454", 10, 2029)
        };

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<MetodoPagoGuardado>(docs));

        var repo = new RepositorioMetodosPagoMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorUsuarioAsync("u1", CancellationToken.None);

        // Verificación
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task ObtenerIdStripeCustomerPorUsuarioAsync_deberia_retornar_ultimo_customer()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<MetodoPagoGuardado>>();
        var findMock = new Mock<IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado>>();

        findMock.Setup(f => f.Sort(It.IsAny<SortDefinition<MetodoPagoGuardado>>())).Returns(findMock.Object);
        findMock.Setup(f => f.Limit(It.IsAny<int>())).Returns(findMock.Object);

        var doc = new MetodoPagoGuardado("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028);

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<MetodoPagoGuardado>(new[] { doc }));

        var repo = new RepositorioMetodosPagoMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerIdStripeCustomerPorUsuarioAsync("u1", CancellationToken.None);

        // Verificación
        Assert.Equal("cus_1", resultado);
    }

    [Fact]
    public async Task CrearAsync_deberia_insertar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<MetodoPagoGuardado>>();
        var repo = new RepositorioMetodosPagoMongo(collectionMock.Object);
        var metodo = new MetodoPagoGuardado("u1", "cus_1", "pm_1", "visa", "4242", 12, 2028);

        // Acción
        await repo.CrearAsync(metodo, CancellationToken.None);

        // Verificación
        collectionMock.Verify(c => c.InsertOneAsync(It.IsAny<MetodoPagoGuardado>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarPorUsuarioYPaymentMethodAsync_deberia_retornar_true_si_elimina()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<MetodoPagoGuardado>>();
        collectionMock
            .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<MetodoPagoGuardado>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeDeleteResult(1));

        var repo = new RepositorioMetodosPagoMongo(collectionMock.Object);

        // Acción
        var resultado = await repo.EliminarPorUsuarioYPaymentMethodAsync("u1", "pm_1", CancellationToken.None);

        // Verificación
        Assert.True(resultado);
    }
}
