# Sumando Valor - Plataforma Web

Plataforma web (Razor Pages + .NET 8) para la gestión de cursos, talleres, inscripciones, encuestas y certificados de la Fundación KPMG Venezuela.

## Roles y flujos

- **Admin**: gestiona cursos/talleres/inscripciones, revisa encuestas, aprueba certificados, administra usuarios, diagnostica correos.
- **Beneficiario**: se registra, confirma email, se inscribe a talleres, responde encuestas (si aplica) y descarga certificados (si aplica).

Rutas clave:
- **Público**: `/`, `/Cursos`, `/Cursos/{id}`, `/Talleres/{id}`, `/Contact`
- **Cuenta/Perfil**: `/Account/Login`, `/Account/Register`, `/Profile`, `/Profile/Talleres`
- **Admin**: `/Admin/*` (Cursos, Talleres, Inscripciones, Encuestas, Certificados, Usuarios, EmailDiagnostics)

## Arquitectura (Producción)

Diagrama textual:

```
Usuario (Browser)
   │
   ▼
IIS (ASP.NET Core Module)  ─────────►  Kestrel (.NET 8 / Razor Pages)
   │                                     │
   │                                     ├─ EF Core (migraciones) ──► SQL Server remoto
   │                                     ├─ SMTP (relay o auth) ────► Servidor SMTP corporativo
   │                                     └─ Archivos (PDF) ─────────► C:\inetpub\sumandovalor\App_Data\Certificates
   │
   └─ Static files (wwwroot)
```

## PARTE 1 — Auditoría actual (obligatorio)

### 1.1 ¿Cómo se crea hoy la base de datos?

- **Migrations EF Core** viven en: `src/SumandoValor.Infrastructure/Migrations`.
- En el arranque, la app ejecuta **migraciones automáticamente**:

```98:116:src/SumandoValor.Web/Program.cs
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await DbInitializer.InitializeAsync(context, userManager, roleManager, configuration, app.Environment.IsDevelopment());
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos.");

        // In production we fail fast: DB connectivity/migrations must be fixed before serving traffic.
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}
```

Y `DbInitializer` aplica migraciones y siembra roles:

```7:27:src/SumandoValor.Infrastructure/Data/DbInitializer.cs
public static async Task InitializeAsync(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    bool isDevelopment)
{
    await context.Database.MigrateAsync();

    await SeedRolesAsync(roleManager);
    await SeedAdminUserAsync(userManager, configuration, isDevelopment);
}
```

### 1.2 ¿Dónde están las connection strings?

- `src/SumandoValor.Web/appsettings.json` → `ConnectionStrings:DefaultConnection`
- Se puede (y se debe en IIS) sobreescribir por **variables de entorno**:
  - `ConnectionStrings__DefaultConnection`

### 1.3 ¿Qué implementación de correo existe hoy?

- Abstracción: `SumandoValor.Infrastructure.Services.IEmailService`
- **Development**: `DevelopmentEmailService` guarda emails en disco (`App_Data/DevEmails`) y se ven en `/Dev/Emails`.
- **Producción/IIS**: `SmtpEmailService` (SMTP real), configurable por `Email:Smtp:*`.

### 1.4 ¿Dónde se generan y guardan los PDFs?

- Se generan con QuestPDF en: `src/SumandoValor.Web/Services/Certificates/CertificatePdfGenerator.cs`.
- Se guardan en: `App_Data/Certificates` (fuera de `wwwroot`).
- Descarga segura: `/Certificates/Download?id={certId}` valida:
  - Certificado aprobado
  - Usuario dueño o Admin
  - Ruta bajo `App_Data/Certificates`

### 1.5 ¿Por qué “se sobrescribían” los PDFs?

- Antes el nombre podía ser determinístico por certificado (o regenerado sobre el mismo archivo).
- **Fix**: ahora se genera un **nombre único por emisión** (incluye `certId`, `tallerId`, `userId` y `Guid`) y nunca se sobrescribe; si existía un PDF anterior, se elimina antes de apuntar al nuevo.

## PARTE 2 — Estrategia de deploy (diseño)

Objetivo: un deploy repetible donde:
- La app se publica desde tu PC (`dotnet publish` Release).
- Se copia al servidor por RDP.
- IIS configura variables de entorno (sin secretos en Git).
- SQL Server remoto se prepara desde tu PC (con `dotnet ef` apuntando remoto) o automáticamente al primer arranque.
- PDFs se guardan en carpeta segura y con permisos NTFS correctos.
- SMTP real queda verificable con `/Admin/EmailDiagnostics`.

