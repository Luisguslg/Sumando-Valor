# Manual de Administrador - Sumando Valor

## Índice

1. [Introducción](#introducción)
2. [Acceso al Panel de Administración](#acceso-al-panel-de-administración)
3. [Gestión de Programas Formativos](#gestión-de-programas-formativos)
4. [Gestión de Talleres](#gestión-de-talleres)
5. [Gestión de Inscripciones](#gestión-de-inscripciones)
6. [Gestión de Usuarios](#gestión-de-usuarios)
7. [Gestión de Roles y Permisos](#gestión-de-roles-y-permisos)
8. [Auditoría del Sistema](#auditoría-del-sistema)
9. [Programas Internos y Control de Acceso](#programas-internos-y-control-de-acceso)
10. [Certificados](#certificados)
11. [Encuestas](#encuestas)
12. [Estadísticas y Reportes](#estadísticas-y-reportes)
13. [Configuración del Sistema](#configuración-del-sistema)

---

## Introducción

Este manual está dirigido a los administradores de la plataforma **Sumando Valor**. Cubre todas las funcionalidades de administración disponibles en el sistema.

### Roles de Administración

- **Admin**: Acceso completo al panel de administración, incluyendo gestión de roles y permisos, y auditoría completa del sistema
- **Moderador**: Acceso al panel de administración para gestión de contenido (cursos, talleres, inscripciones, certificados, usuarios). No puede gestionar roles ni ver auditoría completa

---

## Acceso al Panel de Administración

### Requisitos

- Tener el rol de **Moderador** o **Admin**
- Estar autenticado en el sistema

### Acceso

1. Inicia sesión con tu cuenta de administrador
2. Haz clic en **"Administración"** en el menú principal
3. Serás redirigido al panel de administración

### Navegación del Panel

El panel de administración incluye:
- **Dashboard**: Estadísticas y resumen general
- **Programas Formativos**: Gestión de cursos/programas
- **Talleres**: Gestión de talleres
- **Inscripciones**: Gestión de inscripciones de usuarios
- **Certificados**: Generación y gestión de certificados
- **Usuarios**: Gestión de usuarios y roles

---

## Gestión de Programas Formativos

### Crear un Nuevo Programa

1. Ve a **"Programas Formativos"** en el panel de administración
2. Haz clic en **"Nuevo Programa"**
3. Completa el formulario:
   - **Título**: Nombre del programa
   - **Descripción**: Descripción completa
   - **Público Objetivo**: A quién está dirigido
   - **Estado**: Activo/Inactivo
   - **Es Público**: Si está disponible para todos o es interno
   - **Orden**: Orden de visualización
4. Haz clic en **"Guardar"**

### Editar un Programa

1. En la lista de programas, haz clic en **"Editar"** en el programa deseado
2. Modifica los campos necesarios
3. Haz clic en **"Guardar Cambios"**

### Activar/Desactivar un Programa

1. En la lista de programas, usa el botón **"Activar"** o **"Desactivar"**
2. Los programas inactivos no aparecen en la lista pública

### Eliminar un Programa

**Nota**: Solo se puede eliminar si no tiene talleres asociados.

1. Haz clic en **"Eliminar"** en el programa
2. Confirma la eliminación

---

## Gestión de Talleres

### Crear un Nuevo Taller

1. Ve a **"Talleres"** en el panel de administración
2. Haz clic en **"Nuevo Taller"**
3. Completa el formulario:
   - **Programa Formativo**: Selecciona el programa al que pertenece
   - **Título**: Nombre del taller
   - **Descripción**: Descripción completa
   - **Fecha de Inicio**: Fecha y hora de inicio
   - **Fecha de Fin**: Fecha y hora de fin (opcional)
   - **Hora de Inicio**: Hora de inicio
   - **Modalidad**: Presencial, Virtual o Híbrido
   - **Plataforma Digital**: Si es virtual, URL de la plataforma
   - **Cupos Máximos**: Número máximo de participantes
   - **Estatus**: Abierto, Cerrado, Cancelado, Finalizado
   - **Permite Certificado**: Si los participantes pueden obtener certificado
   - **Requiere Encuesta**: Si se requiere encuesta de satisfacción
   - **Facilitador**: Nombre del facilitador
4. Haz clic en **"Guardar"**

### Editar un Taller

1. En la lista de talleres, haz clic en **"Editar"**
2. Modifica los campos necesarios
3. **Importante**: No puedes reducir los cupos por debajo de las inscripciones activas
4. Haz clic en **"Guardar Cambios"**

### Cambiar Estatus de un Taller

- **Abierto**: Acepta nuevas inscripciones
- **Cerrado**: No acepta nuevas inscripciones
- **Cancelado**: El taller fue cancelado
- **Finalizado**: El taller ya finalizó

---

## Gestión de Inscripciones

### Ver Inscripciones

1. Ve a **"Inscripciones"** en el panel de administración
2. Verás una lista de todas las inscripciones con:
   - Usuario
   - Taller
   - Programa Formativo
   - Fecha de inscripción
   - Estado (Activa/Cancelada)
   - Asistencia

### Buscar Inscripciones

Puedes buscar por:
- Nombre del usuario
- Email del usuario
- Título del taller
- Título del programa formativo

### Agregar Usuario a un Taller

1. En la página de **"Inscripciones"**, haz clic en **"Agregar Usuario"**
2. Busca al usuario por nombre o email
3. Selecciona el taller
4. Haz clic en **"Inscribir"**

### Marcar/Quitar Asistencia

1. En la lista de inscripciones, haz clic en **"Marcar asistencia"** o **"Quitar asistencia"**
2. La asistencia es necesaria para generar certificados

### Cancelar una Inscripción

1. En la lista de inscripciones, haz clic en **"Cancelar"**
2. Confirma la cancelación
3. El cupo quedará disponible para otros usuarios

### Exportar Inscripciones

1. Haz clic en **"Exportar CSV"**
2. Se descargará un archivo CSV con todas las inscripciones

---

## Gestión de Usuarios

### Ver Usuarios

1. Ve a **"Usuarios"** en el panel de administración
2. Verás una lista de todos los usuarios registrados

### Buscar Usuarios

Puedes buscar por:
- Nombre
- Email
- Cédula

### Asignar Roles

1. Haz clic en **"Editar"** en el usuario
2. Asigna o quita roles:
   - **Beneficiario**: Puede inscribirse en talleres
   - **Administrador**: Acceso al panel de administración
3. Haz clic en **"Guardar"**

---

## Gestión de Roles y Permisos

**Nota**: Esta sección solo está disponible para usuarios con rol **Admin**.

### Ver Roles del Sistema

1. Ve a **"Roles y Permisos"** en el panel de administración
2. Verás una lista de todos los roles del sistema con:
   - Nombre del rol
   - Número de usuarios asignados
   - Acciones disponibles

### Crear un Nuevo Rol

1. En la sección **"Crear Nuevo Rol"**, ingresa el nombre del rol
2. Haz clic en **"Crear Rol"**
3. El nuevo rol aparecerá en la lista

**Importante**: Los roles del sistema (Admin, Moderador, Beneficiario) no se pueden eliminar.

### Eliminar un Rol

1. En la lista de roles, haz clic en **"Eliminar"** en el rol que deseas eliminar
2. Confirma la eliminación
3. **Nota**: No puedes eliminar un rol si tiene usuarios asignados

### Asignar Roles a Usuarios

1. En la sección **"Asignar Roles a Usuarios"**, encontrarás una tabla con todos los usuarios
2. Para cada usuario, verás checkboxes con todos los roles disponibles
3. Marca los roles que deseas asignar al usuario
4. Haz clic en **"Guardar Cambios"**

### Reglas de Asignación

- **Solo Admin puede asignar/quitar el rol Admin**: Si intentas asignar o quitar el rol Admin y no eres Admin, verás un error
- **No se puede quitar Admin al último Admin**: El sistema previene dejar el sistema sin administradores
- **Los Admins siempre conservan Moderador**: Si un usuario tiene el rol Admin, automáticamente tiene acceso a todas las funciones de Moderador

---

## Auditoría del Sistema

**Nota**: Esta sección solo está disponible para usuarios con rol **Admin**.

### Ver Eventos de Auditoría

1. Ve a **"Auditoría"** en el panel de administración
2. Verás una lista completa de todos los cambios realizados en el sistema

### Filtrar Eventos

Puedes filtrar los eventos por:
- **Tipo de Entidad**: Curso, Taller, Usuario, Rol, Inscripción, Certificado
- **Acción**: Create, Update, Delete, ToggleActive, MakeModerador, RemoveModerador, etc.
- **Buscar**: Por email del usuario, tipo de entidad, acción o ID de entidad

### Información de Cada Evento

Cada evento muestra:
- **Fecha y Hora**: Cuándo ocurrió el cambio
- **Usuario**: Quién realizó el cambio (nombre y email)
- **Entidad**: Tipo de entidad afectada y su ID
- **Acción**: Qué acción se realizó
- **Detalles**: Botón para ver valores anteriores y nuevos (si aplica)
- **IP**: Dirección IP desde donde se realizó el cambio

### Ver Detalles de Cambios

1. Haz clic en **"Ver Cambios"** en cualquier evento
2. Se abrirá un modal mostrando:
   - **Valores Anteriores**: Estado antes del cambio (para Updates)
   - **Valores Nuevos**: Estado después del cambio
   - **Información Adicional**: Detalles adicionales del evento

### Paginación

Los eventos se muestran en páginas de 50 eventos. Usa la paginación en la parte inferior para navegar entre páginas.

### Importancia de la Auditoría

La auditoría es requerida por **NITSO** y permite:
- Rastrear todos los cambios en la base de datos
- Identificar quién hizo cada cambio y cuándo
- Investigar problemas o cambios no autorizados
- Cumplir con requisitos de seguridad y compliance

---

## Programas Internos y Control de Acceso

### Crear un Programa Interno

1. Al crear o editar un programa, desmarca **"Es Público"**
2. El programa solo será visible para usuarios con acceso

### Generar Clave de Acceso

1. Al crear o editar un programa interno, ingresa una **"Clave de Acceso"**
2. Esta clave puede ser compartida con usuarios específicos
3. Los usuarios ingresan esta clave en la página de programas formativos

### Generar Enlace de Acceso

1. En la página de edición del programa interno, haz clic en **"Generar Enlace"**
2. Se generará un enlace único que puedes copiar
3. Comparte este enlace por correo electrónico o cualquier otro medio
4. El enlace puede ser usado por múltiples usuarios

### Enviar Enlace por Correo

1. En la página de edición del programa interno, haz clic en **"Enviar enlace por correo"**
2. Busca al usuario por nombre o email
3. El sistema enviará automáticamente el enlace al usuario

### Agregar Usuario Directamente

1. En la página de **"Inscripciones"**, haz clic en **"Agregar Usuario"**
2. Busca al usuario
3. Selecciona un taller del programa interno
4. Al inscribir al usuario, automáticamente obtiene acceso al programa

### Flujo de Acceso

1. **Con código**: Usuario ingresa código → Obtiene acceso → Ve el programa
2. **Con enlace**: Usuario hace clic en enlace → Obtiene acceso → Ve el programa
3. **Agregado por admin**: Admin inscribe usuario en taller → Usuario obtiene acceso automático

---

## Certificados

### Generar Certificados

1. Ve a **"Certificados"** en el panel de administración
2. Selecciona los talleres para los cuales generar certificados
3. El sistema mostrará usuarios elegibles (con asistencia y encuesta completada)
4. Selecciona los usuarios
5. Haz clic en **"Generar Certificados"**

### Requisitos para Certificados

- Usuario inscrito en el taller
- Asistencia marcada
- Encuesta completada (si es requerida)
- Taller permite certificados

### Descargar Certificados

1. En la lista de certificados, haz clic en **"Descargar"**
2. El certificado se descargará en formato PDF

### Exportar Lista de Certificados

1. Haz clic en **"Exportar CSV"**
2. Se descargará un archivo con la lista de certificados generados

---

## Encuestas

### Ver Encuestas Completadas

1. Ve a **"Encuestas"** en el panel de administración
2. Verás las encuestas completadas por los usuarios
3. Puedes filtrar por taller o programa formativo

### Estadísticas de Encuestas

- Ver respuestas por pregunta
- Ver promedios de satisfacción
- Exportar resultados

---

## Estadísticas y Reportes

### Dashboard Principal

El dashboard muestra:
- **Programas Activos**: Número de programas activos
- **Talleres del Mes**: Talleres creados este mes
- **Inscripciones del Mes**: Inscripciones realizadas este mes
- **Usuarios Registrados**: Total de usuarios

### Gráficos

- Inscripciones por programa formativo
- Asistencia por taller
- Certificados generados
- Encuestas completadas

### Exportar Datos

Puedes exportar:
- Lista de inscripciones (CSV)
- Lista de certificados (CSV)
- Resultados de encuestas (CSV)

---

## Configuración del Sistema

### Configuración de Email

1. Ve a **"Configuración"** (solo Admin)
2. Configura:
   - Servidor SMTP
   - Puerto
   - Credenciales
   - Dirección de envío

### Configuración de Seguridad

- Políticas de contraseña (configuradas en el sistema)
- Bloqueo de cuentas después de intentos fallidos
- Requisitos de confirmación de email

---

## Mejores Prácticas

### Gestión de Programas Internos

1. **Usa enlaces para grupos grandes**: Si vas a compartir con muchas personas, usa el enlace de acceso
2. **Usa códigos para acceso controlado**: Si quieres controlar quién tiene acceso, usa códigos únicos
3. **Agrega usuarios directamente cuando sea necesario**: Para usuarios específicos, agrégalos directamente desde inscripciones

### 3.1 Gestión de Usuarios

**Ruta**: `/Admin/Usuarios`

Esta pantalla permite ver el listado de todos los usuarios registrados en el sistema (beneficiarios, moderadores y administradores).

**Funcionalidades**:

- **Filtrado Avanzado**: Puede filtrar por nombre, email, cédula, estado (Activo/Inactivo) o rol.
- **Exportación CSV**: Botón para descargar el listado actual (respetando los filtros aplicados) en formato CSV compatible con Excel.
- **Activar/Desactivar**: Bloquea el acceso al sistema (Lockout).
  - *Nota*: No se permite desactivar a usuarios que sean Administradores o Moderadores activos (medida de seguridad).
- **Asignar Roles**: Convertir a un usuario en Moderador.

---

### 3.2 Gestión de Inscripciones

**Ruta**: `/Admin/Inscripciones`

Permite controlar quién asiste a los talleres.

**Visualización**:
- Las inscripciones se muestran **agrupadas por Taller** (ordenado del más reciente al más antiguo).
- Cada grupo muestra el título del taller y el curso asociado.

**Acciones Disponibles**:

1. **Inscribir Usuario (Manual)**:
   - Botón para abrir modal.
   - Selección de taller (solo talleres ABIERTOS).
   - Búsqueda de usuario por nombre/cédula.
   - Valida cupos y duplicados.

2. **Control de Asistencia (Masivo)**:
   - Casillas de verificación (checkbox) para seleccionar múltiples usuarios.
   - Botones en la parte superior para **"Marcar Asistencia"** o **"Desmarcar Asistencia"** a todos los seleccionados en lote.

3. **Acciones Individuales**:
   - **Asistencia**: Marcar/Desmarcar asistencia individualmente.
   - **Cancelar**: Cancela la inscripción y libera el cupo (solo si el taller está abierto).

---

## Solución de Problemas

### Un usuario no puede ver un programa interno

1. Verifica que el programa esté **Activo**
2. Verifica que el usuario tenga acceso (código, enlace o inscripción directa)
3. Verifica que el usuario haya iniciado sesión

### Un usuario no puede inscribirse en un taller

1. Verifica que el taller esté **Abierto**
2. Verifica que haya **cupos disponibles**
3. Verifica que el usuario tenga el rol de **Beneficiario**
4. Si es programa interno, verifica que tenga acceso

### No se pueden generar certificados

1. Verifica que el usuario tenga **asistencia marcada**
2. Verifica que la **encuesta esté completada** (si es requerida)
3. Verifica que el taller **permita certificados**

---

## Soporte Técnico

Para problemas técnicos o consultas:
- Revisa los logs del sistema
- Contacta al equipo de desarrollo
- Consulta la documentación técnica en la carpeta `diseño`

---

**Última actualización**: Enero 2026
