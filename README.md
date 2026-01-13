# Sumando Valor - Plataforma Web

Plataforma web para la gestión de talleres y capacitaciones de la Fundación KPMG Venezuela.

## Requisitos

- .NET 8 SDK
- SQL Server (LocalDB o SQL Server Express)
- Visual Studio 2022 (recomendado) o Visual Studio Code

## Estructura del Proyecto

```
SumandoValor/
├── src/
│   ├── SumandoValor.Web/          # Aplicación Razor Pages (UI)
│   ├── SumandoValor.Infrastructure/ # EF Core, servicios externos
│   ├── SumandoValor.Application/   # Casos de uso/servicios
│   └── SumandoValor.Domain/       # Entidades de dominio
└── tests/
    └── SumandoValor.Tests/         # Tests xUnit
```

## Configuración Inicial

### 1. Base de Datos

Asegúrate de que SQL Server esté corriendo. La cadena de conexión por defecto usa LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SumandoValor;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Para usar SQL Server Express o una instancia local, modifica `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SumandoValor;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 2. Crear la Base de Datos

Desde la consola de administración de paquetes en Visual Studio, o desde la terminal:

```powershell
cd src/SumandoValor.Web
dotnet ef migrations add InitialCreate --project ../SumandoValor.Infrastructure
dotnet ef database update --project ../SumandoValor.Infrastructure
```

**Nota:** Si no tienes la herramienta EF Core CLI instalada:

```powershell
dotnet tool install --global dotnet-ef
```

### 3. Usuario Administrador

El sistema crea automáticamente un usuario administrador al iniciar por primera vez. Las credenciales por defecto están en `appsettings.Development.json`:

- **Email:** admin@sumandovalor.org
- **Contraseña:** Admin123!

Puedes cambiar estas credenciales editando `appsettings.Development.json`:

```json
"Seed": {
  "AdminEmail": "tu-email@ejemplo.com",
  "AdminPassword": "TuPassword123!"
}
```

## Ejecutar la Aplicación

### Desde Visual Studio

1. Abre `SumandoValor.sln`
2. Establece `SumandoValor.Web` como proyecto de inicio
3. Presiona F5 o ejecuta con Ctrl+F5

### Desde la Terminal

```powershell
cd src/SumandoValor.Web
dotnet run
```

La aplicación estará disponible en `https://localhost:5001` o `http://localhost:5000`.

## Funcionalidades Implementadas (Paso 0)

### Autenticación
- ✅ Registro de usuarios (Beneficiario)
- ✅ Inicio de sesión
- ✅ Confirmación de email (simulada en Development)
- ✅ Roles: Admin y Beneficiario
- ✅ Política de contraseñas (mínimo 8 caracteres)

### Base de Datos
- ✅ Identity extendido con campos adicionales para Beneficiario
- ✅ Tablas de dominio: Taller, Inscripción, Certificado, Encuesta, MensajeContacto
- ✅ Índices básicos para optimización

### Páginas Base
- ✅ Home (placeholder)
- ✅ Login/Register
- ✅ Oferta de Talleres (placeholder)
- ✅ Conócenos (placeholder)
- ✅ Contáctanos (formulario básico)
- ✅ Perfil (requiere autenticación)
- ✅ Admin (requiere rol Admin)

### Seguridad
- ✅ Antiforgery tokens
- ✅ Validación server-side
- ✅ Headers de seguridad (HSTS en producción)
- ✅ Cookies seguras

### Servicios Preparados
- ✅ Captcha (Mock en Development, Cloudflare Turnstile preparado para Production)
- ✅ Email (simulado en Development, listo para SMTP)

## Desarrollo

### Email en Development

En modo Development, los emails se simulan y se muestran en la consola/logs. Para ver el link de confirmación, revisa la salida de la consola después de registrarte.

### Migraciones

Para crear una nueva migración:

```powershell
dotnet ef migrations add NombreMigracion --project src/SumandoValor.Infrastructure --startup-project src/SumandoValor.Web
```

Para aplicar migraciones:

```powershell
dotnet ef database update --project src/SumandoValor.Infrastructure --startup-project src/SumandoValor.Web
```

## Publicación en IIS

### Prerequisitos

1. .NET 8 Hosting Bundle instalado en el servidor
2. IIS configurado con Application Pool usando .NET CLR Version "No Managed Code"

### Pasos

1. Publicar desde Visual Studio:
   - Click derecho en `SumandoValor.Web` → Publish
   - Selecciona "Folder" o "IIS"
   - Configura el perfil y publica

2. O desde la terminal:
   ```powershell
   dotnet publish src/SumandoValor.Web -c Release -o ./publish
   ```

3. Copiar archivos al servidor IIS

4. Configurar Application Pool:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

5. Configurar `web.config` si es necesario (se genera automáticamente)

## Próximos Pasos (Paso 1)

- Implementar formulario completo de registro con todos los campos del Beneficiario
- Integración real de captcha (Cloudflare Turnstile o reCAPTCHA)
- Configuración de SMTP para emails
- CRUD completo de talleres (Admin)
- Gestión de inscripciones

## Notas

- El proyecto usa Bootstrap 5 para el UI (incluido por defecto)
- Tailwind CSS puede configurarse en el futuro si se requiere
- La paleta de colores corporativa se puede personalizar en `wwwroot/css/site.css`

## Licencia

Propiedad de Fundación KPMG Venezuela.
