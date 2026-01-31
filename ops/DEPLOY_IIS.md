# Deploy a producción (IIS)

## Pre-requisitos

- .NET 8 Runtime o Hosting Bundle instalado en el servidor
- SQL Server accesible
- Permisos para crear carpetas y configurar el App Pool

## Pasos

### 1. Publicar

```powershell
dotnet publish src\SumandoValor.Web\SumandoValor.Web.csproj -c Release -o .\publish
```

### 2. Configurar web.config

Antes de copiar al servidor, editar `publish\web.config` y añadir en la sección `environmentVariables`:

- `ConnectionStrings__DefaultConnection`: cadena de conexión a SQL Server
- `Email:Smtp:Enabled`, `Email:Smtp:Host`, `Email:Smtp:Port`, etc.
- `Captcha:Provider` queda en `Math` (no requiere Cloudflare Turnstile)

### 3. Copiar al servidor

- Carpeta de la aplicación: según el sitio en IIS
- Crear `App_Data\Certificates` y `App_Data\DataProtection-Keys`
- Crear `wwwroot\uploads\carousel`, `wwwroot\uploads\site` (o la estructura que use la app)
- Dar permisos Modify al App Pool sobre `App_Data` y `logs`

### 4. Base de datos

Ejecutar migraciones antes del primer arranque, o dejar que la aplicación las aplique al iniciar (si la conexión tiene permisos DDL).

Si la columna `Ubicacion` falta en la tabla Talleres, ejecutar `ops/APLICAR_UBICACION_TALLER.sql`.

### 5. App Pool

- .NET CLR: No Managed Code
- Identity: según política de infraestructura (ApplicationPoolIdentity o cuenta de servicio)

### 6. Verificación

- Probar login y registro
- Probar envío de correos (Admin → EmailDiagnostics)
- Verificar generación de PDFs (certificados)