## PARTE 3 — Deploy a IIS (paso a paso)

### Deploy “mínimo esfuerzo” (recomendado)

En este repo hay 2 scripts que automatizan casi todo:
- `deploy/01_publish.ps1` (en tu PC): genera `artifacts/SumandoValor_publish.zip`
- `deploy/02_iis_install.ps1` (en el servidor): descomprime, crea carpetas/permisos, configura IIS y variables (BD + correo), reinicia IIS

Uso:
1) En tu PC:
   - Ejecuta `deploy/01_publish.ps1`
   - Copia `artifacts/SumandoValor_publish.zip` al servidor (ej. `C:\Temp\SumandoValor_publish.zip`)
2) En el servidor (PowerShell Admin):
   - Ejecuta `deploy/02_iis_install.ps1` pasando `SqlUser`/`SqlPassword` (SQL Auth)

Qué se despliega:
- **Solo** el contenido del ZIP (equivalente a `dotnet publish`).
Qué NO se despliega:
- `src/`, `tests/`, `.git/`, `.vs/`, etc.

### 3.0 Requisitos del servidor (una sola vez)

En el servidor IIS:
- Instalar **.NET 8 Hosting Bundle** (incluye ASP.NET Core Module para IIS).
- Habilitar IIS con **Static Content**.

### 3.1 Publicación desde tu PC (Release)

Desde la raíz del repo:

```powershell
dotnet publish src\SumandoValor.Web\SumandoValor.Web.csproj -c Release -o .\publish
```

Se genera la carpeta `.\publish` (no se sube a git).

**No copies** secretos ni configs locales al repo. En el servidor, configura por env vars o un `appsettings.Production.json` local (ignorado por git).

### 3.2 Copiar al servidor (RDP)

Recomendado:
- Carpeta destino: `C:\inetpub\sumandovalor\`
- Copiar el contenido de `.\publish\` → `C:\inetpub\sumandovalor\app\` (por ejemplo)
- Crear carpeta: `C:\inetpub\sumandovalor\App_Data\Certificates\`

Permisos NTFS (crítico):
- Dar **Modify** a la identidad del App Pool (ej. `IIS AppPool\SumandoValor`) sobre:
  - `C:\inetpub\sumandovalor\App_Data\Certificates\`
  - (Opcional) `C:\inetpub\sumandovalor\logs\` si habilitas stdout logs

### 3.3 Configuración IIS (sitio)

1) Crear **Application Pool**
- .NET CLR: **No Managed Code**
- Pipeline: Integrated
- Identity: ApplicationPoolIdentity (ok)

2) Crear **Sitio**
- Physical path: `C:\inetpub\sumandovalor\app\`
- Binding: host/puerto según IT
- Ideal: HTTPS con certificado

### 3.4 Variables de entorno / config (IIS)

Configurar variables en el **App Pool** (recomendado) o en `web.config`:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=...`
- `Email__Smtp__Enabled=true`
- `Email__Smtp__Host=...`
- `Email__Smtp__Port=25` (relay) o `587` (auth)
- `Email__Smtp__EnableSsl=false/true`
- `Email__Smtp__UseDefaultCredentials=false/true`
- `Email__Smtp__User=...` (solo si SMTP autenticado)
- `Email__Smtp__Password=...` (solo si SMTP autenticado)
- `Email__Smtp__FromAddress=...`
- `Email__Smtp__FromName=Sumando Valor`
- `Email__ContactRecipient=fundacionkpmg@kpmg.com`
- `Seed__CreateAdmin=false` (recomendado)
- `Seed__AdminPassword=...` (si `Seed__CreateAdmin=true`, obligatorio en prod)

## PARTE 4 — SQL Server remoto

### 4.1 Crear/actualizar BD desde tu PC (recomendado)

Opción A (controlado): ejecutar migraciones desde tu PC hacia el SQL remoto:

```powershell
dotnet ef database update `
  --project src\SumandoValor.Infrastructure `
  --startup-project src\SumandoValor.Web `
  --connection "Server=SERVIDOR;Database=SumandoValorDb;User Id=...;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

Opción B (automático): la app en producción ejecuta `Database.Migrate()` al iniciar. Si la conexión es correcta, crea/actualiza la BD sola.

### 4.2 Autenticación (Windows vs SQL)

