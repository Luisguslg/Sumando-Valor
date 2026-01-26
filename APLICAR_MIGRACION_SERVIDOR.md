# Instrucciones para Aplicar Migración en Servidor Remoto (IIS)

## Problema
La base de datos en el servidor remoto (`VECCSAPP10,61057`) no tiene las columnas nuevas: `Pais`, `Sector`, `Municipio`.

## Solución: Aplicar Migración Manualmente

### Opción 1: Desde el servidor remoto (recomendado)

1. **Conectarte al servidor remoto** (Remote Desktop)

2. **Ir a la carpeta donde está publicada la aplicación** (donde está el `web.config`)

3. **Abrir PowerShell o CMD como Administrador**

4. **Ejecutar el comando de migración:**
   ```powershell
   cd "ruta\a\la\aplicacion\publicada"
   
   dotnet ef database update --project "ruta\completa\src\SumandoValor.Infrastructure" --startup-project "ruta\completa\src\SumandoValor.Web" --connection "Server=VECCSAPP10,61057;Database=SumandoValorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
   ```

   **O si tienes acceso al código fuente en el servidor:**
   ```powershell
   cd "ruta\al\proyecto\raiz"
   dotnet ef database update --project src\SumandoValor.Infrastructure --startup-project src\SumandoValor.Web
   ```

### Opción 2: SQL Script Manual (si no tienes dotnet ef)

Si no puedes ejecutar `dotnet ef`, ejecuta este SQL directamente en SQL Server Management Studio conectado a `VECCSAPP10,61057`:

```sql
USE SumandoValorDb;
GO

-- Verificar si las columnas ya existen
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Pais')
BEGIN
    -- Hacer Estado y Ciudad nullable
    ALTER TABLE AspNetUsers ALTER COLUMN Estado nvarchar(100) NULL;
    ALTER TABLE AspNetUsers ALTER COLUMN Ciudad nvarchar(100) NULL;
    
    -- Agregar nuevas columnas
    ALTER TABLE AspNetUsers ADD Municipio nvarchar(100) NULL;
    ALTER TABLE AspNetUsers ADD Pais nvarchar(100) NOT NULL DEFAULT '';
    ALTER TABLE AspNetUsers ADD Sector nvarchar(50) NOT NULL DEFAULT '';
    
    -- Registrar la migración en el historial
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260126213955_AddPaisSectorMunicipioToUser', '8.0.0');
    
    PRINT 'Migración aplicada exitosamente';
END
ELSE
BEGIN
    PRINT 'Las columnas ya existen. Migración ya aplicada.';
END
GO
```

### Opción 3: Reiniciar la aplicación (si tiene auto-migración)

La aplicación tiene auto-migración habilitada. Si reinicias el App Pool en IIS, debería aplicar las migraciones automáticamente:

1. Abrir **IIS Manager**
2. Ir a **Application Pools**
3. Encontrar el App Pool de tu aplicación
4. Click derecho → **Recycle** (o **Stop** y luego **Start**)
5. Revisar los logs en `.\logs\stdout` para ver si hubo errores

### Verificar que funcionó

Después de aplicar la migración, verifica:

```sql
USE SumandoValorDb;
GO

-- Verificar columnas
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
AND COLUMN_NAME IN ('Pais', 'Sector', 'Municipio', 'Estado', 'Ciudad')
ORDER BY COLUMN_NAME;
GO
```

Deberías ver:
- `Pais`: nvarchar(100), NOT NULL
- `Sector`: nvarchar(50), NOT NULL  
- `Municipio`: nvarchar(100), NULL
- `Estado`: nvarchar(100), NULL
- `Ciudad`: nvarchar(100), NULL

## Nota Importante

La conexión en `web.config` apunta a:
- **Server**: `VECCSAPP10,61057`
- **Database**: `SumandoValorDb`
- **Autenticación**: Windows Authentication (Trusted_Connection=True)

Asegúrate de que la identidad del App Pool en IIS tenga permisos para modificar la base de datos.
