using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SumandoValor.Web.Services.Certificates;

public sealed class CertificatePdfGenerator
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CertificatePdfGenerator> _logger;

    public CertificatePdfGenerator(IWebHostEnvironment env, ILogger<CertificatePdfGenerator> logger)
    {
        _env = env;
        _logger = logger;
    }

    public byte[] Generate(CertificatePdfData data)
    {
        var nombre = (data.NombreCompleto ?? string.Empty).Trim();
        var taller = (data.TallerTitulo ?? string.Empty).Trim();
        var duracion = (data.DuracionTexto ?? string.Empty).Trim();
        var fecha = data.Fecha.ToString("dd/MM/yyyy");

        var templateBytes = TryLoadTemplate();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(14).FontColor("#0B1F5C"));

                page.Content().Element(root =>
                {
                    if (templateBytes == null)
                    {
                        // Fallback: generate the certificate design in code if the template is missing.
                        root.Padding(40).Border(2).BorderColor("#00338D").Padding(26).Column(col =>
                        {
                            col.Spacing(16);

                            col.Item().Text("Otorga el siguiente certificado a:").FontSize(14).FontColor(Colors.Grey.Darken2);

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

                            if (!string.IsNullOrWhiteSpace(data.VerificationCode))
                            {
                                col.Item().AlignRight().Text($"Código: {data.VerificationCode}").FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                        });
                        return;
                    }

                    root.Layers(layers =>
                    {
                        layers.Layer().Image(templateBytes, ImageScaling.FitArea);

                        layers.PrimaryLayer().PaddingTop(200).PaddingHorizontal(70).Column(col =>
                        {
                            col.Item().AlignCenter()
                                .Text(string.IsNullOrWhiteSpace(nombre) ? "—" : nombre)
                                .FontSize(34).SemiBold().FontColor("#00338D");

                            // Spacing to the three info cards already drawn in the template
                            col.Item().PaddingTop(145).Row(row =>
                            {
                                void CardValue(string value)
                                {
                                    row.RelativeItem().Height(86).AlignMiddle().AlignCenter().PaddingHorizontal(18)
                                        .Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                                        .FontSize(13).SemiBold().FontColor("#0B1F5C");
                                }

                                CardValue(taller);
                                row.ConstantItem(28);
                                CardValue(duracion);
                                row.ConstantItem(28);
                                CardValue(fecha);
                            });

                            if (!string.IsNullOrWhiteSpace(data.VerificationCode))
                            {
                                col.Item().PaddingTop(330).AlignRight()
                                    .Text(data.VerificationCode)
                                    .FontSize(9).FontColor(Colors.Grey.Darken2);
                            }
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private byte[]? TryLoadTemplate()
    {
        try
        {
            var path = Path.Combine(_env.WebRootPath, "images", "certificates", "certificado-template.png");
            if (!File.Exists(path))
            {
                _logger.LogWarning("Certificate template not found at {Path}. Falling back to code-generated certificate.", path);
                return null;
            }

            return File.ReadAllBytes(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load certificate template. Falling back to code-generated certificate.");
            return null;
        }
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

