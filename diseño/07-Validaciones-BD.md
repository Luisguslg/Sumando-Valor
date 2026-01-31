# Validaciones y Reglas de Base de Datos - Sumando Valor

## Resumen

Este documento describe todas las reglas de validación, constraints, índices y reglas de negocio implementadas a nivel de base de datos.

## Constraints de Integridad Referencial

### Foreign Keys con ON DELETE RESTRICT

Estas relaciones previenen la eliminación de registros padre si tienen registros hijos asociados:

1. **Talleres → Cursos**
   - `FK_Talleres_Cursos_CursoId`
   - No se puede eliminar un Curso si tiene Talleres asociados

2. **Inscripciones → Talleres**
   - `FK_Inscripciones_Talleres_TallerId`
   - No se puede eliminar un Taller si tiene Inscripciones

3. **Inscripciones → AspNetUsers**
   - `FK_Inscripciones_AspNetUsers_UserId`
   - No se puede eliminar un Usuario si tiene Inscripciones

4. **Certificados → Talleres**
   - `FK_Certificados_Talleres_TallerId`
   - No se puede eliminar un Taller si tiene Certificados

5. **Certificados → AspNetUsers**
   - `FK_Certificados_AspNetUsers_UserId`
   - No se puede eliminar un Usuario si tiene Certificados

6. **EncuestasSatisfaccion → Talleres**
   - `FK_EncuestasSatisfaccion_Talleres_TallerId`
   - No se puede eliminar un Taller si tiene Encuestas

7. **EncuestasSatisfaccion → AspNetUsers**
   - `FK_EncuestasSatisfaccion_AspNetUsers_UserId`
   - No se puede eliminar un Usuario si tiene Encuestas

8. **SurveyResponses → SurveyTemplates**
   - `FK_SurveyResponses_SurveyTemplates_TemplateId`
   - No se puede eliminar un Template si tiene Responses

9. **SurveyResponses → Talleres**
   - `FK_SurveyResponses_Talleres_TallerId`
   - No se puede eliminar un Taller si tiene SurveyResponses

10. **SurveyResponses → AspNetUsers**
    - `FK_SurveyResponses_AspNetUsers_UserId`
    - No se puede eliminar un Usuario si tiene SurveyResponses

### Foreign Keys con ON DELETE CASCADE

Estas relaciones eliminan automáticamente los registros hijos al eliminar el padre:

1. **SurveyQuestions → SurveyTemplates**
   - `FK_SurveyQuestions_SurveyTemplates_TemplateId`
   - Al eliminar un Template, se eliminan sus Questions

2. **SurveyAnswers → SurveyResponses**
   - `FK_SurveyAnswers_SurveyResponses_ResponseId`
   - Al eliminar una Response, se eliminan sus Answers

## Unique Constraints

### AspNetUsers
- **Email**: `IX_AspNetUsers_Email` (Unique)
  - Regla: Cada email debe ser único en el sistema
  - Validación: A nivel de aplicación y BD

- **Cedula**: `IX_AspNetUsers_Cedula` (Unique)
  - Regla: Cada cédula debe ser única
  - Validación: A nivel de aplicación y BD

### SiteImages
- **Key**: `IX_SiteImages_Key` (Unique)
  - Regla: Cada clave lógica debe ser única (ej: solo un "AboutMain", un "WorkshopCard")
  - Propósito: Garantizar un solo valor por slot de imagen

### Inscripciones
- **Combinación (TallerId, UserId)**: `IX_Inscripciones_TallerId_UserId` (Unique)
  - Regla: Un usuario solo puede inscribirse una vez por taller
  - Validación: A nivel de aplicación y BD

### Certificados
- **Combinación (TallerId, UserId)**: `IX_Certificados_TallerId_UserId` (Unique)
  - Regla: Solo puede existir un certificado por combinación usuario-taller
  - Propósito: Evitar duplicados

### EncuestasSatisfaccion
- **Combinación (TallerId, UserId)**: `IX_EncuestasSatisfaccion_TallerId_UserId` (Unique)
  - Regla: Un usuario solo puede responder una encuesta por taller
  - Propósito: Evitar respuestas duplicadas

### SurveyResponses
- **Combinación (TallerId, UserId)**: `IX_SurveyResponses_TallerId_UserId` (Unique)
  - Regla: Un usuario solo puede responder una vez por taller (sistema de encuestas v2)
  - Propósito: Evitar respuestas duplicadas

## Constraints de NOT NULL

### Campos Requeridos por Tabla

