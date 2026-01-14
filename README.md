# Sumando Valor - Plataforma Web

Plataforma web (Razor Pages + .NET 8) para la gestión de cursos, talleres e inscripciones de la Fundación KPMG Venezuela.

## Requisitos

- .NET 8 SDK
- SQL Server (LocalDB o instancia local)
- Visual Studio 2022 (recomendado)

## Estructura del proyecto

```
/
├── SumandoValor.sln
├── src/
│   ├── SumandoValor.Web/            # UI (Razor Pages)
│   ├── SumandoValor.Infrastructure/ # EF Core, Identity, migraciones, servicios
│   ├── SumandoValor.Application/    # Servicios/casos de uso (ligero)
│   └── SumandoValor.Domain/         # Entidades de dominio
└── tests/
    └── SumandoValor.Tests/          # xUnit
```

## Base de datos / migraciones

Aplicar migraciones:

```powershell
cd src/SumandoValor.Infrastructure
dotnet ef database update --startup-project ../SumandoValor.Web
```

Nota: si tienes la app corriendo, `dotnet build` / `dotnet ef` pueden fallar por archivos bloqueados. Detén `dotnet run` antes de compilar o migrar.

## Ejecutar la aplicación

```powershell
cd src/SumandoValor.Web
dotnet run
```

## Usuario administrador (Development)

Al iniciar en Development se crea un admin desde `appsettings.Development.json`:

- Email: `admin@sumandovalor.org`
- Password: `Admin123!`

## Bandeja de correos (Development)

- `/Dev/Emails` muestra enlaces de confirmación y reset (solo Development).

## Rutas principales

### Público
- `/`
- `/Cursos`
- `/Cursos/{id}`
- `/Talleres/{id}`
- `/Contact`

### Cuenta / Perfil
- `/Account/Login`
- `/Account/Register`
- `/Profile`
- `/Profile/Talleres`

### Admin
- `/Admin`
- `/Admin/Cursos`
- `/Admin/Talleres`
- `/Admin/Inscripciones`

## Notas QA (interno)

Notas de verificación interna: `docs/qa-notes.md`.

