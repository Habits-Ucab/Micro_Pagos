using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.Queries;
using Pagos_API.Modelos.Pagos;
using Pagos_API.Swagger.Ejemplos;
using Swashbuckle.AspNetCore.Filters;

namespace Pagos_API.Controllers;

[ApiController]
[Route("api/pagos")]
public class PagosControlador : ControllerBase
{
    private readonly IMediator _mediator;

    public PagosControlador(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta un pago por id (estado y datos básicos).
    /// </summary>
    [HttpGet("{idPago}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] string idPago, CancellationToken ct)
    {
        var pago = await _mediator.Send(new ObtenerPagoQuery(idPago), ct);
        if (pago is null) return NotFound();
        return Ok(pago);
    }

    /// <summary>
    /// Sincroniza el estado del pago contra Stripe y, si ya está confirmado,
    /// genera factura y publica eventos.
    /// </summary>
    [HttpPost("{idPago}/sincronizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sincronizar([FromRoute] string idPago, CancellationToken ct)
    {
        try
        {
            var resultado = await _mediator.Send(new SincronizarPagoCommand(idPago), ct);
            return Ok(resultado);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Realiza el checkout y crea un pago en Stripe.
    /// </summary>
    /// <remarks>
    /// Flujo:
    /// 1) Valida cupón (si aplica) y registra el uso.
    /// 2) Crea el pago en MongoDB.
    /// 3) Crea y confirma el PaymentIntent en Stripe.
    /// 4) Si es exitoso, genera factura PDF y publica el evento PagoConfirmado por MassTransit.
    /// </remarks>
    /// <param name="solicitud">Datos del pago, método de pago y cupón opcional.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Pago procesado (puede requerir acción si Stripe lo indica).</response>
    /// <response code="400">Datos inválidos o cupón no válido/no vigente/sin stock.</response>
    /// <response code="500">Error inesperado.</response>
    [HttpPost("checkout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerRequestExample(typeof(CrearPagoSolicitud), typeof(CrearPagoSolicitudEjemplo))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResultadoPagoDtoEjemplo))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErrorRespuestaEjemplo))]
    public async Task<IActionResult> Checkout([FromBody] CrearPagoSolicitud solicitud, CancellationToken ct)
    {
        var resultado = await _mediator.Send(
            new CrearPagoCommand(
                solicitud.IdReserva,
                solicitud.IdUsuario,
                solicitud.EmailUsuario,
                solicitud.IdEvento,
                solicitud.Monto,
                solicitud.Moneda,
                solicitud.IdMetodoPagoStripe,
                solicitud.CodigoPromocion),
            ct);

        return Ok(resultado);
    }
}
