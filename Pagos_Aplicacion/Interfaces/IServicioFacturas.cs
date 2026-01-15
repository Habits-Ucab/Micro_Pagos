using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Interfaces;

public interface IServicioFacturas
{
    byte[] GenerarFacturaPdf(Pago pago);
}
