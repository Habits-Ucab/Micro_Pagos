using MongoDB.Driver;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Infraestructura.MongoConfig;

namespace Pagos_Infraestructura.Repositorios;

public class RepositorioFacturasMongo : IRepositorioFacturas
{
    private readonly IMongoCollection<FacturaDigital> _coleccion;

    public RepositorioFacturasMongo(MongoContexto contexto)
    {
        _coleccion = contexto.Facturas;
    }

    /// <summary>
    /// Constructor alternativo para pruebas, permite inyectar colección simulada.
    /// </summary>
    public RepositorioFacturasMongo(IMongoCollection<FacturaDigital> coleccion)
    {
        _coleccion = coleccion;
    }

    public Task CrearAsync(FacturaDigital factura, CancellationToken ct)
    {
        return _coleccion.InsertOneAsync(factura, cancellationToken: ct);
    }

    public async Task<FacturaDigital?> ObtenerPorIdPagoAsync(string idPago, CancellationToken ct)
    {
        var filter = Builders<FacturaDigital>.Filter.Eq(x => x.IdPago, idPago);
        using var cursor = await Buscar(filter).ToCursorAsync(ct);

        while (await cursor.MoveNextAsync(ct))
        {
            var doc = cursor.Current.FirstOrDefault();
            return doc;
        }

        return null;
    }

    /// <summary>
    /// Método virtual para encapsular búsquedas y facilitar pruebas.
    /// </summary>
    protected virtual IFindFluent<FacturaDigital, FacturaDigital> Buscar(FilterDefinition<FacturaDigital> filter)
        => _coleccion.Find(filter);
}
