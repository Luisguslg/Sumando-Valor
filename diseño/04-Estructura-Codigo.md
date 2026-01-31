# Estructura del Código Fuente - Sumando Valor

## Organización de Carpetas

```
Sumando Valor APP/
├── src/
│   ├── SumandoValor.Domain/          # Capa de Dominio
│   │   ├── Entities/                 # Entidades del dominio
│   │   │   ├── Curso.cs
│   │   │   ├── Taller.cs
│   │   │   ├── Inscripcion.cs
│   │   │   ├── Certificado.cs
│   │   │   ├── EncuestaSatisfaccion.cs
│   │   │   ├── MensajeContacto.cs
│   │   │   ├── CarouselItem.cs
│   │   │   ├── SiteImage.cs
│   │   │   ├── AuditLog.cs
│   │   │   └── Surveys/              # Sistema de encuestas
│   │   │       ├── SurveyTemplate.cs
│   │   │       ├── SurveyQuestion.cs
│   │   │       ├── SurveyResponse.cs
│   │   │       └── SurveyAnswer.cs
│   │   └── Helpers/
│   │       └── Catalogos.cs
│   │
│   ├── SumandoValor.Infrastructure/   # Capa de Infraestructura
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs       # Contexto de EF Core
│   │   │   ├── ApplicationUser.cs    # Usuario extendido de Identity
│   │   │   └── DbInitializer.cs      # Inicialización de BD
│   │   ├── Services/
│   │   │   ├── IEmailService.cs
│   │   │   ├── DevelopmentEmailService.cs
│   │   │   ├── SmtpEmailService.cs
│   │   │   ├── ICaptchaValidator.cs
│   │   │   ├── MockCaptchaValidator.cs
│   │   │   ├── CloudflareTurnstileCaptchaValidator.cs
│   │   │   ├── SmtpEmailOptions.cs
│   │   │   ├── IDevEmailStore.cs
│   │   │   ├── FileDevEmailStore.cs
│   │   │   └── InMemoryDevEmailStore.cs
│   │   └── Migrations/               # Migraciones de EF Core
│   │       └── [múltiples archivos de migración]
│   │
│   ├── SumandoValor.Web/             # Capa de Presentación
│   │   ├── Pages/                    # Razor Pages
│   │   │   ├── Account/              # Autenticación
│   │   │   │   ├── Login.cshtml / .cs
│   │   │   │   ├── Register.cshtml / .cs
│   │   │   │   ├── Logout.cshtml / .cs
│   │   │   │   ├── ForgotPassword.cshtml / .cs
│   │   │   │   ├── ResetPassword.cshtml / .cs
│   │   │   │   ├── ConfirmEmail.cshtml / .cs
│   │   │   │   └── AccessDenied.cshtml / .cs
│   │   │   ├── Admin/                # Panel de administración
│   │   │   │   ├── Admin.cshtml / .cs
│   │   │   │   ├── Cursos/
│   │   │   │   │   ├── Index.cshtml / .cs
│   │   │   │   │   ├── Create.cshtml / .cs
│   │   │   │   │   └── Edit.cshtml / .cs
│   │   │   │   ├── Talleres/
│   │   │   │   │   ├── Index.cshtml / .cs
│   │   │   │   │   ├── Create.cshtml / .cs
│   │   │   │   │   └── Edit.cshtml / .cs
│   │   │   │   ├── Inscripciones.cshtml / .cs
│   │   │   │   ├── Certificados.cshtml / .cs
│   │   │   │   ├── Encuestas.cshtml / .cs
│   │   │   │   ├── Usuarios.cshtml / .cs
│   │   │   │   ├── Usuarios/
│   │   │   │   │   └── Edit.cshtml / .cs
│   │   │   │   ├── Carrusel.cshtml / .cs
│   │   │   │   ├── Imagenes.cshtml / .cs
│   │   │   │   ├── SurveyTemplates/
│   │   │   │   │   ├── Index.cshtml / .cs
│   │   │   │   │   ├── Create.cshtml / .cs
│   │   │   │   │   └── Edit.cshtml / .cs
│   │   │   │   └── EmailDiagnostics.cshtml / .cs
│   │   │   ├── Profile/              # Perfil de usuario
│   │   │   │   ├── Profile.cshtml / .cs
│   │   │   │   ├── Talleres.cshtml / .cs
│   │   │   │   └── Encuesta.cshtml / .cs
│   │   │   ├── Cursos/               # Público
│   │   │   │   ├── Index.cshtml / .cs
│   │   │   │   └── Details.cshtml / .cs
│   │   │   ├── Talleres/
│   │   │   │   └── Details.cshtml / .cs
│   │   │   ├── Certificates/
│   │   │   │   └── Download.cshtml / .cs
│   │   │   ├── Shared/               # Componentes compartidos
│   │   │   │   ├── _Layout.cshtml
│   │   │   │   ├── _AdminLayout.cshtml
│   │   │   │   ├── _PageHeader.cshtml
│   │   │   │   ├── _WorkshopCard.cshtml
│   │   │   │   ├── _Flash.cshtml
│   │   │   │   └── [otros partials]
│   │   │   ├── Index.cshtml / .cs    # Homepage
│   │   │   ├── About.cshtml / .cs
│   │   │   ├── Contact.cshtml / .cs
│   │   │   ├── Error.cshtml / .cs
│   │   │   └── Dev/                  # Solo desarrollo
│   │   │       └── Emails.cshtml / .cs
│   │   ├── Services/
│   │   │   ├── Certificates/
│   │   │   │   └── CertificatePdfGenerator.cs
│   │   │   └── UploadCleanupService.cs
│   │   ├── Helpers/
│   │   │   └── DateFormat.cs
│   │   ├── wwwroot/                  # Archivos estáticos
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   ├── images/
│   │   │   │   ├── certificates/
│   │   │   │   ├── cards/
│   │   │   │   ├── hero/
│   │   │   │   └── sections/
│   │   │   ├── lib/                  # Librerías (Bootstrap, etc.)
│   │   │   ├── theme/
│   │   │   └── uploads/              # Uploads de usuarios
│   │   │       ├── carousel/
│   │   │       └── site/
│   │   ├── Program.cs                # Configuración de la app
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   └── SumandoValor.Tests/           # Tests unitarios
│       ├── Domain/
│       ├── Services/
│       └── SumandoValor.Tests.csproj
│
├── Imágenes/                         # Recursos de diseño
│   ├── Certificados.png
│   └── [otras imágenes]
│
├── ops/                              # Documentación operativa
│   ├── BD_CONEXIONES_LOCAL_Y_REMOTA.txt
│   └── COMANDOS_BD_LOCAL.txt
│
├── diseño/                           # Documentación técnica (esta carpeta)
│
└── README.md
```

