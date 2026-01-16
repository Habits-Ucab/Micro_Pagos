using System.Net;
using System.Text.Json;
using Pagos_Dominio.Excepciones;
using Stripe;

namespace Pagos_API.Middlewares;

public class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ManejadorExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejadorExcepcionesMiddleware> logger, IWebHostEnvironment env)
    {
        _siguiente = siguiente;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ExcepcionDeDominio ex)
        {
            contexto.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            contexto.Response.ContentType = "application/json";
            await contexto.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje = ex.Message }));
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Error Stripe");
            contexto.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            contexto.Response.ContentType = "application/json";
            await contexto.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            contexto.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            contexto.Response.ContentType = "application/json";
            if (_env.IsDevelopment())
            {
                await contexto.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje = ex.Message, detalle = ex.ToString() }));
            }
            else
            {
                await contexto.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje = "Ocurrió un error inesperado." }));
            }
        }
    }
}
