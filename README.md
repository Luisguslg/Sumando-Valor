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

- **Administrador (Admin)**: gestiona cursos, talleres, inscripciones, encuestas, certificados, usuarios y diagnósticos de correo.
- **Beneficiario**: se registra, confirma email, se inscribe en talleres, responde encuestas (cuando aplica) y descarga certificados aprobados.

### Rutas clave (referenciales; pueden variar según el estado actual del proyecto)

**Públicas**

- `/`
- `/Cursos`
- `/Cursos/{id}`
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

## 12. Por confirmar / Pendiente de definición

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

