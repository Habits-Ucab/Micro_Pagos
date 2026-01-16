using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioPromocionesMongo : IRepositorioPromociones
{
    private readonly IMongoCollection<Promocion> _coleccion;

    public RepositorioPromocionesMongo(MongoContexto contexto)
    {
        _coleccion = contexto.Promociones;
    }

    /// <summary>
    /// Constructor alternativo para pruebas, permite inyectar colección simulada.
    /// </summary>
    public RepositorioPromocionesMongo(IMongoCollection<Promocion> coleccion)
    {
        _coleccion = coleccion;
    }

    public Task CrearAsync(Promocion promocion, CancellationToken ct)
    {
        return _coleccion.InsertOneAsync(promocion, cancellationToken: ct);
    }

    public async Task<Promocion?> ObtenerPorIdAsync(string idPromocion, CancellationToken ct)
    {
        var filter = Builders<Promocion>.Filter.Eq(x => x.Id, idPromocion);
        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            var doc = cursor.Current.FirstOrDefault();
            return doc;
        }

        return null;
    }

    public async Task<Promocion?> ObtenerPorCodigoYEventoAsync(string idEvento, string codigo, CancellationToken ct)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        var filter = Builders<Promocion>.Filter.And(
            Builders<Promocion>.Filter.Eq(x => x.IdEvento, idEvento),
            Builders<Promocion>.Filter.Eq(x => x.Codigo, codigoNormalizado));

        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            var doc = cursor.Current.FirstOrDefault();
            return doc;
        }

        return null;
    }

    public async Task<IReadOnlyList<Promocion>> ObtenerPorEventoAsync(string idEvento, CancellationToken ct)
    {
        var filter = Builders<Promocion>.Filter.Eq(x => x.IdEvento, idEvento);
        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        var docs = new List<Promocion>();
        while (await cursor.MoveNextAsync(ct))
        {
            docs.AddRange(cursor.Current);
        }

        return docs;
    }

    public async Task<bool> IntentarRegistrarUsoAsync(string idPromocion, CancellationToken ct)
    {
        // Control de concurrencia optimista: leer y actualizar solo si UsosRealizados no cambió.
        for (var intento = 0; intento < 3; intento++)
        {
            var filterId = Builders<Promocion>.Filter.Eq(x => x.Id, idPromocion);
            using var cursor = await Buscar(filterId).ToCursorAsync(ct);

            Promocion? promocion = null;
            while (await cursor.MoveNextAsync(ct))
            {
                promocion = cursor.Current.FirstOrDefault();
                break;
            }

            if (promocion is null) return false;
            if (!promocion.Activa) return false;
            if (promocion.UsosRealizados >= promocion.StockUsos) return false;

            var filtro = Builders<Promocion>.Filter.And(
                Builders<Promocion>.Filter.Eq(x => x.Id, idPromocion),
                Builders<Promocion>.Filter.Eq(x => x.Activa, true),
                Builders<Promocion>.Filter.Eq(x => x.UsosRealizados, promocion.UsosRealizados));

            var actualizacion = Builders<Promocion>.Update.Inc(x => x.UsosRealizados, 1);
            var resultado = await _coleccion.UpdateOneAsync(filtro, actualizacion, cancellationToken: ct);

            if (resultado.ModifiedCount > 0)
            {
                // Best effort: si se agotó el stock, desactivar.
                var despues = promocion.UsosRealizados + 1;
                if (despues >= promocion.StockUsos)
                {
                    var filtroId = Builders<Promocion>.Filter.Eq(x => x.Id, idPromocion);
                    await _coleccion.UpdateOneAsync(
                        filtroId,
                        Builders<Promocion>.Update.Set(x => x.Activa, false),
                        cancellationToken: ct);
                }

                return true;
            }
        }

        return false;
    }

    public async Task<int> DesactivarExpiradasAsync(DateTimeOffset ahoraUtc, CancellationToken ct)
    {
        var filtro = Builders<Promocion>.Filter.And(
            Builders<Promocion>.Filter.Eq(x => x.Activa, true),
            Builders<Promocion>.Filter.Lt(x => x.FechaFinUtc, ahoraUtc));

        var update = Builders<Promocion>.Update.Set(x => x.Activa, false);
        var resultado = await _coleccion.UpdateManyAsync(filtro, update, cancellationToken: ct);
        return (int)resultado.ModifiedCount;
    }

    /// <summary>
    /// Método virtual para encapsular búsquedas y facilitar pruebas.
    /// </summary>
    protected virtual IFindFluent<Promocion, Promocion> Buscar(FilterDefinition<Promocion> filter)
        => _coleccion.Find(filter);
}
