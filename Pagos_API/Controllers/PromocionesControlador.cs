using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Queries;
using Pagos_API.Modelos.Promociones;
using Pagos_API.Swagger.Ejemplos;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Controllers;

[ApiController]
[Route("api/promociones")]
public class PromocionesControlador : ControllerBase
{
    private readonly IMediator _mediator;

    public PromocionesControlador(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea una promoción/cupón para un evento.
    /// </summary>
    /// <remarks>
    /// Soporta promociones por porcentaje o monto fijo, con ventana de vigencia (inicio/fin) y stock de usos.
    /// El código se normaliza a MAYÚSCULAS.
    /// </remarks>
    /// <param name="solicitud">Datos de la promoción.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Promoción creada.</response>
    /// <response code="400">Datos inválidos o el código ya existe para el evento.</response>
    /// <response code="500">Error inesperado.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerRequestExample(typeof(CrearPromocionSolicitud), typeof(CrearPromocionSolicitudEjemplo))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PromocionDtoEjemplo))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErrorRespuestaEjemplo))]
    public async Task<IActionResult> Crear([FromBody] CrearPromocionSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await _mediator.Send(
            new CrearPromocionCommand(
                solicitud.IdEvento,
                solicitud.Codigo,
                solicitud.Tipo,
                solicitud.Valor,
                solicitud.FechaInicioUtc,
                solicitud.FechaFinUtc,
                solicitud.StockUsos),
            ct);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene promociones por evento.
    /// </summary>
    /// <param name="idEvento">Identificador del evento.</param>
    /// <param name="soloVigentes">Si es true, devuelve solo promociones vigentes y con stock.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Lista de promociones.</response>
    /// <response code="500">Error inesperado.</response>
    [HttpGet("por-evento/{idEvento}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PromocionesListaEjemplo))]
    public async Task<IActionResult> ObtenerPorEvento([FromRoute] string idEvento, [FromQuery] bool soloVigentes = true, CancellationToken ct = default)
    {
        var resultado = await _mediator.Send(new ObtenerPromocionesPorEventoQuery(idEvento, soloVigentes), ct);
        return Ok(resultado);
    }
}
