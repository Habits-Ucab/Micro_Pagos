using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pagos_Dominio.Entidades;

namespace Pagos_Infraestructura.MongoConfig;

public class MongoContexto
{
    public IMongoDatabase BaseDeDatos { get; }

    private readonly MongoColeccionesOpciones _colecciones;

    public IMongoCollection<Pago> Pagos => BaseDeDatos.GetCollection<Pago>(NombreColeccion(_colecciones.Pagos, "pagos"));
    public IMongoCollection<Promocion> Promociones => BaseDeDatos.GetCollection<Promocion>(NombreColeccion(_colecciones.Promociones, "promociones"));
    public IMongoCollection<MetodoPagoGuardado> MetodosPago => BaseDeDatos.GetCollection<MetodoPagoGuardado>(NombreColeccion(_colecciones.MetodosPago, "metodos_pago"));
    public IMongoCollection<FacturaDigital> Facturas => BaseDeDatos.GetCollection<FacturaDigital>(NombreColeccion(_colecciones.Facturas, "facturas"));

    public MongoContexto(IOptions<MongoOpciones> opciones)
    {
        if (string.IsNullOrWhiteSpace(opciones.Value.CadenaConexion))
            throw new InvalidOperationException("MongoDb:CadenaConexion no está configurado.");

        if (string.IsNullOrWhiteSpace(opciones.Value.NombreBaseDeDatos))
            throw new InvalidOperationException("MongoDb:NombreBaseDeDatos no está configurado.");

        var cliente = new MongoClient(opciones.Value.CadenaConexion);
        BaseDeDatos = cliente.GetDatabase(opciones.Value.NombreBaseDeDatos);

        _colecciones = opciones.Value.Colecciones ?? new MongoColeccionesOpciones();
    }

    private static string NombreColeccion(string? desdeConfig, string porDefecto)
    {
        return string.IsNullOrWhiteSpace(desdeConfig) ? porDefecto : desdeConfig;
    }
}
