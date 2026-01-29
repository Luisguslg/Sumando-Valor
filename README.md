# Sumando Valor – Plataforma Web

Plataforma web desarrollada en **ASP.NET Core (.NET 8, Razor Pages)** para la **Fundación KPMG Venezuela**, orientada a la gestión de **cursos**, **talleres**, **inscripciones**, **encuestas** y **certificados**, con control de roles y despliegue en **IIS**.

## 1. Introducción

Sumando Valor centraliza y digitaliza el proceso de formación ofrecido por la Fundación, permitiendo:

- Publicación de cursos y talleres.
- Inscripción controlada de beneficiarios.
- Gestión administrativa de eventos.
- Recolección de encuestas de satisfacción.
- Emisión y descarga segura de certificados en PDF.
- Notificaciones por correo electrónico.

### Roles del sistema

- **Admin**: rol de máximo privilegio. Puede gestionar cursos, talleres, inscripciones, encuestas, certificados, usuarios, roles y permisos, y ver auditoría completa del sistema. Puede asignar/quitar el rol Moderador a otros usuarios.
- **Moderador**: gestiona cursos, talleres, inscripciones, encuestas, certificados, usuarios y diagnósticos de correo. Puede inscribir usuarios directamente en talleres y gestionar acceso a cursos internos mediante claves o enlaces únicos. No puede gestionar roles ni ver auditoría completa.
- **Beneficiario**: se registra, confirma email, se inscribe en talleres, responde encuestas (cuando aplica) y descarga certificados aprobados.

### Rutas clave (referenciales; pueden variar según el estado actual del proyecto)

**Públicas**

- `/`
- `/Cursos` (solo muestra cursos públicos)
- `/Cursos/{id}` (requiere clave si el curso es interno)
- `/Cursos/Access/{id}` (página para ingresar clave de acceso)
- `/Talleres/{id}`
- `/Contact`

**Cuenta / Perfil**

- `/Account/Login`
- `/Account/Register`
- `/Profile`
- `/Profile/Talleres`

**Administración**

- `/Admin/*`
- `/Admin/Cursos`
- `/Admin/Talleres`
- `/Admin/Inscripciones`
- `/Admin/EmailDiagnostics`
- `/Admin/Auditoria` (solo Admin)
- `/Admin/Seguridad` (solo Admin: políticas de contraseña, bloqueo, CAPTCHA, conexión)
- (Encuestas, Certificados y Usuarios según estado actual del proyecto)

## 2. Arquitectura

### Diagrama textual (Producción)

Usuario (Browser)
   │
   ▼
IIS (ASP.NET Core Module - ANCM)
   │
   ▼
Kestrel (.NET 8 / Razor Pages)
   ├─ ASP.NET Identity (Roles)
   ├─ EF Core (Migraciones)
   ├─ SMTP (correo real)
   ├─ QuestPDF (PDFs)
   └─ File System (App_Data)
        ├─ Certificates
        └─ DataProtection-Keys

### Componentes principales

- **Backend/Frontend**: Razor Pages (.NET 8).
- **Persistencia**: SQL Server.
- **Autenticación/Autorización**: ASP.NET Identity (Roles).
- **Correo**: servicio SMTP configurable por entorno.
- **PDFs**: QuestPDF, almacenamiento fuera de `wwwroot`.

## 3. Tecnologías y dependencias

- .NET 8 (ASP.NET Core)
- Razor Pages
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- QuestPDF
- IIS (ASP.NET Core Module V2)
- SMTP (relay corporativo o autenticado)

## 4. Requisitos del servidor (Producción)

- Windows Server (2019 o superior).
- IIS instalado con:
  - Static Content
  - ASP.NET Core Module (incluido en el Hosting Bundle)
- .NET 8 Hosting Bundle instalado.
- Acceso a SQL Server.
- Acceso de red al servidor SMTP.
- Permisos NTFS sobre carpetas de la aplicación (ver secciones 8 y 10).

## 5. Configuración por entorno

### Development

- Base de datos de desarrollo (según configuración local).
- Correos simulados (`DevelopmentEmailService`).
- Emails guardados en `App_Data/DevEmails`.
- Visualización (solo dev): `/Dev/Emails`.

### Production (IIS)

- SQL Server remoto.
- SMTP real (`SmtpEmailService`).
- PDFs persistentes en disco.
- DataProtection keys persistidas en disco.

## 6. Base de datos

### Migraciones

Las migraciones se encuentran en:

`src/SumandoValor.Infrastructure/Migrations`

### Opción A – Migración manual (recomendada)

Ejecutar desde la raíz del repo:

```powershell
dotnet ef database update `
  --project src\SumandoValor.Infrastructure `
  --startup-project src\SumandoValor.Web
```

### Opción B – Migración automática al iniciar

La aplicación ejecuta migraciones automáticamente en el arranque (cuando la conexión es válida).

### Autenticación SQL (referencia)

- **Windows Authentication**: requiere que la identidad del App Pool tenga permisos en SQL Server.
- **SQL Authentication**: suele ser más simple de operar en producción.

Permisos mínimos durante migraciones: capacidad de ejecutar DDL (en muchos entornos se usa `db_owner`).

## 7. Correos electrónicos

### Flujo

- Abstracción: `IEmailService`
- Implementaciones:
  - `DevelopmentEmailService`
  - `SmtpEmailService`

### Diagnóstico

- Ruta Admin: `/Admin/EmailDiagnostics`

