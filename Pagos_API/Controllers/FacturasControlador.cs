using Microsoft.AspNetCore.Mvc;
using Pagos_Aplicacion.Interfaces;

namespace Pagos_API.Controllers;

[ApiController]
[Route("api/facturas")]
public class FacturasControlador : ControllerBase
{
    private readonly IRepositorioFacturas _repositorioFacturas;

    public FacturasControlador(IRepositorioFacturas repositorioFacturas)
    {
        _repositorioFacturas = repositorioFacturas;
    }

    /// <summary>
    /// Descarga la factura PDF asociada a un pago.
    /// </summary>
    /// <param name="idPago">Id del pago.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("por-pago/{idPago}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarPorPago([FromRoute] string idPago, CancellationToken ct)
    {
        var factura = await _repositorioFacturas.ObtenerPorIdPagoAsync(idPago, ct);
        if (factura is null) return NotFound();

        return File(factura.ContenidoPdf, "application/pdf", factura.NombreArchivo);
    }
}
