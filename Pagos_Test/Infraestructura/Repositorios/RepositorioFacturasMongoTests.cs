using MongoDB.Driver;
using Moq;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.Repositorios;
using Xunit;

namespace Pagos_Test.Infraestructura.Repositorios;

public class RepositorioFacturasMongoTests
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

    private sealed class RepositorioFacturasMongoFake : RepositorioFacturasMongo
    {
        private readonly IFindFluent<FacturaDigital, FacturaDigital> _find;

        public RepositorioFacturasMongoFake(IMongoCollection<FacturaDigital> coleccion, IFindFluent<FacturaDigital, FacturaDigital> find)
            : base(coleccion)
        {
            _find = find;
        }

        protected override IFindFluent<FacturaDigital, FacturaDigital> Buscar(FilterDefinition<FacturaDigital> filter)
            => _find;
    }

    [Fact]
    public async Task ObtenerPorIdPagoAsync_deberia_retornar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<FacturaDigital>>();
        var findMock = new Mock<IFindFluent<FacturaDigital, FacturaDigital>>();

        var factura = new FacturaDigital("pago_1", "factura.pdf", new byte[] { 1 });

        findMock
            .Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeAsyncCursor<FacturaDigital>(new[] { factura }));

        var repo = new RepositorioFacturasMongoFake(collectionMock.Object, findMock.Object);

        // Acción
        var resultado = await repo.ObtenerPorIdPagoAsync("pago_1", CancellationToken.None);

        // Verificación
        Assert.NotNull(resultado);
        Assert.Equal("pago_1", resultado!.IdPago);
    }

    [Fact]
    public async Task CrearAsync_deberia_insertar_documento()
    {
        // Preparación
        var collectionMock = new Mock<IMongoCollection<FacturaDigital>>();
        var repo = new RepositorioFacturasMongo(collectionMock.Object);
        var factura = new FacturaDigital("pago_1", "factura.pdf", new byte[] { 1, 2 });

        // Acción
        await repo.CrearAsync(factura, CancellationToken.None);

        // Verificación
        collectionMock.Verify(c => c.InsertOneAsync(It.IsAny<FacturaDigital>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
