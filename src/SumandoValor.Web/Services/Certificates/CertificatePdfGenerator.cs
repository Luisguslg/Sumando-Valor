using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SumandoValor.Web.Services.Certificates;

public sealed class CertificatePdfGenerator
{
    private readonly IWebHostEnvironment _env;

    public CertificatePdfGenerator(IWebHostEnvironment env)
    {
        _env = env;
    }

    public byte[] Generate(CertificatePdfData data)
    {
        var templatePath = Path.Combine(_env.WebRootPath, "images", "certificates", "certificado-template.png");
        byte[]? templateBytes = null;
        if (File.Exists(templatePath))
        {
            templateBytes = File.ReadAllBytes(templatePath);
        }

        var nombre = (data.NombreCompleto ?? string.Empty).Trim();
        var taller = (data.TallerTitulo ?? string.Empty).Trim();
        var duracion = (data.DuracionTexto ?? string.Empty).Trim();
        var fecha = data.Fecha.ToString("dd/MM/yyyy");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(16));

                page.Content().Layers(layers =>
                {
                    if (templateBytes != null)
                    {
                        layers.Layer().Image(templateBytes);
                    }
                    else
                    {
                        layers.Layer().Background(Colors.Grey.Lighten3);
                    }

                    // The template already includes the graphic boxes and labels.
                    // We only place the dynamic values (name, taller, duración, fecha) in the blank areas.
                    layers.PrimaryLayer().Column(col =>
                    {
                        // Position roughly at the blank line under "Otorga el siguiente certificado a:"
                        col.Item().Height(210);

                        col.Item().AlignCenter()
                            .Text(string.IsNullOrWhiteSpace(nombre) ? "-" : nombre)
                            .FontSize(34)
                            .SemiBold()
                            .FontColor("#00338D");

                        // Space down to the row of 3 info boxes
                        col.Item().Height(120);

                        col.Item().PaddingHorizontal(78).Row(row =>
                        {
                            void BoxValue(string value)
                            {
                                row.RelativeItem().AlignCenter().AlignMiddle().Height(58).Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                                    .FontSize(14)
                                    .SemiBold()
                                    .FontColor("#0B1F5C");
                            }

                            BoxValue(taller);
                            row.ConstantItem(28);
                            BoxValue(duracion);
                            row.ConstantItem(28);
                            BoxValue(fecha);
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
}

