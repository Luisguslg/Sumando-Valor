using System.Net;

namespace SumandoValor.Infrastructure.Services;

public static class EmailTemplates
{
    private const string BrandColor = "#00338D"; // KPMG Blue
    private const string AccentColor = "#0091DA"; // KPMG Light Blue
    private const string BackgroundColor = "#F0F2F5";
    private const string ContainerColor = "#FFFFFF";
    private const string TextColor = "#333333";

    private static string GetHtmlLayout(string title, string content)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{WebUtility.HtmlEncode(title)}</title>
    <style>
        body {{ margin: 0; padding: 0; background-color: {BackgroundColor}; font-family: 'Arial', sans-serif; color: {TextColor}; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: {ContainerColor}; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: {BrandColor}; padding: 30px 40px; text-align: left; }}
        .header h1 {{ margin: 0; color: #FFFFFF; font-size: 24px; font-weight: bold; letter-spacing: 0.5px; }}
        .header p {{ margin: 5px 0 0; color: rgba(255,255,255,0.8); font-size: 14px; }}
        .content {{ padding: 40px; line-height: 1.6; font-size: 16px; }}
        .button {{ display: inline-block; background-color: {BrandColor}; color: #FFFFFF; text-decoration: none; padding: 12px 25px; border-radius: 4px; font-weight: bold; margin-top: 20px; }}
        .footer {{ background-color: #F8F9FA; padding: 20px 40px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #E9ECEF; }}
        a {{ color: {AccentColor}; text-decoration: none; }}
        @media only screen and (max-width: 600px) {{
            .content {{ padding: 20px; }}
            .header {{ padding: 20px; }}
        }}
    </style>
</head>
<body>
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: {BackgroundColor}; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <div class=""container"">
                    <div class=""header"">
                        <h1>Sumando Valor</h1>
                        <p>Fundación KPMG Venezuela</p>
                    </div>
                    <div class=""content"">
                        {content}
                    </div>
                    <div class=""footer"">
                        <p>&copy; {DateTime.Now.Year} Fundación KPMG Venezuela. Todos los derechos reservados.</p>
                        <p>Este es un correo automático, por favor no respondas a esta dirección.</p>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string CourseAccessLinkHtml(string cursoTitulo, string accessLink)
    {
        var content = $@"
            <h2 style=""margin-top: 0; color: {BrandColor};"">Invitación al Programa Formativo</h2>
            <p>Hola,</p>
            <p>Has sido invitado a participar en el programa formativo <strong>{WebUtility.HtmlEncode(cursoTitulo)}</strong>.</p>
            <p>Para acceder al contenido exclusivo, por favor haz clic en el siguiente botón:</p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{WebUtility.HtmlEncode(accessLink)}"" class=""button"" style=""color: #FFFFFF !important;"">Acceder al Programa</a>
            </div>
            <p style=""font-size: 14px; color: #666;"">O copia y pega el siguiente enlace en tu navegador:</p>
            <p style=""font-size: 13px; color: #888; word-break: break-all;"">{WebUtility.HtmlEncode(accessLink)}</p>
            <hr style=""border: 0; border-top: 1px solid #eee; margin: 30px 0;"" />
            <p style=""font-size: 13px; color: #999;"">Este enlace es personal. Por favor, no lo compartas con otras personas.</p>";

        return GetHtmlLayout($"Invitación: {cursoTitulo}", content);
    }

    public static string GenericMessageHtml(string subject, string bodyContent)
    {
        var safeBody = WebUtility.HtmlEncode(bodyContent ?? "").Replace("\n", "<br>");
        var content = $@"
            <h2 style=""margin-top: 0; color: {BrandColor};"">{WebUtility.HtmlEncode(subject ?? "")}</h2>
            <div style=""color: #333;"">{safeBody}</div>";
        return GetHtmlLayout(subject ?? "", content);
    }

    public static string CourseAccessLinkPlain(string cursoTitulo, string accessLink)
    {
        return $"Sumando Valor - Fundación KPMG Venezuela\n\n" +
               $"Invitación al Programa Formativo: {cursoTitulo}\n\n" +
               $"Haz clic en el siguiente enlace para acceder:\n{accessLink}\n\n" +
               "Este enlace es personal. No lo compartas con otras personas.";
    }
}
