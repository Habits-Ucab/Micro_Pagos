using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Queries;
using Pagos_API.Swagger.Ejemplos;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Controllers;

[ApiController]
[Route("api/metodos-pago")]
public class MetodosPagoControlador : ControllerBase
{
    private readonly IMediator _mediator;

    public MetodosPagoControlador(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene los métodos de pago guardados de un usuario.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Lista de métodos de pago del usuario.</response>
    /// <response code="500">Error inesperado.</response>
    [HttpGet("{idUsuario}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MetodosPagoListaEjemplo))]
    public async Task<IActionResult> ObtenerPorUsuario([FromRoute] string idUsuario, CancellationToken ct)
    {
        var resultado = await _mediator.Send(new ObtenerMetodosPagoPorUsuarioQuery(idUsuario), ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Elimina un método de pago guardado (lo desasocia en Stripe y lo elimina de Mongo).
    /// </summary>
    [HttpDelete("{idUsuario}/{idMetodoPagoStripe}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Eliminar([FromRoute] string idUsuario, [FromRoute] string idMetodoPagoStripe, CancellationToken ct)
    {
        try
        {
            var eliminado = await _mediator.Send(new EliminarMetodoPagoCommand(idUsuario, idMetodoPagoStripe), ct);
            if (!eliminado) return NotFound();
            return Ok(new { eliminado = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