### Variables de entorno (IIS) – Producción (sin secretos)

```text
Email__Smtp__Enabled=true
Email__Smtp__Host=goemairs.go.kworld.kpmg.com
Email__Smtp__Port=25
Email__Smtp__EnableSsl=false
Email__Smtp__UseDefaultCredentials=true
Email__Smtp__FromAddress=sumandovalor@kpmg.com
Email__Smtp__FromName=Sumando Valor
```

### Troubleshooting típico

- Host incorrecto (error común).
- Firewall bloqueando puertos (25/587).
- Relay SMTP sin permiso por IP.
- Timeout SMTP (infraestructura/red).

## 8. Certificados / PDFs

- Generación: QuestPDF.
- Almacenamiento: `App_Data/Certificates` (fuera de `wwwroot`).
- Naming (referencial):

```text
cert_{certId}_{tallerId}_{userId}_{guid}.pdf
```

- Descarga protegida (referencial): `/Certificates/Download?id={certId}`

### Seguridad

- Solo el usuario dueño o un Admin puede descargar.
- PDFs anteriores se eliminan al regenerar (según implementación actual).

### Permisos NTFS

Dar **Modify** a la identidad del App Pool sobre `App_Data/Certificates`.

## 9. Seguridad

- Roles y autorización por página.
- Archivos sensibles fuera de `wwwroot`.
- DataProtection keys persistidas en:
  - `App_Data/DataProtection-Keys`
- No se almacenan secretos en el repositorio.

### 9.1 CAPTCHA

**Por defecto** la aplicación usa un **CAPTCHA matemático**: una pregunta tipo *"¿Cuánto es 5 + 3?"* que el usuario responde. No requiere cuentas ni claves; funciona sin configurar nada.

- **Valor por defecto**: `Captcha:Provider` = **"Math"** en `appsettings.json`.
- **Desactivar**: Pon `"Provider": "None"` si no quieres verificación.
- **Alternativa (Cloudflare Turnstile)**: Si prefieres un CAPTCHA externo, pon `"Provider": "Turnstile"` y configura Site Key y Secret Key en `Captcha:CloudflareTurnstile`. Ver `diseño/11-CAPTCHA-Turnstile.md`.

**Dónde se usa**: Login, Registro y formulario de contacto. El estado se revisa en **Admin → Seguridad** (solo rol Admin).

## 10. Deploy en IIS (paso a paso)

### 10.1 Publicar desde desarrollo

Desde la raíz del repo:

```powershell
dotnet publish src\SumandoValor.Web\SumandoValor.Web.csproj -c Release -o .\publish
```

### 10.2 Copiar al servidor

Ejemplo:

- Carpeta destino del sitio: `C:\inetpub\sumandovalor\app\`
- Crear:
  - `C:\inetpub\sumandovalor\App_Data\Certificates\`
  - `C:\inetpub\sumandovalor\App_Data\DataProtection-Keys\`

### 10.3 IIS

**App Pool**

- .NET CLR: **No Managed Code**
- Identity: `ApplicationPoolIdentity` (o identidad definida por infraestructura)

**Site**

- Physical Path: carpeta publicada
- Binding: según infraestructura (host/puerto/certificado)

### 10.4 Variables de entorno / web.config

Definir variables de entorno en IIS (recomendado) o en `web.config`.  
**Nunca** versionar contraseñas en Git.

### 10.5 Logs

`logs\stdout` (si se habilita en `web.config`) requiere permisos de escritura NTFS.

## 11. Checklist post-deploy

- La app inicia sin errores.
- Migraciones aplicadas correctamente.
- Login / Registro funcional.
- Envío de correos probado (ideal: `/Admin/EmailDiagnostics`).
- PDFs se generan y descargan.
- Roles Admin/Beneficiario correctos.
- Permisos NTFS validados.

## 12. Funcionalidades recientes

### Cursos internos (no públicos)

- **Clave de acceso**: Se genera automáticamente al crear/editar un curso como "no público"
- **Enlaces únicos**: El admin puede generar y enviar enlaces únicos por email con expiración configurable
- **Acceso controlado**: Los cursos internos no aparecen en listados públicos y requieren clave o token para acceder
- **Gestión**: Desde `/Admin/Cursos` se puede ver la clave y enviar enlaces

### Inscripción por administrador

- **Funcionalidad**: El admin puede inscribir usuarios directamente en talleres desde `/Admin/Inscripciones`
- **Validaciones**: Verifica cupos disponibles, estado del taller, y previene inscripciones duplicadas
- **Transacciones**: Usa transacciones SERIALIZABLE para evitar race conditions

### Actualizaciones de UI

- **Capitalización**: "Programa formativo" → "Programa Formativo" en toda la aplicación
- **Tarjetas**: "Programa Formativo" aparece una sola vez, centrado
- **Opciones de registro**: Actualizadas según especificación (Redes sociales, Amigos o familiares, etc.)

## 13. Por confirmar / Pendiente de definición

- ¿El CRUD completo de Encuestas ya está habilitado en Admin o solo parcialmente?
- ¿El flujo exacto de aprobación de certificados requiere:
  - aprobación manual obligatoria siempre?
  - o automática tras encuesta aprobada?
- ¿El CRUD de Usuarios (Admin) incluye:
  - paginación?
  - activación/desactivación?
  - asignación de rol Admin desde UI?
- ¿Existe política de retención/eliminación de certificados antiguos?
- ¿El SMTP corporativo permite envío externo o solo interno?

