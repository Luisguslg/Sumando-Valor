using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SumandoValor.Web.Services.Certificates;

public sealed class CertificatePdfGenerator
{
    public byte[] Generate(CertificatePdfData data)
    {
        var nombre = (data.NombreCompleto ?? string.Empty).Trim();
        var taller = (data.TallerTitulo ?? string.Empty).Trim();
        var duracion = (data.DuracionTexto ?? string.Empty).Trim();
        var fecha = data.Fecha.ToString("dd/MM/yyyy");

        // NOTE: We intentionally generate the full certificate design in code.
        // This avoids IIS publish issues where static template images may not be present,
        // and prevents "writing on top of an image" misalignment problems.
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(14).FontColor("#0B1F5C"));

                page.Content().Element(root =>
                {
                    root.Border(2).BorderColor("#00338D").Padding(26).Column(col =>
                    {
                        col.Spacing(16);

                        // Header
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(h =>
                            {
                                h.Item().Text("Sumando Valor").FontSize(16).SemiBold().FontColor("#00338D");
                                h.Item().Text("CERTIFICADO").FontSize(28).Bold().FontColor("#00338D");
                            });

                            row.ConstantItem(160).AlignRight().AlignMiddle()
                                .Text("Fundación KPMG\nVenezuela")
                                .FontSize(12).FontColor(Colors.Grey.Darken2);
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        col.Item().Text("Otorga el siguiente certificado a:").FontSize(14).FontColor(Colors.Grey.Darken2);

                        // Name
                        col.Item().PaddingVertical(6).AlignCenter().Text(string.IsNullOrWhiteSpace(nombre) ? "—" : nombre)
                            .FontSize(34).SemiBold().FontColor("#00338D");

                        col.Item().AlignCenter().Text("Por su participación y culminación satisfactoria del taller:")
                            .FontSize(13).FontColor(Colors.Grey.Darken2);

                        col.Item().PaddingTop(6).AlignCenter().Text(string.IsNullOrWhiteSpace(taller) ? "—" : taller)
                            .FontSize(18).SemiBold().FontColor("#0B1F5C");

                        col.Item().PaddingTop(18).Row(row =>
                        {
                            void InfoBox(string label, string value)
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(12).Column(b =>
                                {
                                    b.Item().Text(label).FontSize(11).FontColor(Colors.Grey.Darken2);
                                    b.Item().PaddingTop(6).Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(14).SemiBold();
                                });
                            }

                            InfoBox("Duración", duracion);
                            row.ConstantItem(12);
                            InfoBox("Fecha", fecha);
                        });

                        col.Item().PaddingTop(28).Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Column(sig =>
                            {
                                sig.Item().PaddingBottom(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                                sig.Item().Text("Firma / Sello").FontSize(11).FontColor(Colors.Grey.Darken2);
                            });

                            row.RelativeItem().AlignRight().Column(meta =>
                            {
                                meta.Item().AlignRight().Text($"Emitido el {fecha}").FontSize(11).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrWhiteSpace(data.VerificationCode))
                                {
                                    meta.Item().AlignRight().Text($"Código: {data.VerificationCode}").FontSize(11).FontColor(Colors.Grey.Darken2);
                                }
                            });
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }
}

public sealed class CertificatePdfData
{
    public string NombreCompleto { get; init; } = string.Empty;
    public string TallerTitulo { get; init; } = string.Empty;
    public string? DuracionTexto { get; init; }
    public DateTime Fecha { get; init; }
    public string? VerificationCode { get; init; }
}

