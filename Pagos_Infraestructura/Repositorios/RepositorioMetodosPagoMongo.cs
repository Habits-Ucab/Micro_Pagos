using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioMetodosPagoMongo : IRepositorioMetodosPago
{
    private readonly IMongoCollection<MetodoPagoGuardado> _coleccion;

    public RepositorioMetodosPagoMongo(MongoContexto contexto)
    {
        _coleccion = contexto.MetodosPago;
    }

    /// <summary>
    /// Constructor alternativo para pruebas, permite inyectar colección simulada.
    /// </summary>
    public RepositorioMetodosPagoMongo(IMongoCollection<MetodoPagoGuardado> coleccion)
    {
        _coleccion = coleccion;
    }

    public Task CrearAsync(MetodoPagoGuardado metodo, CancellationToken ct)
    {
        return _coleccion.InsertOneAsync(metodo, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<MetodoPagoGuardado>> ObtenerPorUsuarioAsync(string idUsuario, CancellationToken ct)
    {
        var filter = Builders<MetodoPagoGuardado>.Filter.Eq(x => x.IdUsuario, idUsuario);
        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        var docs = new List<MetodoPagoGuardado>();
        while (await cursor.MoveNextAsync(ct))
        {
            docs.AddRange(cursor.Current);
        }

        return docs;
    }

    public async Task<string?> ObtenerIdStripeCustomerPorUsuarioAsync(string idUsuario, CancellationToken ct)
    {
        var filter = Builders<MetodoPagoGuardado>.Filter.Eq(x => x.IdUsuario, idUsuario);
        using var cursor = await Buscar(filter)
            .Sort(Builders<MetodoPagoGuardado>.Sort.Descending(x => x.FechaRegistroUtc))
            .Limit(1)
            .ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            var doc = cursor.Current.FirstOrDefault();
            return doc?.IdStripeCustomer;
        }

        return null;
    }

    public async Task<bool> EliminarPorUsuarioYPaymentMethodAsync(string idUsuario, string idStripePaymentMethod, CancellationToken ct)
    {
        var filter = Builders<MetodoPagoGuardado>.Filter.And(
            Builders<MetodoPagoGuardado>.Filter.Eq(x => x.IdUsuario, idUsuario),
            Builders<MetodoPagoGuardado>.Filter.Eq(x => x.IdStripePaymentMethod, idStripePaymentMethod));

        var result = await _coleccion.DeleteOneAsync(filter, ct);

        return result.DeletedCount > 0;
    }

    /// <summary>
    /// Método virtual para encapsular búsquedas y facilitar pruebas.
    /// </summary>
    protected virtual IFindFluent<MetodoPagoGuardado, MetodoPagoGuardado> Buscar(FilterDefinition<MetodoPagoGuardado> filter)
        => _coleccion.Find(filter);
}
