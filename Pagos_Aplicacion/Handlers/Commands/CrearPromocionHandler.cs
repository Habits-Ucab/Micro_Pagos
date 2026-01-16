using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using Pagos_Dominio.Excepciones;

namespace Pagos_Aplicacion.Handlers.Commands;

public class CrearPromocionHandler : IRequestHandler<CrearPromocionCommand, PromocionDto>
{
    private readonly IRepositorioPromociones _repositorioPromociones;

    public CrearPromocionHandler(IRepositorioPromociones repositorioPromociones)
    {
        _repositorioPromociones = repositorioPromociones;
    }

    public async Task<PromocionDto> Handle(CrearPromocionCommand request, CancellationToken cancellationToken)
    {
        var codigoNormalizado = request.Codigo.Trim().ToUpperInvariant();
        var existente = await _repositorioPromociones.ObtenerPorCodigoYEventoAsync(request.IdEvento, codigoNormalizado, cancellationToken);
        if (existente is not null)
            throw new PromocionNoValidaExcepcion($"Ya existe una promoción con el código '{codigoNormalizado}' para el evento.");

        var promocion = new Promocion(
            request.IdEvento,
            request.Codigo,
            request.Tipo,
            request.Valor,
            request.FechaInicioUtc,
            request.FechaFinUtc,
            request.StockUsos);

        await _repositorioPromociones.CrearAsync(promocion, cancellationToken);

        return new PromocionDto(
            promocion.Id,
            promocion.IdEvento,
            promocion.Codigo,
            promocion.Tipo,
            promocion.Valor,
            promocion.FechaInicioUtc,
            promocion.FechaFinUtc,
            promocion.StockUsos,
            promocion.UsosRealizados,
            promocion.Activa);
    }
}
