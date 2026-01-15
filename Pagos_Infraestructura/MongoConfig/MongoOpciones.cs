namespace Pagos_Infraestructura.MongoConfig;
public class MongoOpciones
{
    public const string Seccion = "MongoDb";

    public string CadenaConexion { get; init; } = string.Empty;
    public string NombreBaseDeDatos { get; init; } = string.Empty;

    public MongoColeccionesOpciones Colecciones { get; init; } = new();
}

public class MongoColeccionesOpciones
{
    public string Pagos { get; init; } = "pagos";
    public string Promociones { get; init; } = "promociones";
    public string MetodosPago { get; init; } = "metodos_pago";
    public string Facturas { get; init; } = "facturas";
}
