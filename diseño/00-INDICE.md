# Documentación Técnica - Sumando Valor

## Índice de Documentación

Esta carpeta contiene la documentación técnica completa de la aplicación **Sumando Valor** para revisión de seguridad.

### Documentos Disponibles

1. **[01-Diagrama-Clases.md](01-Diagrama-Clases.md)** - Diagrama de clases del modelo de dominio
2. **[02-Diagrama-Componentes.md](02-Diagrama-Componentes.md)** - Arquitectura de componentes y capas
3. **[03-Diagrama-Entidad-Relacion.md](03-Diagrama-Entidad-Relacion.md)** - Modelo de datos y relaciones
4. **[04-Estructura-Codigo.md](04-Estructura-Codigo.md)** - Estructura de carpetas y organización del código
5. **[05-APIs-Endpoints.md](05-APIs-Endpoints.md)** - Endpoints, parámetros y respuestas
6. **[06-Integraciones-Externas.md](06-Integraciones-Externas.md)** - Sistemas externos integrados
7. **[07-Validaciones-BD.md](07-Validaciones-BD.md)** - Reglas de validación a nivel de base de datos
8. **[08-Evaluacion-Seguridad.md](08-Evaluacion-Seguridad.md)** - Análisis de seguridad y recomendaciones
9. **[09-Manual-Usuario.md](09-Manual-Usuario.md)** - Manual completo de usuario con todos los flujos
10. **[10-Manual-Administrador.md](10-Manual-Administrador.md)** - Manual completo de administrador con todos los flujos
11. **[11-CAPTCHA-Turnstile.md](11-CAPTCHA-Turnstile.md)** - CAPTCHA matemático (por defecto) y opción Cloudflare Turnstile

### Operaciones

- **ops/DEPLOY_IIS.md** - Pasos para deploy en IIS
- **ops/COMANDOS_BD_LOCAL.txt** - Comandos de migraciones
- **ops/APLICAR_UBICACION_TALLER.sql** - Script por si la columna Ubicacion no se crea

### Notas

- Los diagramas están en formato **Mermaid** y se pueden visualizar en GitHub, GitLab o editores que soporten Mermaid
- Para visualizar diagramas Mermaid localmente, usar herramientas como:
  - [Mermaid Live Editor](https://mermaid.live/)
  - Extensiones de VS Code: "Markdown Preview Mermaid Support"
  - GitHub/GitLab renderiza automáticamente los diagramas

### Versión

- **Última actualización**: Enero 2026
- **Versión de la aplicación**: .NET 8.0
- **Base de datos**: SQL Server
