using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Enums;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioPagosMongo : IRepositorioPagos
{
    private readonly MongoContexto _contexto;

    public RepositorioPagosMongo(MongoContexto contexto)
    {
        _contexto = contexto;
    }

    public Task CrearAsync(Pago pago, CancellationToken ct)
    {
        return _contexto.Pagos.InsertOneAsync(pago, cancellationToken: ct);
    }

    public Task ActualizarAsync(Pago pago, CancellationToken ct)
    {
        return _contexto.Pagos.ReplaceOneAsync(x => x.Id == pago.Id, pago, new ReplaceOptions { IsUpsert = false }, ct);
    }

    public async Task<Pago?> ObtenerPorIdAsync(string idPago, CancellationToken ct)
    {
        return await _contexto.Pagos.Find(x => x.Id == idPago).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Pago>> ObtenerPendientesParaConciliacionAsync(DateTimeOffset maxFechaCreacionUtc, int maxRegistros, CancellationToken ct)
    {
        return await _contexto.Pagos
            .Find(x => x.Estado == EstadoPago.Pendiente && x.FechaCreacionUtc <= maxFechaCreacionUtc)
            .SortBy(x => x.FechaCreacionUtc)
            .Limit(maxRegistros)
            .ToListAsync(ct);
    }
}
