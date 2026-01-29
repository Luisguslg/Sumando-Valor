# Integraciones con Sistemas Externos - Sumando Valor

## Resumen

Este documento describe todas las integraciones con sistemas externos, protocolos utilizados, autenticación y flujos de datos.

## 1. SQL Server (Base de Datos)

### Descripción
Base de datos relacional principal de la aplicación.

### Tipo de Integración
- **Protocolo**: TDS (Tabular Data Stream) sobre TCP/IP
- **Driver**: Microsoft.Data.SqlClient
- **ORM**: Entity Framework Core 8.0

### Configuración
- **Connection String**: Configurado en `appsettings.json` o variables de entorno
- **Autenticación**: 
  - Windows Authentication (Integrated Security)
  - SQL Authentication (usuario/contraseña)
- **Timeout**: Por defecto de EF Core (30 segundos)

### Flujos de Datos
- **Lectura**: Queries LINQ → EF Core → SQL Server
- **Escritura**: Cambios en DbContext → EF Core → SQL Server (transacciones)
- **Migraciones**: Scripts SQL generados por EF Core Migrations

### Seguridad
- Connection string encriptado en producción (User Secrets o Azure Key Vault)
- Parámetros parametrizados (previene SQL Injection)
- Transacciones para operaciones críticas

### Ubicación en Código
- `src/SumandoValor.Infrastructure/Data/AppDbContext.cs`
- `src/SumandoValor.Infrastructure/Migrations/`

## 2. SMTP Server (Correo Electrónico)

### Descripción
Servidor SMTP para envío de correos electrónicos (confirmación de email, recuperación de contraseña, notificaciones).

### Tipo de Integración
- **Protocolo**: SMTP (Simple Mail Transfer Protocol)
- **Puerto**: 25, 587 (TLS), 465 (SSL)
- **Librería**: `System.Net.Mail.SmtpClient` (.NET)

### Configuración
```json
{
  "Email": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.example.com",
      "Port": 587,
      "EnableSsl": true,
      "User": "usuario@example.com",
      "Password": "contraseña",
      "FromAddress": "noreply@sumandovalor.org"
    },
    "SendCertificateNotification": true
  }
}
```

### Implementaciones

#### DevelopmentEmailService
- **Entorno**: Development
- **Comportamiento**: Guarda emails en disco (`App_Data/DevEmails/`)
- **Propósito**: Desarrollo sin necesidad de SMTP real
- **Visualización**: `/Dev/Emails` (solo desarrollo)

#### SmtpEmailService
- **Entorno**: Production
- **Comportamiento**: Envía emails reales por SMTP
- **Autenticación**: Usuario/contraseña o anónimo según configuración

### Tipos de Emails Enviados
1. **Confirmación de Email**: Al registrarse
2. **Recuperación de Contraseña**: Al solicitar reset
3. **Notificación de Certificado**: Cuando se aprueba un certificado

### Seguridad
- Credenciales en configuración (no en código)
- TLS/SSL para conexión segura
- Validación de destinatarios (solo emails válidos)

### Ubicación en Código
- `src/SumandoValor.Infrastructure/Services/IEmailService.cs`
- `src/SumandoValor.Infrastructure/Services/DevelopmentEmailService.cs`
- `src/SumandoValor.Infrastructure/Services/SmtpEmailService.cs`
- `src/SumandoValor.Infrastructure/Services/SmtpEmailOptions.cs`

## 3. Cloudflare Turnstile (CAPTCHA)

### Opción por defecto: CAPTCHA matemático (Math)

**No requiere configuración.** Por defecto la aplicación usa un CAPTCHA matemático: una pregunta tipo *"¿Cuánto es 5 + 3?"* que el usuario responde. Se valida en el servidor con la sesión; no hay servicios externos ni claves. Se usa en **Login**, **Registro** y **formulario de Contacto**.

### Opción alternativa: Cloudflare Turnstile

