using MediatR;
using Pagos_Aplicacion.Commands;
using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;

namespace Pagos_Aplicacion.Handlers.Commands;

public class AgregarMetodoPagoHandler : IRequestHandler<AgregarMetodoPagoCommand, MetodoPagoDto>
{
    private readonly IRepositorioMetodosPago _repositorioMetodosPago;
    private readonly IServicioStripe _servicioStripe;

    public AgregarMetodoPagoHandler(IRepositorioMetodosPago repositorioMetodosPago, IServicioStripe servicioStripe)
    {
        _repositorioMetodosPago = repositorioMetodosPago;
        _servicioStripe = servicioStripe;
    }

    public async Task<MetodoPagoDto> Handle(AgregarMetodoPagoCommand request, CancellationToken cancellationToken)
    {
        var idStripeCustomerExistente = await _repositorioMetodosPago.ObtenerIdStripeCustomerPorUsuarioAsync(request.IdUsuario, cancellationToken);

        var datosStripe = await _servicioStripe.AgregarMetodoPagoAsync(
            idStripeCustomerExistente,
            request.EmailUsuario,
            request.IdMetodoPagoStripe,
            cancellationToken);

        var entidad = new MetodoPagoGuardado(
            request.IdUsuario,
            datosStripe.IdStripeCustomer,
            datosStripe.IdStripePaymentMethod,
            datosStripe.Marca,
            datosStripe.Ultimos4,
            datosStripe.MesExp,
            datosStripe.AnioExp);

        await _repositorioMetodosPago.CrearAsync(entidad, cancellationToken);

        return new MetodoPagoDto(
            entidad.IdUsuario,
            entidad.IdStripeCustomer,
            entidad.IdStripePaymentMethod,
            entidad.Marca,
            entidad.Ultimos4,
            entidad.MesExp,
            entidad.AñoExp);
    }
}
