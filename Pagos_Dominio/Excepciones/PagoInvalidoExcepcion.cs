namespace Pagos_Dominio.Excepciones;

public class PagoInvalidoExcepcion : ExcepcionDeDominio
{
    public PagoInvalidoExcepcion(string mensaje) : base(mensaje)
    {
    }
}