#### AspNetUsers
- `Nombres` (Required, MaxLength: 80)
- `Apellidos` (Required, MaxLength: 80)
- `Cedula` (Required, MaxLength: 20)
- `Sexo` (Required)
- `FechaNacimiento` (Required)
- `NivelEducativo` (Required)
- `SituacionLaboral` (Required)
- `CanalConocio` (Required)
- `Estado` (Required)
- `Ciudad` (Required)

#### Cursos
- `Titulo` (Required, MaxLength: 150)
- `Descripcion` (Required)

#### Talleres
- `CursoId` (Required)
- `Titulo` (Required, MaxLength: 200)
- `FechaInicio` (Required)
- `HoraInicio` (Required)
- `Modalidad` (Required)
- `CuposMaximos` (Required)
- `CuposDisponibles` (Required)
- `Estatus` (Required)

#### Inscripciones
- `TallerId` (Required)
- `UserId` (Required)
- `Estado` (Required)
- `Asistencia` (Required, default: false)

#### Certificados
- `TallerId` (Required)
- `UserId` (Required)
- `Estado` (Required)

#### EncuestasSatisfaccion
- `TallerId` (Required)
- `UserId` (Required)
- `Rating1_5` (Required)

#### MensajesContacto
- `Nombre` (Required, MaxLength: 100)
- `Email` (Required, MaxLength: 200)
- `Titulo` (Required, MaxLength: 200)
- `Mensaje` (Required, MaxLength: 2000)

#### CarouselItems
- `FileName` (Required, MaxLength: 260)
- `AltText` (Required, MaxLength: 200)

#### SiteImages
- `Key` (Required, MaxLength: 80)
- `FileName` (Required, MaxLength: 260)
- `AltText` (Required, MaxLength: 200)

#### AuditLogs
- `TableName` (Required, MaxLength: 100)
- `Action` (Required, MaxLength: 20)

#### SurveyTemplates
- `Name` (Required, MaxLength: 200)

#### SurveyQuestions
- `TemplateId` (Required)
- `Type` (Required)
- `Text` (Required, MaxLength: 500)
- `Order` (Required)
- `IsRequired` (Required, default: true)

#### SurveyResponses
- `TemplateId` (Required)
- `TallerId` (Required)
- `UserId` (Required, MaxLength: 450)
- `CreatedAt` (Required)

#### SurveyAnswers
- `ResponseId` (Required)
- `QuestionId` (Required)
- `Value` (Required, MaxLength: 2000)

## Constraints de Longitud (MaxLength)

### Strings con Límite de Longitud

| Tabla | Campo | MaxLength | Propósito |
|-------|-------|-----------|-----------|
| AspNetUsers | Nombres | 80 | Prevenir strings excesivamente largos |
| AspNetUsers | Apellidos | 80 | Prevenir strings excesivamente largos |
| AspNetUsers | Cedula | 20 | Formato de cédula venezolana |
| AspNetUsers | DiscapacidadDescripcion | 120 | Descripción opcional |
| AspNetUsers | Telefono | 25 | Formato internacional |
| Cursos | Titulo | 150 | Título del curso |
| Cursos | PublicoObjetivo | 500 | Descripción del público |
| Talleres | Titulo | 200 | Título del taller |
| Talleres | Descripcion | 2000 | Descripción detallada |
| Talleres | PlataformaDigital | 200 | URL o nombre de plataforma |
| Talleres | FacilitadorTexto | 200 | Nombre del facilitador |
| EncuestasSatisfaccion | Comentario | 2000 | Comentario opcional |
| MensajesContacto | Nombre | 100 | Nombre del remitente |
| MensajesContacto | Email | 200 | Email del remitente |
| MensajesContacto | Titulo | 200 | Asunto del mensaje |
| MensajesContacto | Mensaje | 2000 | Contenido del mensaje |
| CarouselItems | FileName | 260 | Ruta completa de archivo Windows |
| CarouselItems | AltText | 200 | Texto alternativo |
| SiteImages | Key | 80 | Clave lógica |
| SiteImages | FileName | 260 | Ruta completa de archivo |
| SiteImages | AltText | 200 | Texto alternativo |
| AuditLogs | TableName | 100 | Tabla afectada |
| AuditLogs | Action | 20 | INSERT, UPDATE, DELETE |
| SurveyTemplates | Name | 200 | Nombre del template |
| SurveyTemplates | Description | 4000 | Descripción del template |
| SurveyQuestions | Text | 500 | Texto de la pregunta |
| SurveyQuestions | OptionsJson | 4000 | Configuración JSON |
| SurveyResponses | UserId | 450 | Compatible con Identity |
| SurveyAnswers | Value | 2000 | Respuesta del usuario |

## Constraints de Tipo de Dato