- **Windows Auth**: el App Pool ejecuta como `ApplicationPoolIdentity` (máquina local). Para usar Windows Auth real, normalmente se requiere cuenta de dominio y configurar SPN/Delegation (depende de IT).
- **SQL Auth (más simple)**: crear login SQL y otorgar permisos en la DB.

Permisos mínimos para EF migrations: típicamente **db_owner** durante migraciones (o permisos DDL equivalentes).

## PARTE 5 — Email real (producción)

### 5.0 Cómo conseguir los datos SMTP (si no los tienes)

En este repo **no guardamos credenciales SMTP** (por seguridad). La carpeta `Ejemplo/` que existe en la raíz **no contiene SMTP**; su `web.config` es de archivos estáticos.

Para obtener SMTP real necesitas uno de estos 3 caminos:

1) **IT / Infra (recomendado)**: pedir un “SMTP relay” para la app.
   - Te deben dar: **Host**, **Puerto** (25 normalmente), si usa **SSL/TLS**, y si requiere **auth**.
   - Ideal corporativo: **relay sin auth** restringido por **IP del servidor IIS** (no relay abierto).

2) **Revisar un despliegue anterior** (si existe):
   - En el servidor, buscar en la carpeta del sitio por:
     - `appsettings.Production.json`, `appsettings.json`, `web.config`
   - En IIS Manager:
     - Sitio/AppPool → **Configuration Editor** → `system.webServer/aspNetCore` → `environmentVariables`
     - Ahí suelen estar `Email__Smtp__Host`, `Email__Smtp__User`, etc.

3) **Correo autenticado (Office365/Exchange/SMTP corporativo)**:
   - IT te debe dar usuario/clave (o cuenta de servicio) + host/puerto/TLS.

### 5.0.1 Si vienes del proyecto antiguo (Ejemplo/Web.config)

En `Ejemplo/Web.config` se usaban keys “legacy” como:
- `ServidorCorreo` (host)
- `PuertoCorreo` (port)
- `EnviarCorreo` (enabled)
- `CorreoDeServicios` (from / user)
- `CorreoPassword` (password)
- `Enablessl` (ssl)

En esta app .NET 8 ya soportamos esos keys como **fallback**, por lo que en IIS puedes configurar **o** `Email__Smtp__*` **o** esos legacy keys como variables de entorno.

### 5.1 Diagnóstico (antes de culpar a la app)

Desde el servidor IIS (PowerShell) prueba conectividad:

```powershell
Test-NetConnection -ComputerName <SMTP_HOST> -Port 25
Test-NetConnection -ComputerName <SMTP_HOST> -Port 587
```

Si esto falla:
- Es **red/firewall** o el host/puerto es incorrecto.
- La app no puede enviar emails si el servidor no puede conectar al SMTP.

### 5.1 Diagnóstico

Hay dos escenarios típicos corporativos:

- **Opción A (preferida si IT lo permite)**: SMTP relay interno por IP (puerto 25), sin auth.
  - Riesgo: si no restringen por IP, es un relay abierto (NO aceptable).
  - Requisito: IT debe **restringir por IP** (la IP del servidor IIS) y/o por red.

- **Opción B (fallback)**: SMTP autenticado (587/TLS).

### 5.2 Configuración recomendada

**A) Relay interno sin auth**
- `Email__Smtp__Port=25`
- `Email__Smtp__EnableSsl=false` (según IT)
- `Email__Smtp__User=` (vacío)
- `Email__Smtp__UseDefaultCredentials=false`

**B) SMTP autenticado**
- `Email__Smtp__Port=587`
- `Email__Smtp__EnableSsl=true`
- `Email__Smtp__User=...`
- `Email__Smtp__Password=...`

Prueba: `/Admin/EmailDiagnostics`

### 5.3 “Debe enviar de verdad”: checklist rápido

Para que **sí** envíe real en producción deben cumplirse:
- `ASPNETCORE_ENVIRONMENT=Production`
- `Email__Smtp__Enabled=true`
- Host/Port correctos y accesibles desde el servidor (Test-NetConnection OK)
- Si es relay por IP: IT debe permitir la IP del IIS
- Si es autenticado: user/password válidos
- `Email__Smtp__FromAddress` permitido por el servidor SMTP (algunas orgs lo restringen)

## PARTE 6 — PDFs persistentes y seguros (crítico)

- Carpeta: `App_Data/Certificates` (NO pública).
- Nombre: `cert_{certId}_{tallerId}_{userId}_{guid}.pdf` (no colisiona, no sobrescribe).
- Descarga: endpoint protegido `/Certificates/Download`.

