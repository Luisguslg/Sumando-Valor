# Sumando Valor - Plataforma Web

Plataforma web (Razor Pages + .NET 8) para la gestión de cursos, talleres e inscripciones de la Fundación KPMG Venezuela.

## Qué incluye (Paso 2 + Paso 3)

### Roles
- **Admin**: gestiona cursos/talleres/inscripciones, revisa encuestas, aprueba certificados, administra usuarios, diagnostica correos.
- **Beneficiario**: se registra, confirma email, se inscribe a talleres, responde encuestas (si aplica) y descarga certificados (si aplica).

### Flujo del usuario (Beneficiario)
- **Explorar**: Home → `/Cursos` → `/Cursos/{id}` → `/Talleres/{id}`
- **Inscribirse**: desde `/Talleres/{id}` (si está Abierto y hay cupos)
- **Mis Talleres**: `/Profile/Talleres`
  - **Encuesta**: aparece embebida cuando el taller está **Finalizado**, el usuario estuvo inscrito y el taller **RequiereEncuesta**
  - **Certificado**: se habilita cuando el taller **PermiteCertificado**, el Admin marcó **Asistencia**, la encuesta está completada (si aplica) y el Admin lo **aprueba**
- **Descarga**: `/Certificates/Download?id={certId}` (validación por usuario/rol)

### Flujo del Admin
- **Cursos**: `/Admin/Cursos`
- **Talleres**: `/Admin/Talleres`
- **Inscripciones**: `/Admin/Inscripciones` (marcar asistencia / cancelar)
- **Encuestas**: `/Admin/Encuestas` (filtros + paginación + resumen + export CSV)
- **Certificados**: `/Admin/Certificados` (aprobar / revocar + PDF + descarga)
- **Usuarios**: `/Admin/Usuarios` (paginación server-side, búsqueda, activar/desactivar, hacer/quitar admin con guardas)
- **Email Diagnostics**: `/Admin/EmailDiagnostics` (enviar correo de prueba)

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

## Correos (Development vs Producción/IIS)

### Development
- Se usa `DevelopmentEmailService` + `FileDevEmailStore` y los emails quedan persistidos en `App_Data/DevEmails`.
- **Bandeja Dev**: `/Dev/Emails` (solo **Admin** y solo **Development**) muestra enlaces de confirmación/reset.

### Producción (IIS)
- Se usa `SmtpEmailService` (SMTP real).
- Configura `Email:Smtp` por variables de entorno / `appsettings.Production.json` (no hardcodear secretos en el repo).

Ejemplo (placeholders):

```json
{
  "Email": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.tu-dominio.com",
      "Port": 587,
      "EnableSsl": true,
      "User": "usuario-smtp",
      "Password": "SECRETO",
      "FromAddress": "no-reply@tu-dominio.com",
      "FromName": "Sumando Valor"
    }
  }
}
```

### Contacto
- El formulario `/Contact` guarda el mensaje en BD y envía correo a `Email:ContactRecipient` (por defecto: `fundacionkpmg@kpmg.com`).

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
