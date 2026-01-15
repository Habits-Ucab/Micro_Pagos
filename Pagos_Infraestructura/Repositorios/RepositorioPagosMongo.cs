using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioPagosMongo : IRepositorioPagos
{
    private readonly IMongoCollection<Pago> _coleccion;

    public RepositorioPagosMongo(MongoContexto contexto)
    {
        _coleccion = contexto.Pagos;
    }

    /// <summary>
    /// Constructor alternativo para pruebas, permite inyectar colección simulada.
    /// </summary>
    public RepositorioPagosMongo(IMongoCollection<Pago> coleccion)
    {
        _coleccion = coleccion;
    }

    public Task CrearAsync(Pago pago, CancellationToken ct)
    {
        return _coleccion.InsertOneAsync(pago, cancellationToken: ct);
    }

    public Task ActualizarAsync(Pago pago, CancellationToken ct)
    {
        return _coleccion.ReplaceOneAsync(x => x.Id == pago.Id, pago, new ReplaceOptions { IsUpsert = false }, ct);
    }

    public async Task<Pago?> ObtenerPorIdAsync(string idPago, CancellationToken ct)
    {
        var filter = Builders<Pago>.Filter.Eq(x => x.Id, idPago);
        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            var doc = cursor.Current.FirstOrDefault();
            return doc;
        }

        return null;
    }

    public async Task<IReadOnlyList<Pago>> ObtenerPendientesParaConciliacionAsync(DateTimeOffset maxFechaCreacionUtc, int maxRegistros, CancellationToken ct)
    {
        var filter = Builders<Pago>.Filter.And(
            Builders<Pago>.Filter.Eq(x => x.Estado, EstadoPago.Pendiente),
            Builders<Pago>.Filter.Lte(x => x.FechaCreacionUtc, maxFechaCreacionUtc));

        using var cursor = await Buscar(filter)
            .Sort(Builders<Pago>.Sort.Ascending(x => x.FechaCreacionUtc))
            .Limit(maxRegistros)
            .ToCursorAsync(ct);

        var docs = new List<Pago>();
        while (await cursor.MoveNextAsync(ct))
        {
            docs.AddRange(cursor.Current);
        }

        return docs;
    }

    /// <summary>
    /// Método virtual para encapsular búsquedas y facilitar pruebas.
    /// </summary>
    protected virtual IFindFluent<Pago, Pago> Buscar(FilterDefinition<Pago> filter)
        => _coleccion.Find(filter);
}
