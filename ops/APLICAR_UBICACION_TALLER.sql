-- Script para agregar la columna Ubicacion a Talleres
-- Ejecutar en SQL Server Management Studio o similar si la app falla con "Invalid column name 'Ubicacion'"
-- La app también intenta aplicar esto al arrancar (DbInitializer).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Talleres') AND name = 'Ubicacion'
)
BEGIN
    ALTER TABLE [Talleres] ADD [Ubicacion] nvarchar(300) NULL;
END
GO
