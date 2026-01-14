using System.Globalization;

namespace SumandoValor.Web.Helpers;

public static class DateFormat
{
    private static readonly CultureInfo EsVe = CultureInfo.GetCultureInfo("es-VE");

    public static string Fecha(DateTime date)
        => date.ToString("dd/MM/yyyy", EsVe);

    public static string FechaConDia(DateTime date)
    {
        // ej: "mié 21/01/2026" (sobrio, sin mayúsculas agresivas)
        return date.ToString("ddd dd/MM/yyyy", EsVe);
    }

    public static string Hora(TimeSpan time)
        => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
}