**Turnstile** es un servicio de **Cloudflare** que sustituye los CAPTCHA clásicos. Suele mostrar una verificación casi invisible o muy rápida para usuarios reales y bloquea bots. Es gratuito. Para usarlo hay que poner `Provider: "Turnstile"` y configurar Site Key y Secret Key en Cloudflare. Si el CAPTCHA está desactivado (`Provider: "None"`), no se muestra ninguna verificación.

### Cómo activar / cambiar CAPTCHA

- **Math (por defecto)**: No hace falta configurar nada; ya está activo.
- **Turnstile (Cloudflare)**: 1) En [dash.cloudflare.com](https://dash.cloudflare.com) → Turnstile → crear widget y obtener Site Key y Secret Key. 2) En `appsettings.json`: `"Provider": "Turnstile"` y rellenar `Captcha:CloudflareTurnstile:SiteKey` y `SecretKey`. 3) Ver [11-CAPTCHA-Turnstile.md](11-CAPTCHA-Turnstile.md) para claves de prueba en desarrollo.
- **Desactivar**: Poner `"Provider": "None"` en la sección `Captcha`.

Guía detallada: **[11-CAPTCHA-Turnstile.md](11-CAPTCHA-Turnstile.md)**.

### Tipo de Integración
- **Protocolo**: HTTPS REST API
- **Endpoint**: `https://challenges.cloudflare.com/turnstile/v0/siteverify`
- **Método**: POST (Form URL Encoded)

### Configuración
```json
{
  "Captcha": {
    "Provider": "Turnstile",
    "CloudflareTurnstile": {
      "SiteKey": "clave-publica",
      "SecretKey": "clave-privada"
    }
  }
}
```

### Flujo de Validación
1. Cliente carga página → Frontend muestra widget Turnstile
2. Usuario completa CAPTCHA → Frontend obtiene token
3. Frontend envía token en formulario POST
4. Backend valida token con Cloudflare API
5. Si válido → Procesa formulario, si no → Rechaza

### Request a Cloudflare
```
POST https://challenges.cloudflare.com/turnstile/v0/siteverify
Content-Type: application/x-www-form-urlencoded

secret=SECRET_KEY&response=TOKEN&remoteip=CLIENT_IP
```

### Response de Cloudflare
```json
{
  "success": true,
  "challenge_ts": "2026-01-25T12:00:00.000Z",
  "hostname": "sumandovalor.org"
}
```

### Implementaciones

#### MockCaptchaValidator
- **Cuándo se usa**: Cuando `Captcha:Provider = "None"`
- **Comportamiento**: Siempre retorna `true` (no valida realmente)
- **Propósito**: No mostrar widget ni depender de Cloudflare

#### CloudflareTurnstileCaptchaValidator
- **Cuándo se usa**: Cuando `Captcha:Provider = "Turnstile"` (en cualquier entorno)
- **Comportamiento**: Valida el token con la API de Cloudflare (`siteverify`)
- **Timeout**: Por defecto de HttpClient

### Seguridad
- Secret key nunca expuesta al cliente (solo SiteKey)
- Validación de IP remota (opcional)
- Timeout en requests HTTP

### Ubicación en Código
- `src/SumandoValor.Infrastructure/Services/ICaptchaValidator.cs`
- `src/SumandoValor.Infrastructure/Services/MockCaptchaValidator.cs`
- `src/SumandoValor.Infrastructure/Services/CloudflareTurnstileCaptchaValidator.cs`

## 4. Sistema de Archivos (File System)

### Descripción
Almacenamiento de archivos subidos por usuarios y generados por la aplicación.

### Tipo de Integración
- **Acceso**: I/O de archivos del sistema operativo
- **Ubicación**: `wwwroot/uploads/` y `App_Data/`

### Estructura de Carpetas

#### wwwroot/uploads/
- **carousel/**: Imágenes del carrusel del homepage
- **site/**: Imágenes del sitio (AboutMain, WorkshopCard, HomePillars)

#### App_Data/
- **Certificates/**: PDFs de certificados generados
- **DevEmails/**: Emails guardados en desarrollo
- **DataProtection-Keys/**: Claves de encriptación de Identity

### Operaciones
- **Lectura**: Servir archivos estáticos (carousel, imágenes)
- **Escritura**: Guardar uploads de usuarios
- **Eliminación**: Limpieza de archivos huérfanos (solo desarrollo)

### Seguridad
- Validación de tipo de archivo (magic bytes)
- Límite de tamaño (4MB para imágenes)
- Nombres de archivo generados (GUID) para prevenir path traversal
- Permisos NTFS en producción (solo App Pool puede escribir)

### Ubicación en Código
- `src/SumandoValor.Web/Pages/Admin/Carrusel.cshtml.cs`
- `src/SumandoValor.Web/Pages/Admin/Imagenes.cshtml.cs`
- `src/SumandoValor.Web/Services/UploadCleanupService.cs`
- `src/SumandoValor.Web/Services/Certificates/CertificatePdfGenerator.cs`

## 5. QuestPDF (Generación de PDFs)

### Descripción
Librería para generación de certificados en formato PDF.

### Tipo de Integración
- **Librería**: NuGet package `QuestPDF` (v2202.8.2)
- **Tipo**: Biblioteca local (no servicio externo)

### Uso
- Generación de certificados de participación en talleres
- Template base: `wwwroot/images/certificates/certificado-template.png`
- Overlay de datos: Nombre, taller, duración, fecha

### Flujo
1. Admin aprueba certificado
2. `CertificatePdfGenerator.Generate()` crea PDF
3. PDF guardado en `App_Data/Certificates/`
4. Ruta relativa guardada en BD (`Certificado.UrlPdf`)

### Ubicación en Código
- `src/SumandoValor.Web/Services/Certificates/CertificatePdfGenerator.cs`
- `src/SumandoValor.Web/Pages/Admin/Certificados.cshtml.cs`

## 6. ASP.NET Core Identity (Autenticación)

### Descripción
Framework de autenticación y autorización integrado en ASP.NET Core.

### Tipo de Integración
- **Framework**: Parte de ASP.NET Core
- **Almacenamiento**: Tablas en SQL Server (AspNetUsers, AspNetRoles, etc.)

### Funcionalidades
- Autenticación por email/contraseña
- Confirmación de email requerida
- Recuperación de contraseña
- Lockout después de intentos fallidos
- Roles: `Admin`, `SuperAdmin`, `Beneficiario`
- Cookies de autenticación (HttpOnly, Secure)

### Configuración
- Password requirements: Min 8 caracteres, requiere mayúscula, minúscula, dígito
- Lockout: 5 intentos, 10 minutos
- Cookie: HttpOnly, Secure en producción, SameSite=Lax

### Ubicación en Código
- `src/SumandoValor.Infrastructure/Data/ApplicationUser.cs`
- `src/SumandoValor.Web/Program.cs` (configuración)

## Resumen de Dependencias Externas

| Sistema | Tipo | Protocolo | Autenticación | Entorno |
|---------|------|-----------|---------------|---------|
| SQL Server | Base de datos | TDS/TCP | Windows/SQL Auth | Todos |
| SMTP Server | Email | SMTP | Usuario/Password | Production |
| Cloudflare Turnstile | CAPTCHA | HTTPS REST | Secret Key | Production |
| File System | Almacenamiento | I/O local | Permisos NTFS | Todos |
| QuestPDF | Librería | Local | N/A | Todos |
| ASP.NET Identity | Framework | Local | Cookies | Todos |

## Consideraciones de Seguridad

1. **Credenciales**: Nunca en código, siempre en configuración
2. **HTTPS**: Todas las comunicaciones externas por HTTPS
3. **Timeouts**: Configurados en HttpClient y SmtpClient
4. **Validación**: Validación de entrada en todos los endpoints
5. **Logging**: Logs de operaciones críticas (sin datos sensibles)
6. **Error Handling**: Manejo de errores sin exponer información sensible