## PARTE 7 — Checklist de pruebas en producción

1) Home carga y assets (CSS/imagenes) OK
2) Registro + confirmación email (si SMTP real)
3) Login / logout (Admin y Beneficiario)
4) Inscripción a taller (cupos)
5) Encuesta (si aplica) en `/Profile/Talleres`
6) Admin marca asistencia
7) Admin aprueba certificado
8) Beneficiario descarga PDF
9) Export CSV en Admin
10) `/Admin/EmailDiagnostics` envía correo real
11) Revisar logs IIS / Event Viewer (errores al iniciar suelen ser connection string/SMTP/permisos)

## PARTE 8 — Seguridad y buenas prácticas

- No subir secretos al repo:
  - `appsettings.Production.json` está ignorado por `.gitignore`
  - Preferir variables de entorno en IIS
- `Seed:CreateAdmin`:
  - En **Development** está en `true`
  - En **Producción** se recomienda `false`
  - Si se activa en prod, **Seed:AdminPassword debe ser explícito** (no se permite el default)
- `/Admin`
- `/Admin/Cursos`
- `/Admin/Talleres`
- `/Admin/Inscripciones`
- `/Admin/Encuestas`
- `/Admin/Certificados`
- `/Admin/Usuarios`
- `/Admin/EmailDiagnostics`

## Notas QA (interno)

Notas de verificación interna: `docs/qa-notes.md`.

## Checklist de pruebas (flujo completo)

> Estos pasos son para validar end-to-end. Úsalos en local; si luego quieres, puedes moverlos a `docs/qa-notes.md`.

### 0) Preparación
1. Aplicar migraciones (ver sección “Base de datos / migraciones”).
2. Ejecutar web: `cd src/SumandoValor.Web; dotnet run`.
3. Iniciar sesión como Admin:
   - `admin@sumandovalor.org` / `Admin123!`

### 1) Email / Confirmación (Development)
1. Registrar un usuario Beneficiario en `/Account/Register`.
2. Abrir `/Dev/Emails` (con Admin) y usar el enlace de confirmación.
3. Iniciar sesión con el usuario en `/Account/Login`.

### 2) Crear Curso + Taller (Admin)
1. Admin: crear curso en `/Admin/Cursos`.
2. Admin: crear taller en `/Admin/Talleres/Create`:
   - Modalidad Virtual/Híbrido → debe pedir Plataforma Digital.
   - Cupos pequeños (ej. 2) para probar sobrepaso.

### 3) Inscripciones + cupos (Beneficiario)
1. Beneficiario: entrar a `/Cursos/{id}` y abrir un taller.
2. Inscribirse desde `/Talleres/{id}`:
   - Debe mostrar “Inscripción realizada con éxito.”
3. Intentar inscribirse de nuevo → debe indicar “Ya estás inscrito”.
4. Crear usuarios adicionales (Usuario2/Usuario3) y llenar cupos.
5. Intentar Usuario4 cuando cupos=0 → “No hay cupos disponibles”.

### 4) Admin: asistencia / cancelación
1. Admin: `/Admin/Inscripciones`:
   - Marcar asistencia del usuario.
   - Cancelar inscripción (confirma) y verificar que libera cupo.

### 5) Encuesta (Beneficiario)
1. Admin: marcar el taller como **Finalizado** (editando taller).
2. Beneficiario: ir a `/Profile/Talleres`:
   - Si el taller **RequiereEncuesta** y está finalizado, aparece la encuesta embebida.
   - Enviar rating 1–5 (comentario opcional).
3. Admin: validar resultados en `/Admin/Encuestas` y exportar CSV.

### 6) Certificado (Admin → Beneficiario)
1. Admin: `/Admin/Certificados`:
   - Filtrar por taller.
   - Seleccionar filas elegibles (asistencia ok + encuesta ok si aplica + taller finalizado).
   - “Aprobar seleccionados” → genera PDF en `App_Data/Certificates` y habilita descarga.
2. Beneficiario: `/Profile/Talleres` → botón “Descargar certificado (PDF)”.
3. Descargar: `/Certificates/Download?id={certId}` debe permitir solo al dueño (o Admin).

### 7) Usuarios (Admin)
1. Admin: `/Admin/Usuarios`:
   - Buscar por nombre/email/cédula, filtrar por estado y rol.
   - Activar/Desactivar usuario (con confirm).
   - Hacer/Quitar Admin (no permite quitar al último admin).
