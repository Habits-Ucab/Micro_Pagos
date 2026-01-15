using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagos_Aplicacion.Interfaces;
using Pagos_Infraestructura.HangFire;
using Pagos_Infraestructura.MassTransit;
using Pagos_Infraestructura.MongoConfig;
using Pagos_Infraestructura.Repositorios;
using Pagos_Infraestructura.Servicios;

namespace Pagos_Infraestructura;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, IConfiguration configuracion)
    {
        MongoMapeos.Configurar();

        servicios.Configure<MongoOpciones>(configuracion.GetSection(MongoOpciones.Seccion));
        servicios.AddSingleton<MongoContexto>();

        servicios.AddScoped<IRepositorioPagos, RepositorioPagosMongo>();
        servicios.AddScoped<IRepositorioPromociones, RepositorioPromocionesMongo>();
        servicios.AddScoped<IRepositorioMetodosPago, RepositorioMetodosPagoMongo>();
        servicios.AddScoped<IRepositorioFacturas, RepositorioFacturasMongo>();

        servicios.AddScoped<IServicioStripe, ServicioStripe>();
        servicios.AddScoped<IServicioFacturas, ServicioFacturasPdf>();
        servicios.AddScoped<IPublicadorEventos, PublicadorEventosMassTransit>();

        servicios.AddScoped<TrabajosProgramados>();

        return servicios;
    }
}
