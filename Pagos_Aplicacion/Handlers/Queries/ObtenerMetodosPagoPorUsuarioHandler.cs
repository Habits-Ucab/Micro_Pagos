using MediatR;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Aplicacion.Queries;

namespace Pagos_Aplicacion.Handlers.Queries;

public class ObtenerMetodosPagoPorUsuarioHandler : IRequestHandler<ObtenerMetodosPagoPorUsuarioQuery, IReadOnlyList<MetodoPagoDto>>
{
    private readonly IRepositorioMetodosPago _repositorioMetodosPago;

    public ObtenerMetodosPagoPorUsuarioHandler(IRepositorioMetodosPago repositorioMetodosPago)
    {
        _repositorioMetodosPago = repositorioMetodosPago;
    }

    public async Task<IReadOnlyList<MetodoPagoDto>> Handle(ObtenerMetodosPagoPorUsuarioQuery request, CancellationToken cancellationToken)
    {
        var metodos = await _repositorioMetodosPago.ObtenerPorUsuarioAsync(request.IdUsuario, cancellationToken);
        return metodos
            .OrderByDescending(x => x.FechaRegistroUtc)
            .Select(x => new MetodoPagoDto(
                x.IdUsuario,
                x.IdStripeCustomer,
                x.IdStripePaymentMethod,
                x.Marca,
                x.Ultimos4,
                x.MesExp,
                x.AñoExp))
            .ToList();
    }
}