## Convenciones de Nomenclatura

### Archivos C#
- **Clases**: PascalCase (ej: `CertificatePdfGenerator.cs`)
- **Interfaces**: I + PascalCase (ej: `IEmailService.cs`)
- **Enums**: PascalCase (ej: `EstatusTaller`)
- **Métodos**: PascalCase (ej: `OnGetAsync()`)
- **Propiedades**: PascalCase (ej: `TotalCursosActivos`)
- **Campos privados**: _camelCase (ej: `_context`)

### Archivos Razor
- **Páginas**: PascalCase (ej: `Admin.cshtml`)
- **Partials**: _PascalCase (ej: `_Layout.cshtml`)
- **Layouts**: _PascalCase (ej: `_AdminLayout.cshtml`)

### Base de Datos
- **Tablas**: PascalCase (ej: `AspNetUsers`, `Talleres`)
- **Columnas**: PascalCase (ej: `FechaInicio`, `CuposMaximos`)
- **Índices**: `IX_Tabla_Columna` (ej: `IX_Talleres_Estatus`)
- **Foreign Keys**: `FK_TablaHija_TablaPadre_Columna` (ej: `FK_Talleres_Cursos_CursoId`)

## Patrones de Diseño Utilizados

### Repository Pattern (implícito)
- `AppDbContext` actúa como repositorio genérico
- Acceso a datos centralizado a través de DbSets

### Service Pattern
- Servicios inyectados por DI (`IEmailService`, `ICaptchaValidator`)
- Separación de responsabilidades

### Page Model Pattern
- Cada Razor Page tiene su Page Model asociado
- Lógica de negocio en el Page Model, presentación en la vista

### Factory Pattern (parcial)
- `DevelopmentEmailService` vs `SmtpEmailService` según entorno
- `MockCaptchaValidator` vs `CloudflareTurnstileCaptchaValidator` según configuración

## Estructura de Proyectos

### SumandoValor.Domain
- **Propósito**: Entidades puras del dominio, sin dependencias externas
- **Dependencias**: Ninguna (proyecto de biblioteca estándar)

### SumandoValor.Infrastructure
- **Propósito**: Implementaciones técnicas (EF Core, Identity, servicios externos)
- **Dependencias**: Domain

### SumandoValor.Web
- **Propósito**: Capa de presentación (Razor Pages, configuración)
- **Dependencias**: Domain, Infrastructure

### SumandoValor.Tests
- **Propósito**: Tests unitarios e integración
- **Dependencias**: Todos los proyectos

## Configuración y Settings

### appsettings.json
- Connection strings
- Configuración de email (SMTP)
- Configuración de captcha
- Seed data (usuarios iniciales)

### Program.cs
- Registro de servicios (DI)
- Configuración de middleware
- Configuración de Identity
- Configuración de seguridad (headers, cookies)

## Archivos de Migración

Las migraciones de Entity Framework se encuentran en:
`src/SumandoValor.Infrastructure/Migrations/`

Cada migración incluye:
- `Up()`: Cambios a aplicar
- `Down()`: Reversión de cambios
- `AppDbContextModelSnapshot.cs`: Estado actual del modelo
