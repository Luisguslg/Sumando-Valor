# Manual de Administrador - Sumando Valor

## Índice

1. [Introducción](#introducción)
2. [Acceso al Panel de Administración](#acceso-al-panel-de-administración)
3. [Gestión de Programas Formativos](#gestión-de-programas-formativos)
4. [Gestión de Talleres](#gestión-de-talleres)
5. [Gestión de Inscripciones](#gestión-de-inscripciones)
6. [Gestión de Usuarios](#gestión-de-usuarios)
7. [Programas Internos y Control de Acceso](#programas-internos-y-control-de-acceso)
8. [Certificados](#certificados)
9. [Encuestas](#encuestas)
10. [Estadísticas y Reportes](#estadísticas-y-reportes)
11. [Configuración del Sistema](#configuración-del-sistema)

---

## Introducción

Este manual está dirigido a los administradores de la plataforma **Sumando Valor**. Cubre todas las funcionalidades de administración disponibles en el sistema.

### Roles de Administración

- **Administrador**: Acceso completo al panel de administración
- **Super Administrador**: Acceso completo + configuración del sistema

---

## Acceso al Panel de Administración

### Requisitos

- Tener el rol de **Administrador** o **Super Administrador**
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

1. Ve a **"Configuración"** (solo Super Administrador)
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

### Gestión de Talleres

1. **Crea talleres con suficiente anticipación**: Permite que los usuarios se inscriban con tiempo
2. **Actualiza cupos según necesidad**: Ajusta los cupos según la demanda
3. **Marca asistencia después de cada taller**: Es necesario para generar certificados

### Gestión de Inscripciones

1. **Revisa regularmente las inscripciones**: Mantén un control de quién está inscrito
2. **Cancela inscripciones cuando sea necesario**: Libera cupos para otros usuarios
3. **Marca asistencia puntualmente**: Facilita la generación de certificados

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
