using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioMetodosPagoMongo : IRepositorioMetodosPago
{
    private readonly MongoContexto _contexto;

    public RepositorioMetodosPagoMongo(MongoContexto contexto)
    {
        _contexto = contexto;
    }

    public Task CrearAsync(MetodoPagoGuardado metodo, CancellationToken ct)
    {
        return _contexto.MetodosPago.InsertOneAsync(metodo, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<MetodoPagoGuardado>> ObtenerPorUsuarioAsync(string idUsuario, CancellationToken ct)
    {
        return await _contexto.MetodosPago.Find(x => x.IdUsuario == idUsuario).ToListAsync(ct);
    }

    public async Task<string?> ObtenerIdStripeCustomerPorUsuarioAsync(string idUsuario, CancellationToken ct)
    {
        var doc = await _contexto.MetodosPago
            .Find(x => x.IdUsuario == idUsuario)
            .SortByDescending(x => x.FechaRegistroUtc)
            .Limit(1)
            .FirstOrDefaultAsync(ct);

        return doc?.IdStripeCustomer;
    }

    public async Task<bool> EliminarPorUsuarioYPaymentMethodAsync(string idUsuario, string idStripePaymentMethod, CancellationToken ct)
    {
        var result = await _contexto.MetodosPago.DeleteOneAsync(
            x => x.IdUsuario == idUsuario && x.IdStripePaymentMethod == idStripePaymentMethod,
            ct);

        return result.DeletedCount > 0;
    }
}
