# Informe de Auditoría - Sumando Valor | Enero 2026

## Resumen Ejecutivo

Auditoría completa del proyecto Sumando Valor: código, base de datos, seguridad, documentación y cumplimiento del PDF de revisión. Se han identificado correcciones críticas y recomendaciones.

---

## 1. SEGURIDAD

### 1.1 Entorno Development vs Producción (CRÍTICO)

**Problema:** Con `ASPNETCORE_ENVIRONMENT=Development`, la aplicación usa `UseDeveloperExceptionPage()` y expone stack traces completos y datos sensibles a los usuarios.

**Implicación:** Si ejecutas en una máquina conectada a IIS con Development, cualquier error expone información interna (rutas, variables, conexiones).

**Recomendación para "producción-like en development":**
- Crear perfil `Staging` o `DevelopmentForProduction` con `ASPNETCORE_ENVIRONMENT=Staging`
- O agregar variable `UseProductionErrorHandling=true` en appsettings para usar manejo de errores tipo producción aunque el entorno sea Development

**Estado:** Program.cs actualmente:
- Development → `UseDeveloperExceptionPage()` (expone detalles)
- No-Development → `UseExceptionHandler("/Error")` + HSTS

### 1.2 Página de Error (Error.cshtml)

La página actual muestra texto genérico que incluye "Development Mode" y referencias a `ASPNETCORE_ENVIRONMENT`. Para un entorno tipo producción, conviene:
- Mensaje amigable al usuario: "Ha ocurrido un error. Por favor intente más tarde."
- Sin mencionar Development ni variables técnicas
- RequestId solo para soporte (opcional)

### 1.3 Headers de Seguridad

✅ Implementados correctamente:
- X-Content-Type-Options: nosniff
- Referrer-Policy: strict-origin-when-cross-origin
- X-Frame-Options: DENY
- Content-Security-Policy: frame-ancestors 'self'; base-uri 'self';

### 1.4 Autenticación y Autorización

✅ Identity con contraseñas robustas (14 caracteres, 3 únicos, mayúscula, minúscula, dígito, no alfanumérico)
✅ Lockout: 5 intentos fallidos → 15 minutos
✅ Confirmación de email requerida
✅ Roles: Admin, Moderador, Beneficiario
✅ Roles y Auditoría solo visibles para Admin en _AdminLayout

---

## 2. BUGS CRÍTICOS DETECTADOS

### 2.1 Activar usuario inactivo (CRÍTICO - CORREGIDO)

**Ubicación:** `Usuarios.cshtml.cs` → `OnPostToggleActiveAsync`

**Problema:** Cuando un usuario está **inactivo** (LockoutEnd establecido) y se presiona "Activar", el código nunca entra al bloque que ejecuta la actualización. Toda la lógica está dentro de `if (isActive)`; cuando `isActive` es false, no se hace ningún cambio.

**Efecto:** No se puede reactivar usuarios desactivados desde la UI.

**Corrección:** Agregar bloque `else` para manejar la activación (LockoutEnd = null).

### 2.2 Mensaje "Solo SuperAdmin puede gestionar el rol Admin"

**Ubicación:** `Roles.cshtml.cs` línea 207

**Observación:** El mensaje dice "SuperAdmin" pero la verificación es `User.IsInRole("Admin")`. En el sistema no existe rol "SuperAdmin" como tal; el SuperAdmin es un usuario con rol Admin (el inicial sembrado). Cualquier Admin puede gestionar el rol Admin. El mensaje puede confundir; sugerencia: "Solo un administrador puede gestionar el rol Admin."

---

## 3. TABLA DE AUDITORÍA

### 3.1 Campos disponibles vs mostrados

**AuditLog** tiene:
- Id, TableName, Action, RecordId, UserId, UserEmail, CreatedAt
- **OldValues** (JSON) - valores antes del cambio
- **NewValues** (JSON) - valores después del cambio

**En Auditoria.cshtml actualmente se muestra:**
- Fecha, Tabla, Acción, Registro ID, Usuario

**Lo que falta mostrar:**
- **OldValues** y **NewValues** (datos anteriores y nuevos en UPDATE/INSERT/DELETE)
- Estos campos contienen el detalle de qué cambió; sin ellos la auditoría es incompleta

**Recomendación:** Añadir columna expandible o modal para ver OldValues/NewValues cuando existan, con formato JSON legible o tabla de difusión.

### 3.2 Tablas auditadas por triggers

Triggers en: Cursos, Talleres, Inscripciones, Certificados, EncuestasSatisfaccion, MensajesContacto, CarouselItems, SiteImages, SurveyTemplates, SurveyQuestions, SurveyResponses, SurveyAnswers.

**No auditadas:**
- **AspNetUsers** (activar/desactivar usuario, cambios de perfil)
- AspNetRoles, AspNetUserRoles (cambios de roles)

### 3.3 Filtros de tabla en Auditoría

El desplegable incluye: Cursos, Talleres, Inscripciones, Certificados, Encuestas, Mensajes, Carrusel, Imágenes.  
No incluye: SurveyTemplates, SurveyQuestions, SurveyResponses, SurveyAnswers (aunque sí tienen triggers).

---

## 4. MODERADOR Y SUPER ADMIN

### 4.1 Permisos por rol

- **Admin:** Acceso total (Roles, Auditoría, Seguridad)
- **Moderador:** Cursos, Talleres, Usuarios, Inscripciones, Certificados, Encuestas, Carrusel, Imágenes, Email Diagnostics. **Sin** Roles, Auditoría, Seguridad.

### 4.2 Posibles causas de errores