### Decimales con Precisión
- **EncuestasSatisfaccion.ScorePromedio**: `decimal(5,2)`
  - Permite valores de 0.00 a 999.99
  - Nullable (opcional)

### Enumeraciones
Todas las enumeraciones se almacenan como `int` en la BD:

- `EstatusCurso`: 1=Activo, 2=Inactivo
- `EstatusTaller`: 1=Abierto, 2=Cerrado, 3=Cancelado, 4=Finalizado
- `ModalidadTaller`: 1=Presencial, 2=Virtual, 3=Hibrido
- `EstadoInscripcion`: 1=Activa, 2=Cancelada
- `EstadoCertificado`: 1=Pendiente, 2=Aprobado, 3=Rechazado
- `EstadoMensaje`: 1=Nuevo, 2=Leido, 3=Archivado
- `SurveyQuestionType`: 1=Rating1To5, 2=Text, 3=SingleChoice, 4=ScoreNumber, 5=Description

## Índices para Performance

### Índices No Únicos

#### Cursos
- `IX_Cursos_Estado`: Búsqueda por estado
- `IX_Cursos_Orden`: Ordenamiento

#### Talleres
- `IX_Talleres_CursoId`: Join con Cursos
- `IX_Talleres_FechaInicio`: Filtrado por fecha
- `IX_Talleres_Estatus`: Filtrado por estatus

#### Inscripciones
- `IX_Inscripciones_TallerId`: Join con Talleres
- `IX_Inscripciones_UserId`: Join con Usuarios

#### Certificados
- `IX_Certificados_TallerId`: Join con Talleres
- `IX_Certificados_UserId`: Join con Usuarios

#### EncuestasSatisfaccion
- `IX_EncuestasSatisfaccion_TallerId`: Join con Talleres
- `IX_EncuestasSatisfaccion_UserId`: Join con Usuarios

#### MensajesContacto
- `IX_MensajesContacto_CreatedAt`: Ordenamiento por fecha

#### CarouselItems
- `IX_CarouselItems_SortOrder`: Ordenamiento del carrusel
- `IX_CarouselItems_IsActive`: Filtrado de activos

#### SiteImages
- `IX_SiteImages_UpdatedAt`: Ordenamiento por fecha de actualización

#### AuditLogs
- `IX_AuditLogs_TableName`: Filtrado por tabla
- `IX_AuditLogs_CreatedAt`: Ordenamiento por fecha
- `IX_AuditLogs_UserId`: Búsqueda por usuario

#### SurveyTemplates
- `IX_SurveyTemplates_IsActive`: Filtrado de activos

#### SurveyQuestions
- `IX_SurveyQuestions_TemplateId_Order`: Ordenamiento de preguntas

#### SurveyAnswers
- `IX_SurveyAnswers_QuestionId`: Join con Questions

## Reglas de Negocio Implementadas en BD

### 1. Unicidad de Inscripción
- Un usuario no puede inscribirse dos veces en el mismo taller
- Implementado con: Unique constraint `IX_Inscripciones_TallerId_UserId`

### 2. Unicidad de Certificado
- Solo puede existir un certificado por usuario-taller
- Implementado con: Unique constraint `IX_Certificados_TallerId_UserId`

### 3. Unicidad de Encuesta
- Un usuario solo puede responder una encuesta por taller
- Implementado con: Unique constraints en `EncuestasSatisfaccion` y `SurveyResponses`

### 4. Integridad de Relaciones
- No se pueden eliminar registros padre con hijos (RESTRICT)
- Implementado con: Foreign keys con ON DELETE RESTRICT

### 5. Cascada de Eliminación
- Al eliminar un SurveyTemplate, se eliminan sus Questions
- Al eliminar una SurveyResponse, se eliminan sus Answers
- Implementado con: Foreign keys con ON DELETE CASCADE

## Validaciones a Nivel de Aplicación (Complementarias)

Estas validaciones se realizan en el código C# antes de guardar en BD:

1. **Validación de formato de email**: Regex en `RegisterModel`
2. **Validación de contraseña**: Reglas de Identity (min 8 chars, mayúscula, minúscula, dígito)
3. **Validación de archivos**: Magic bytes, tamaño máximo (4MB)
4. **Validación de fechas**: FechaInicio < FechaFin (si ambas presentes)
5. **Validación de cupos**: CuposDisponibles <= CuposMaximos
6. **Validación de rating**: Rating1_5 entre 1 y 5

## Migraciones y Versionado

Las migraciones se encuentran en:
`src/SumandoValor.Infrastructure/Migrations/`

Cada migración incluye:
- `Up()`: Aplica cambios
- `Down()`: Revierte cambios
- Validaciones defensivas (IF EXISTS) para evitar errores en BD existentes
