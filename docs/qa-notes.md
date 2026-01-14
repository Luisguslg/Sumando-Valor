# QA Notes (interno)

Este archivo contiene notas de verificación interna para el equipo.

## Escenarios mínimos (Paso 2)

### Escenario P0 (inscripciones no se mezclan)
- Crear Taller A (100 cupos) e inscribir Usuario1.
- Crear Taller B (2 cupos).
- Verificar:
  - Usuario1 **NO** aparece inscrito en Taller B.
  - Taller A mantiene inscripción.
  - Taller B llega a 0 cupos al inscribir Usuario2/Usuario3.
  - Usuario4 no puede inscribirse (mensaje “No hay cupos disponibles.”).

### Admin Logout
- Desde cualquier ruta `/Admin/*` usar **Cerrar Sesión** y validar que vuelve a Home/Login correctamente.

