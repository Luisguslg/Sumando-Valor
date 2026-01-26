# Aplicar Migración desde Consola en Servidor Remoto

## Pasos

1. **Conectarte al servidor remoto** (Remote Desktop)

2. **Abrir PowerShell o CMD** (como Administrador si es necesario)

3. **Ir al directorio del proyecto** (donde está el código fuente):
   ```powershell
   cd "ruta\completa\al\proyecto"
   ```

4. **Hacer pull de los últimos cambios**:
   ```powershell
   git pull origin main
   ```

5. **Aplicar la migración** apuntando a la BD de producción:
   ```powershell
   dotnet ef database update --project src\SumandoValor.Infrastructure --startup-project src\SumandoValor.Web --connection "Server=VECCSAPP10,61057;Database=SumandoValorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
   ```

   **O si el appsettings.json ya tiene la conexión correcta:**
   ```powershell
   dotnet ef database update --project src\SumandoValor.Infrastructure --startup-project src\SumandoValor.Web
   ```

6. **Verificar que se aplicó correctamente**:
   - Deberías ver: `Applying migration '20260126213955_AddPaisSectorMunicipioToUser'.`
   - Y al final: `Done.`

## Nota Importante

Asegúrate de que:
- Tienes acceso a la base de datos `VECCSAPP10,61057` desde el servidor
- La identidad de Windows con la que ejecutas el comando tiene permisos en SQL Server
- El proyecto tiene las herramientas de EF Core instaladas (`dotnet tool install --global dotnet-ef` si es necesario)
