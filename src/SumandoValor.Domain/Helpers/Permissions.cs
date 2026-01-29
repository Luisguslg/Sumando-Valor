namespace SumandoValor.Domain.Helpers;

public static class Permissions
{
    public const string ClaimType = "Permission";

    // Permisos para Cursos
    public const string Cursos_Listar = "Cursos.Listar";
    public const string Cursos_Crear = "Cursos.Crear";
    public const string Cursos_Editar = "Cursos.Editar";
    public const string Cursos_Eliminar = "Cursos.Eliminar";

    // Permisos para Talleres
    public const string Talleres_Listar = "Talleres.Listar";
    public const string Talleres_Crear = "Talleres.Crear";
    public const string Talleres_Editar = "Talleres.Editar";
    public const string Talleres_Eliminar = "Talleres.Eliminar";

    // Permisos para Usuarios
    public const string Usuarios_Listar = "Usuarios.Listar";
    public const string Usuarios_Crear = "Usuarios.Crear";
    public const string Usuarios_Editar = "Usuarios.Editar";
    public const string Usuarios_Eliminar = "Usuarios.Eliminar";

    // Permisos para Inscripciones
    public const string Inscripciones_Listar = "Inscripciones.Listar";
    public const string Inscripciones_Crear = "Inscripciones.Crear";
    public const string Inscripciones_Editar = "Inscripciones.Editar";
    public const string Inscripciones_Eliminar = "Inscripciones.Eliminar";

    // Permisos para Certificados
    public const string Certificados_Listar = "Certificados.Listar";
    public const string Certificados_Crear = "Certificados.Crear";
    public const string Certificados_Editar = "Certificados.Editar";
    public const string Certificados_Eliminar = "Certificados.Eliminar";

    // Permisos para Encuestas
    public const string Encuestas_Listar = "Encuestas.Listar";
    public const string Encuestas_Crear = "Encuestas.Crear";
    public const string Encuestas_Editar = "Encuestas.Editar";
    public const string Encuestas_Eliminar = "Encuestas.Eliminar";

    // Permisos para Roles
    public const string Roles_Listar = "Roles.Listar";
    public const string Roles_Crear = "Roles.Crear";
    public const string Roles_Editar = "Roles.Editar";
    public const string Roles_Eliminar = "Roles.Eliminar";

    // Permisos para Auditoría
    public const string Auditoria_Ver = "Auditoria.Ver";

    // Obtener todos los permisos agrupados por módulo
    public static Dictionary<string, List<string>> GetAllPermissions()
    {
        return new Dictionary<string, List<string>>
        {
            ["Cursos"] = new List<string> { Cursos_Listar, Cursos_Crear, Cursos_Editar, Cursos_Eliminar },
            ["Talleres"] = new List<string> { Talleres_Listar, Talleres_Crear, Talleres_Editar, Talleres_Eliminar },
            ["Usuarios"] = new List<string> { Usuarios_Listar, Usuarios_Crear, Usuarios_Editar, Usuarios_Eliminar },
            ["Inscripciones"] = new List<string> { Inscripciones_Listar, Inscripciones_Crear, Inscripciones_Editar, Inscripciones_Eliminar },
            ["Certificados"] = new List<string> { Certificados_Listar, Certificados_Crear, Certificados_Editar, Certificados_Eliminar },
            ["Encuestas"] = new List<string> { Encuestas_Listar, Encuestas_Crear, Encuestas_Editar, Encuestas_Eliminar },
            ["Roles"] = new List<string> { Roles_Listar, Roles_Crear, Roles_Editar, Roles_Eliminar },
            ["Auditoría"] = new List<string> { Auditoria_Ver }
        };
    }

    // Permisos por defecto para cada rol
    public static List<string> GetDefaultPermissionsForRole(string roleName)
    {
        return roleName switch
        {
            "Admin" => GetAllPermissions().Values.SelectMany(p => p).ToList(),
            "Moderador" => new List<string>
            {
                // Moderador puede hacer todo excepto Roles y Auditoría
                Cursos_Listar, Cursos_Crear, Cursos_Editar, Cursos_Eliminar,
                Talleres_Listar, Talleres_Crear, Talleres_Editar, Talleres_Eliminar,
                Usuarios_Listar, Usuarios_Crear, Usuarios_Editar, Usuarios_Eliminar,
                Inscripciones_Listar, Inscripciones_Crear, Inscripciones_Editar, Inscripciones_Eliminar,
                Certificados_Listar, Certificados_Crear, Certificados_Editar, Certificados_Eliminar,
                Encuestas_Listar, Encuestas_Crear, Encuestas_Editar, Encuestas_Eliminar
                // NO incluye: Roles_*, Auditoria_Ver
            },
            "Beneficiario" => new List<string>
            {
                Cursos_Listar,
                Talleres_Listar,
                Inscripciones_Listar, Inscripciones_Crear
            },
            _ => new List<string>()
        };
    }
}
