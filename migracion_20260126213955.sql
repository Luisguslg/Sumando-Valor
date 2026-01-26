-- Script SQL para aplicar migración AddPaisSectorMunicipioToUser
-- Ejecutar en SQL Server Management Studio conectado a: VECCSAPP10,61057
-- Base de datos: SumandoValorDb

USE SumandoValorDb;
GO

-- Verificar si la migración ya fue aplicada
IF EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260126213955_AddPaisSectorMunicipioToUser')
BEGIN
    PRINT 'La migración 20260126213955_AddPaisSectorMunicipioToUser ya fue aplicada.';
    RETURN;
END
GO

PRINT 'Iniciando aplicación de migración...';
GO

BEGIN TRANSACTION;
GO

BEGIN TRY
    -- 1. Hacer Estado nullable y cambiar tipo
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Estado')
    BEGIN
        DECLARE @sqlEstado NVARCHAR(MAX);
        SET @sqlEstado = N'
        DECLARE @constraintName NVARCHAR(200);
        SELECT @constraintName = name FROM sys.default_constraints 
        WHERE parent_object_id = OBJECT_ID(''AspNetUsers'') 
        AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID(''AspNetUsers'') AND name = ''Estado'');
        IF @constraintName IS NOT NULL
            EXEC(''ALTER TABLE AspNetUsers DROP CONSTRAINT '' + @constraintName);
        ALTER TABLE AspNetUsers ALTER COLUMN Estado nvarchar(100) NULL;';
        EXEC sp_executesql @sqlEstado;
        PRINT 'Columna Estado actualizada.';
    END
    ELSE
    BEGIN
        PRINT 'ADVERTENCIA: Columna Estado no existe.';
    END
    GO

    -- 2. Hacer Ciudad nullable y cambiar tipo
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Ciudad')
    BEGIN
        DECLARE @sqlCiudad NVARCHAR(MAX);
        SET @sqlCiudad = N'
        DECLARE @constraintName NVARCHAR(200);
        SELECT @constraintName = name FROM sys.default_constraints 
        WHERE parent_object_id = OBJECT_ID(''AspNetUsers'') 
        AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID(''AspNetUsers'') AND name = ''Ciudad'');
        IF @constraintName IS NOT NULL
            EXEC(''ALTER TABLE AspNetUsers DROP CONSTRAINT '' + @constraintName);
        ALTER TABLE AspNetUsers ALTER COLUMN Ciudad nvarchar(100) NULL;';
        EXEC sp_executesql @sqlCiudad;
        PRINT 'Columna Ciudad actualizada.';
    END
    ELSE
    BEGIN
        PRINT 'ADVERTENCIA: Columna Ciudad no existe.';
    END
    GO

    -- 3. Agregar columna Municipio
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Municipio')
    BEGIN
        ALTER TABLE AspNetUsers ADD Municipio nvarchar(100) NULL;
        PRINT 'Columna Municipio agregada.';
    END
    ELSE
    BEGIN
        PRINT 'Columna Municipio ya existe.';
    END
    GO

    -- 4. Agregar columna Pais
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Pais')
    BEGIN
        ALTER TABLE AspNetUsers ADD Pais nvarchar(100) NOT NULL DEFAULT '';
        PRINT 'Columna Pais agregada.';
    END
    ELSE
    BEGIN
        PRINT 'Columna Pais ya existe.';
    END
    GO

    -- 5. Agregar columna Sector
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Sector')
    BEGIN
        ALTER TABLE AspNetUsers ADD Sector nvarchar(50) NOT NULL DEFAULT '';
        PRINT 'Columna Sector agregada.';
    END
    ELSE
    BEGIN
        PRINT 'Columna Sector ya existe.';
    END
    GO

    -- 6. Registrar migración en el historial
    IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260126213955_AddPaisSectorMunicipioToUser')
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES ('20260126213955_AddPaisSectorMunicipioToUser', '8.0.0');
        PRINT 'Migración registrada en el historial.';
    END
    ELSE
    BEGIN
        PRINT 'Migración ya está en el historial.';
    END
    GO

    COMMIT TRANSACTION;
    PRINT 'Migración aplicada exitosamente.';
    GO

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'ERROR al aplicar migración: ' + @ErrorMessage;
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO

-- Verificar resultado
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
AND COLUMN_NAME IN ('Pais', 'Sector', 'Municipio', 'Estado', 'Ciudad')
ORDER BY COLUMN_NAME;
GO
