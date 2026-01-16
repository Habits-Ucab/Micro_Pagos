using Pagos_Aplicacion.Interfaces;
using Pagos_Dominio.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pagos_Infraestructura.Servicios;

public class ServicioFacturasPdf : IServicioFacturas
{
    public byte[] GenerarFacturaPdf(Pago pago)
    {
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Margin(30);
                pagina.Size(PageSizes.A4);
                pagina.DefaultTextStyle(x => x.FontSize(11));

                pagina.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Factura Digital").FontSize(18).SemiBold();
                        col.Item().Text($"Id Pago: {pago.Id}");
                        col.Item().Text($"Fecha (UTC): {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}");
                    });
                });

                pagina.Content().Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text($"Usuario: {pago.IdUsuario}");
                    col.Item().Text($"Evento: {pago.IdEvento}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Monto original:");
                        r.ConstantItem(140).AlignRight().Text($"{pago.MontoOriginal:0.00} {pago.Moneda.ToUpperInvariant()}");
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Descuento:");
                        r.ConstantItem(140).AlignRight().Text($"-{pago.DescuentoAplicado:0.00} {pago.Moneda.ToUpperInvariant()}");
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total pagado:").SemiBold();
                        r.ConstantItem(140).AlignRight().Text($"{pago.MontoFinal:0.00} {pago.Moneda.ToUpperInvariant()}").SemiBold();
                    });

                    if (!string.IsNullOrWhiteSpace(pago.CodigoPromocionAplicado))
                        col.Item().Text($"Cupón aplicado: {pago.CodigoPromocionAplicado}");
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado por Micro_Pagos").FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return documento.GeneratePdf();
    }
}
