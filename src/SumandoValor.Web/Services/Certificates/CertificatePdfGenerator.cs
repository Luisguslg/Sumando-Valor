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
        var curso = (data.CursoTitulo ?? string.Empty).Trim();
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

                    // The template has printed text inside the central boxes. To avoid overlap,
                    // we "mask" that area and draw our own clean blocks on top.
                    layers.PrimaryLayer().Column(col =>
                    {
                        // Push down to the area where the recipient name and info blocks live in the template.
                        col.Item().Height(245);

                        col.Item().PaddingHorizontal(70).Element(panel =>
                        {
                            panel
                                .Background(Colors.White)
                                .BorderRadius(18)
                                .PaddingVertical(24)
                                .PaddingHorizontal(26)
                                .Column(p =>
                                {
                                    p.Item().AlignCenter()
                                        .Text(string.IsNullOrWhiteSpace(nombre) ? "-" : nombre)
                                        .FontSize(30)
                                        .Bold()
                                        .FontColor(Colors.Black);

                                    p.Item().Height(16);

                                    p.Item().Row(row =>
                                    {
                                        void Block(string label, string value)
                                        {
                                            row.RelativeItem().Element(card =>
                                            {
                                                card
                                                    .Background("#EEE9FF")
                                                    .BorderRadius(16)
                                                    .Padding(14)
                                                    .Column(c =>
                                                    {
                                                        c.Item().AlignCenter()
                                                            .Text(label)
                                                            .FontSize(11)
                                                            .FontColor(Colors.Blue.Darken3);

                                                        c.Item().Height(6);
                                                        c.Item().AlignCenter()
                                                            .Text(string.IsNullOrWhiteSpace(value) ? "-" : value)
                                                            .FontSize(13)
                                                            .SemiBold()
                                                            .FontColor(Colors.Black);
                                                    });
                                            });
                                        }

                                        Block("Taller", taller);
                                        row.ConstantItem(12);
                                        Block("Curso", curso);
                                        row.ConstantItem(12);
                                        Block("Fecha", fecha);
                                    });
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
    public string? CursoTitulo { get; init; }
    public DateTime Fecha { get; init; }
}

