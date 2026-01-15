using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioFacturasMongo : IRepositorioFacturas
{
    private readonly MongoContexto _contexto;

    public RepositorioFacturasMongo(MongoContexto contexto)
    {
        _contexto = contexto;
    }

    public Task CrearAsync(FacturaDigital factura, CancellationToken ct)
    {
        return _contexto.Facturas.InsertOneAsync(factura, cancellationToken: ct);
    }

    public async Task<FacturaDigital?> ObtenerPorIdPagoAsync(string idPago, CancellationToken ct)
    {
        return await _contexto.Facturas.Find(x => x.IdPago == idPago).FirstOrDefaultAsync(ct);
    }
}
