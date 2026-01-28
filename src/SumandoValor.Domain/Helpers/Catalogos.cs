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
        { "REDES_SOCIALES", "Redes sociales" },
        { "AMIGO_FAMILIAR", "Amigos o familiares" },
        { "MEDIOS", "Medios de comunicación tradicionales" },
        { "ORGANIZACION", "Organización en la que haces parte" },
        { "FERIA", "Feria (emprendimiento o laboral)" },
        { "CORREO_KPMG", "Correo enviado por Fundación KPMG" }
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

    // Municipios por estado de Venezuela
    public static readonly Dictionary<string, List<string>> MunicipiosPorEstado = new()
    {
        { "DISTRITO_CAPITAL", new List<string> { "Libertador" } },
        { "MIRANDA", new List<string> { "Baruta", "Chacao", "El Hatillo", "Guarenas", "Guatire", "Los Salias", "Páez", "Plaza", "Sucre", "Urdaneta", "Zamora" } },
        { "CARABOBO", new List<string> { "Bejuma", "Carlos Arvelo", "Diego Ibarra", "Guacara", "Juan José Mora", "Libertador", "Los Guayos", "Miranda", "Montalbán", "Naguanagua", "Puerto Cabello", "San Diego", "San Joaquín", "Valencia" } },
        { "ARAGUA", new List<string> { "Bolívar", "Camatagua", "Francisco Linares Alcántara", "Girardot", "José Ángel Lamas", "José Félix Ribas", "José Rafael Revenga", "Libertador", "Mario Briceño Iragorry", "Ocumare de la Costa de Oro", "San Casimiro", "San Sebastián", "Santiago Mariño", "Santos Michelena", "Sucre", "Tovar", "Urdaneta", "Zamora" } },
        { "ZULIA", new List<string> { "Almirante Padilla", "Baralt", "Cabimas", "Catatumbo", "Colón", "Francisco Javier Pulgar", "Jesús Enrique Lossada", "Jesús María Semprún", "La Cañada de Urdaneta", "Lagunillas", "Machiques de Perijá", "Mara", "Maracaibo", "Miranda", "Páez", "Rosario de Perijá", "San Francisco", "Santa Rita", "Simón Bolívar", "Sucre", "Valmore Rodríguez" } },
        { "LARA", new List<string> { "Andrés Eloy Blanco", "Crespo", "Iribarren", "Jiménez", "Morán", "Palavecino", "Simón Planas", "Torres", "Urdaneta" } },
        { "ANZOATEGUI", new List<string> { "Anaco", "Aragua", "Bolívar", "Bruzual", "Cajigal", "Carvajal", "Diego Bautista Urbaneja", "Fernando de Peñalver", "Francisco del Carmen Carvajal", "Francisco de Miranda", "Guanta", "Independencia", "José Gregorio Monagas", "Juan Antonio Sotillo", "Juan Manuel Cajigal", "Libertad", "Manuel Ezequiel Bruzual", "Pedro María Freites", "Píritu", "San José de Guanipa", "San Juan de Capistrano", "Santa Ana", "Simón Bolívar", "Simón Rodríguez", "Sir Arthur Mcgregor" } },
        { "BOLIVAR", new List<string> { "Angostura", "Caroní", "Cedeño", "El Callao", "Gran Sabana", "Heres", "Padre Pedro Chien", "Piar", "Raúl Leoni", "Roscio", "Sifontes", "Sucre" } },
        { "TACHIRA", new List<string> { "Andrés Bello", "Antonio Rómulo Costa", "Ayacucho", "Bolívar", "Cárdenas", "Córdoba", "Fernández Feo", "Francisco de Miranda", "García de Hevia", "Guásimos", "Independencia", "Jáuregui", "José María Vargas", "Junín", "Libertad", "Libertador", "Lobatera", "Michelena", "Panamericano", "Pedro María Ureña", "Rafael Urdaneta", "Samuel Darío Maldonado", "San Cristóbal", "San Judas Tadeo", "Seboruco", "Simón Rodríguez", "Sucre", "Torbes", "Uribante", "Ureña" } },
        { "MERIDA", new List<string> { "Alberto Adriani", "Andrés Bello", "Antonio Pinto Salinas", "Aricagua", "Arzobispo Chacón", "Campo Elías", "Caracciolo Parra Olmedo", "Cardenal Quintero", "Guaraque", "Julio César Salas", "Justo Briceño", "Libertador", "Miranda", "Obispo Ramos de Lora", "Padre Noguera", "Pueblo Llano", "Rangel", "Rivas Dávila", "Santos Marquina", "Sucre", "Tovar", "Tulio Febres Cordero", "Zea" } },
        { "TRUJILLO", new List<string> { "Andrés Bello", "Boconó", "Bolívar", "Candelaria", "Carache", "Escuque", "José Felipe Márquez Cañizalez", "José Víctor Jiménez", "Juan Vicente Campo Elías", "La Ceiba", "Miranda", "Monte Carmelo", "Motatán", "Pampán", "Pampanito", "Rafael Rangel", "San Rafael de Carvajal", "Sucre", "Trujillo", "Urdaneta", "Valera" } },
        { "BARINAS", new List<string> { "Alberto Arvelo Torrealba", "Andrés Eloy Blanco", "Antonio José de Sucre", "Arismendi", "Barinas", "Bolívar", "Cruz Paredes", "Ezequiel Zamora", "Obispos", "Pedraza", "Rojas", "Sosa" } },
        { "PORTUGUESA", new List<string> { "Agua Blanca", "Araure", "Esteller", "Guanare", "Guanarito", "Monseñor José Vicente de Unda", "Ospino", "Páez", "Papelón", "San Genaro de Boconoíto", "San Rafael de Onoto", "Santa Rosalía", "Sucre", "Turén" } },
        { "COJEDES", new List<string> { "Anzoátegui", "Falcón", "Girardot", "Lima Blanco", "Pao de San Juan Bautista", "Ricaurte", "Rómulo Gallegos", "San Carlos", "Tinaco", "Tinaquillo" } },
        { "GUARICO", new List<string> { "Camaguán", "Chaguaramas", "El Socorro", "Francisco de Miranda", "José Félix Ribas", "José Tadeo Monagas", "Juan Germán Roscio", "Julián Mellado", "Las Mercedes", "Leonardo Infante", "Ortiz", "Pedro Zaraza", "San Gerónimo de Guayabal", "San José de Guaribe", "Santa María de Ipire" } },
        { "VARGAS", new List<string> { "Vargas" } },
        { "SUCRE", new List<string> { "Andrés Eloy Blanco", "Andrés Mata", "Arismendi", "Benítez", "Bermúdez", "Bolívar", "Cajigal", "Cruz Salmerón Acosta", "Libertador", "Mariño", "Mejía", "Montes", "Ribero", "Sucre", "Valdez" } },
        { "MONAGAS", new List<string> { "Acosta", "Aguasay", "Bolívar", "Caripe", "Cedeño", "Ezequiel Zamora", "Libertador", "Maturín", "Piar", "Punceres", "Santa Bárbara", "Sotillo", "Uracoa" } },
        { "NUEVA_ESPARTA", new List<string> { "Antolín del Campo", "Arismendi", "Díaz", "García", "Gómez", "Maneiro", "Marcano", "Mariño", "Península de Macanao", "Tubores", "Villalba" } },
        { "FALCON", new List<string> { "Acosta", "Bolívar", "Buchivacoa", "Cacique Manaure", "Carirubana", "Colina", "Dabajuro", "Democracia", "Falcón", "Federación", "Jacura", "Los Taques", "Mauroa", "Miranda", "Monseñor Iturriza", "Palmasola", "Petit", "Píritu", "San Francisco", "Silva", "Sucre", "Tocópero", "Unión", "Urumaco", "Zamora" } },
        { "YARACUY", new List<string> { "Arístides Bastidas", "Bolívar", "Bruzual", "Cocorote", "Independencia", "José Antonio Páez", "La Trinidad", "Manuel Monge", "Nirgua", "Peña", "San Felipe", "Sucre", "Urachiche", "Veroes" } },
        { "APURE", new List<string> { "Achaguas", "Biruaca", "Muñoz", "Pedro Camejo", "Rómulo Gallegos", "San Fernando", "Páez" } },
        { "AMAZONAS", new List<string> { "Alto Orinoco", "Atabapo", "Atures", "Autana", "Manapiare", "Maroa", "Río Negro" } },
        { "DELTA_AMACURO", new List<string> { "Antonio Díaz", "Casacoima", "Pedernales", "Tucupita" } }
    };
}
