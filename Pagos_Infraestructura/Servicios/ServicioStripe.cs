using Pagos_Aplicacion.DTOs;
using Pagos_Aplicacion.Interfaces;
using Stripe;

namespace Pagos_Infraestructura.Servicios;

public class ServicioStripe : IServicioStripe
{
    private readonly CustomerService _customerService = new();
    private readonly PaymentMethodService _paymentMethodService = new();
    private readonly PaymentIntentService _paymentIntentService = new();

    public async Task<DatosMetodoPagoStripeDto> AgregarMetodoPagoAsync(
        string? idStripeCustomerExistente,
        string emailUsuario,
        string idMetodoPagoStripe,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(emailUsuario))
            throw new InvalidOperationException("El email del usuario es obligatorio para crear el Customer en Stripe.");

        var idCustomer = idStripeCustomerExistente;
        if (string.IsNullOrWhiteSpace(idCustomer))
        {
            var customer = await _customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = emailUsuario,
                Description = "Cliente creado por Micro_Pagos"
            }, cancellationToken: ct);

            idCustomer = customer.Id;
        }

        await _paymentMethodService.AttachAsync(idMetodoPagoStripe, new PaymentMethodAttachOptions
        {
            Customer = idCustomer
        }, cancellationToken: ct);

        // Establecer como método por defecto para futuras operaciones
        await _customerService.UpdateAsync(idCustomer, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = idMetodoPagoStripe
            }
        }, cancellationToken: ct);

        var metodo = await _paymentMethodService.GetAsync(idMetodoPagoStripe, cancellationToken: ct);
        var tarjeta = metodo.Card;

        return new DatosMetodoPagoStripeDto(
            idCustomer,
            metodo.Id,
            tarjeta?.Brand ?? "desconocida",
            tarjeta?.Last4 ?? "",
            tarjeta?.ExpMonth ?? 0,
            tarjeta?.ExpYear ?? 0);
    }

    public async Task<ResultadoIntentoPagoStripeDto> CrearYConfirmarPagoAsync(
        string idStripeCustomer,
        string idMetodoPagoStripe,
        long montoEnUnidadMenor,
        string moneda,
        string idPago,
        CancellationToken ct)
    {
        var intento = await _paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = montoEnUnidadMenor,
            Currency = moneda,
            Customer = idStripeCustomer,
            PaymentMethod = idMetodoPagoStripe,
            Confirm = true,
            OffSession = true,
            Metadata = new Dictionary<string, string>
            {
                ["idPago"] = idPago
            }
        }, cancellationToken: ct);

        return new ResultadoIntentoPagoStripeDto(intento.Id, intento.Status, intento.ClientSecret);
    }

    public async Task<ResultadoIntentoPagoStripeDto> ConfirmarPaymentIntentAsync(
        string idPaymentIntent,
        string idMetodoPagoStripe,
        CancellationToken ct)
    {
        var intento = await _paymentIntentService.ConfirmAsync(idPaymentIntent, new PaymentIntentConfirmOptions
        {
            PaymentMethod = idMetodoPagoStripe,
            OffSession = true
        }, cancellationToken: ct);

        return new ResultadoIntentoPagoStripeDto(intento.Id, intento.Status, intento.ClientSecret);
    }

    public async Task<string> ObtenerEstadoPaymentIntentAsync(string idPaymentIntent, CancellationToken ct)
    {
        var pi = await _paymentIntentService.GetAsync(idPaymentIntent, cancellationToken: ct);
        return pi.Status;
    }

    public async Task DesasociarMetodoPagoAsync(string idMetodoPagoStripe, CancellationToken ct)
    {
        await _paymentMethodService.DetachAsync(idMetodoPagoStripe, cancellationToken: ct);
    }

    public async Task EstablecerMetodoPagoPorDefectoAsync(string idStripeCustomer, string? idMetodoPagoStripe, CancellationToken ct)
    {
        await _customerService.UpdateAsync(idStripeCustomer, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = idMetodoPagoStripe
            }
        }, cancellationToken: ct);
    }
}
