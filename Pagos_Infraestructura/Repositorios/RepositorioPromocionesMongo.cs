using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioPromocionesMongo : IRepositorioPromociones
{
    private readonly MongoContexto _contexto;

    public RepositorioPromocionesMongo(MongoContexto contexto)
    {
        _contexto = contexto;
    }

    public Task CrearAsync(Promocion promocion, CancellationToken ct)
    {
        return _contexto.Promociones.InsertOneAsync(promocion, cancellationToken: ct);
    }

    public async Task<Promocion?> ObtenerPorIdAsync(string idPromocion, CancellationToken ct)
    {
        return await _contexto.Promociones.Find(x => x.Id == idPromocion).FirstOrDefaultAsync(ct);
    }

    public async Task<Promocion?> ObtenerPorCodigoYEventoAsync(string idEvento, string codigo, CancellationToken ct)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        return await _contexto.Promociones
            .Find(x => x.IdEvento == idEvento && x.Codigo == codigoNormalizado)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Promocion>> ObtenerPorEventoAsync(string idEvento, CancellationToken ct)
    {
        return await _contexto.Promociones
            .Find(x => x.IdEvento == idEvento)
            .ToListAsync(ct);
    }

    public async Task<bool> IntentarRegistrarUsoAsync(string idPromocion, CancellationToken ct)
    {
        // Control de concurrencia optimista: leer y actualizar solo si UsosRealizados no cambió.
        for (var intento = 0; intento < 3; intento++)
        {
            var promocion = await _contexto.Promociones.Find(x => x.Id == idPromocion).FirstOrDefaultAsync(ct);
            if (promocion is null) return false;
            if (!promocion.Activa) return false;
            if (promocion.UsosRealizados >= promocion.StockUsos) return false;

            var filtro = Builders<Promocion>.Filter.And(
                Builders<Promocion>.Filter.Eq(x => x.Id, idPromocion),
                Builders<Promocion>.Filter.Eq(x => x.Activa, true),
                Builders<Promocion>.Filter.Eq(x => x.UsosRealizados, promocion.UsosRealizados));

            var actualizacion = Builders<Promocion>.Update.Inc(x => x.UsosRealizados, 1);
            var resultado = await _contexto.Promociones.UpdateOneAsync(filtro, actualizacion, cancellationToken: ct);

            if (resultado.ModifiedCount > 0)
            {
                // Best effort: si se agotó el stock, desactivar.
                var despues = promocion.UsosRealizados + 1;
                if (despues >= promocion.StockUsos)
                {
                    await _contexto.Promociones.UpdateOneAsync(
                        x => x.Id == idPromocion,
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
        var resultado = await _contexto.Promociones.UpdateManyAsync(filtro, update, cancellationToken: ct);
        return (int)resultado.ModifiedCount;
    }
}
