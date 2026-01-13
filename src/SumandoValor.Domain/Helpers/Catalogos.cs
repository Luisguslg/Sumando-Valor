namespace SumandoValor.Domain.Helpers;

public static class Catalogos
{
    public static readonly Dictionary<string, string> Sexos = new()
    {
        { "M", "Masculino" },
        { "F", "Femenino" },
        { "O", "Otro" }
    };

    public static readonly Dictionary<string, string> NivelesEducativos = new()
    {
        { "PRIMARIA", "Primaria" },
        { "SECUNDARIA", "Secundaria" },
        { "TECNICO", "Técnico" },
        { "UNIVERSITARIO", "Universitario" },
        { "POSTGRADO", "Postgrado" },
        { "NINGUNO", "Ninguno" }
    };

    public static readonly Dictionary<string, string> SituacionesLaborales = new()
    {
        { "EMPLEADO", "Empleado" },
        { "DESEMPLEADO", "Desempleado" },
        { "INDEPENDIENTE", "Independiente" },
        { "ESTUDIANTE", "Estudiante" },
        { "JUBILADO", "Jubilado" },
        { "OTRO", "Otro" }
    };

    public static readonly Dictionary<string, string> CanalesConocio = new()
    {
        { "REDES_SOCIALES", "Redes Sociales" },
        { "AMIGO_FAMILIAR", "Amigo o Familiar" },
        { "MEDIOS", "Medios de Comunicación" },
        { "ORGANIZACION", "Organización" },
        { "OTRO", "Otro" }
    };

    public static readonly Dictionary<string, string> EstadosVenezuela = new()
    {
        { "AMAZONAS", "Amazonas" },
        { "ANZOATEGUI", "Anzoátegui" },
        { "APURE", "Apure" },
        { "ARAGUA", "Aragua" },
        { "BARINAS", "Barinas" },
        { "BOLIVAR", "Bolívar" },
        { "CARABOBO", "Carabobo" },
        { "COJEDES", "Cojedes" },
        { "DELTA_AMACURO", "Delta Amacuro" },
        { "DISTRITO_CAPITAL", "Distrito Capital" },
        { "FALCON", "Falcón" },
        { "GUARICO", "Guárico" },
        { "LARA", "Lara" },
        { "MERIDA", "Mérida" },
        { "MIRANDA", "Miranda" },
        { "MONAGAS", "Monagas" },
        { "NUEVA_ESPARTA", "Nueva Esparta" },
        { "PORTUGUESA", "Portuguesa" },
        { "SUCRE", "Sucre" },
        { "TACHIRA", "Táchira" },
        { "TRUJILLO", "Trujillo" },
        { "VARGAS", "Vargas" },
        { "YARACUY", "Yaracuy" },
        { "ZULIA", "Zulia" }
    };

    public static readonly Dictionary<string, string> CiudadesVenezuela = new()
    {
        { "CARACAS", "Caracas" },
        { "MARACAIBO", "Maracaibo" },
        { "VALENCIA", "Valencia" },
        { "BARQUISIMETO", "Barquisimeto" },
        { "MARACAY", "Maracay" },
        { "CIUDAD_GUAYANA", "Ciudad Guayana" },
        { "BARCELONA", "Barcelona" },
        { "MÉRIDA", "Mérida" },
        { "PUERTO_LA_CRUZ", "Puerto La Cruz" },
        { "MATURIN", "Maturín" },
        { "SAN_CRISTOBAL", "San Cristóbal" },
        { "CIUDAD_BOLIVAR", "Ciudad Bolívar" },
        { "BARINAS", "Barinas" },
        { "CUMANA", "Cumaná" },
        { "CORO", "Coro" },
        { "PUERTO_CABELLO", "Puerto Cabello" },
        { "VALERA", "Valera" },
        { "GUARENAS", "Guarenas" },
        { "LOS_TEQUES", "Los Teques" },
        { "CABIMAS", "Cabimas" },
        { "OTRA", "Otra" }
    };
}