1. **Activar usuario:** Bug en `OnPostToggleActiveAsync` impedía activar usuarios inactivos (corregido).
2. **Moderador desactivando Moderador:** Solo Admin puede desactivar moderadores; Moderador que intenta desactivar a otro Moderador recibe error correcto.
3. **Moderador intentando gestionar roles:** No ve el enlace a Roles (solo Admin). Si accede por URL directa, `[Authorize(Roles = "Admin")]` devuelve 403.
4. **Usuario desactivado con sesión activa:** Un usuario desactivado (LockoutEnd) que ya tiene sesión puede seguir haciendo peticiones hasta que la cookie expire o Identity revalide. Conviene middleware que verifique LockoutEnd en cada request para usuarios autenticados.

### 4.3 Recomendación: validar usuario activo en cada request

Agregar filtro o middleware que, para usuarios autenticados, compruebe `LockoutEnd` y, si está bloqueado, cierre sesión y redirija a Login con mensaje "Tu cuenta ha sido desactivada."

---

## 5. BASE DE DATOS

### 5.1 Robustez

✅ Migraciones con EF Core
✅ Foreign keys con ON DELETE RESTRICT donde corresponde
✅ Unique constraints (Email, Cédula, combinaciones usuario-taller)
✅ Índices en columnas usadas en filtros y joins
✅ UseSqlOutputClause(false) para tablas con triggers

### 5.2 SESSION_CONTEXT para auditoría

Los triggers usan `SESSION_CONTEXT('UserId')` y `SESSION_CONTEXT('UserEmail')` inyectados por `AuditContextMiddleware`. Si la operación no pasa por ese middleware (por ejemplo, jobs en background), UserId/UserEmail quedarán vacíos.

### 5.3 Documentación desactualizada

- `01-Diagrama-Clases.md`: Menciona `AdminAuditEvent`; en código actual es `AuditLog`.
- `03-Diagrama-Entidad-Relacion.md`: Describe `AdminAuditEvents`; la tabla actual es `AuditLogs`.
- `07-Validaciones-BD.md`: AdminAuditEvents; debe actualizarse a AuditLogs.

---

## 6. REVISIÓN PDF - ENE 2026

### Items del PDF vs implementación

| Item PDF | Estado |
|----------|--------|
| Logo Fundación KPMG Venezuela | Revisar en assets |
| Iniciar sesión: "Accede para inscribirte..." | Revisar texto en Login |
| Registro: "Completa tus datos para registrarte" | Revisar formulario |
| Sexo: mujer y hombre | Revisar opciones |
| Cédula V/E, 8 caracteres máx | Revisar validación |
| Teléfono solo números | Revisar validación |
| Campo Sector (tercer sector, privado, público, academia) | ✅ En dominio |
| País, Estado, Municipio (sin Ciudad si no Venezuela) | ✅ AddPaisSectorMunicipio |
| Omitir títulos en formularios admin | Revisar UI |
| Generar enlace en programas | Revisar funcionalidad |
| Ubicación en taller presencial | Revisar en Create/Edit Taller |
| Filtros en tabla talleres | Revisar |
| Nombre facilitador en tabla | Revisar |
| Inscripciones agrupadas por taller | Revisar diseño |
| Usuarios: remover ID, agregar ubicación, filtros, CSV | Parcial |
| Certificados agrupados por taller | Revisar |
| Encuestas agrupadas por taller | Revisar |

---

## 7. RASTROS DE IA

Búsqueda de patrones típicos (GPT, ChatGPT, Claude, Copilot, Cursor, "generated by AI"):
- No se encontraron referencias explícitas en código fuente.
- Las coincidencias en `cursor` pertenecen a CSS (cursor: pointer) y son normales.

---

## 8. CONFIGURACIÓN PARA IIS + PUSH

### 8.1 Variable de entorno en IIS

Para entorno tipo producción en la máquina con IIS:

```
ASPNETCORE_ENVIRONMENT=Staging
```

O, si quieres mantener Development pero sin exponer errores, añadir en `appsettings.Development.json`:

```json
"UseProductionErrorHandling": true
```

Y en Program.cs, evaluar esa opción para decidir si usar `UseExceptionHandler` en lugar de `UseDeveloperExceptionPage`.

### 8.2 launchSettings.json

Todos los perfiles usan `ASPNETCORE_ENVIRONMENT=Development`. Para pruebas locales tipo producción, conviene un perfil adicional con `Staging`.

---

## 9. CHECKLIST DE ACCIONES

### Corregido en esta auditoría
- [x] Bug activar usuario en `OnPostToggleActiveAsync` (Usuarios.cshtml.cs)
- [x] Página Error.cshtml amigable sin referencias a Development
- [x] Opción `UseProductionErrorHandling` en appsettings para producción-like en Development
- [x] Perfil Staging en launchSettings.json
- [x] Columna Detalle en Auditoría con OldValues/NewValues expandibles
- [x] Filtros para SurveyTemplates, SurveyQuestions, SurveyResponses, SurveyAnswers en Auditoría
- [x] Mensaje Roles: "Solo un administrador puede gestionar el rol Admin"

### Pendiente (recomendado)
- [ ] Middleware para cerrar sesión de usuarios desactivados al detectar LockoutEnd
- [x] Actualizar documentación (AdminAuditEvent → AuditLog en 01, 03, 04, 05, 07, 08)
- [ ] Revisar ítems pendientes del PDF de revisión (UI/UX)

---

**Fecha de auditoría:** 31 de enero de 2026  
**Versión aplicación:** .NET 8.0, SQL Server
