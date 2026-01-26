# Diagrama de Clases - Sumando Valor

## Modelo de Dominio

Este documento describe las clases principales del modelo de dominio de la aplicación.

```mermaid
classDiagram
    class ApplicationUser {
        +string Id
        +string Email
        +string UserName
        +string Nombres
        +string Apellidos
        +string Cedula
        +string Sexo
        +DateTime FechaNacimiento
        +bool TieneDiscapacidad
        +string DiscapacidadDescripcion
        +string NivelEducativo
        +string SituacionLaboral
        +string CanalConocio
        +string Estado
        +string Ciudad
        +string Telefono
        +DateTime CreatedAt
        +ICollection~Inscripcion~ Inscripciones
        +ICollection~Certificado~ Certificados
        +ICollection~EncuestaSatisfaccion~ Encuestas
    }

    class Curso {
        +int Id
        +string Titulo
        +string Descripcion
        +string PublicoObjetivo
        +bool EsPublico
        +DateTime FechaCreacion
        +EstatusCurso Estado
        +int? Orden
        +ICollection~Taller~ Talleres
    }

    class Taller {
        +int Id
        +int CursoId
        +string Titulo
        +string Descripcion
        +DateTime FechaInicio
        +DateTime? FechaFin
        +TimeSpan HoraInicio
        +ModalidadTaller Modalidad
        +string PlataformaDigital
        +int CuposMaximos
        +int CuposDisponibles
        +EstatusTaller Estatus
        +bool PermiteCertificado
        +bool RequiereEncuesta
        +string FacilitadorTexto
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +Curso Curso
        +ICollection~Inscripcion~ Inscripciones
        +ICollection~Certificado~ Certificados
        +ICollection~EncuestaSatisfaccion~ Encuestas
    }

    class Inscripcion {
        +int Id
        +int TallerId
        +string UserId
        +EstadoInscripcion Estado
        +bool Asistencia
        +DateTime CreatedAt
        +Taller Taller
    }

    class Certificado {
        +int Id
        +int TallerId
        +string UserId
        +EstadoCertificado Estado
        +string UrlPdf
        +DateTime? IssuedAt
        +DateTime CreatedAt
        +Taller Taller
    }

    class EncuestaSatisfaccion {
        +int Id
        +int TallerId
        +string UserId
        +int Rating1_5
        +string Comentario
        +string PayloadJson
        +decimal? ScorePromedio
        +DateTime CreatedAt
        +Taller Taller
    }

    class MensajeContacto {
        +int Id
        +string Nombre
        +string Email
        +string Titulo
        +string Mensaje
        +EstadoMensaje Estado
        +DateTime CreatedAt
    }

    class CarouselItem {
        +int Id
        +string FileName
        +string AltText
        +int SortOrder
        +bool IsActive
        +DateTime CreatedAt
    }

    class SiteImage {
        +int Id
        +string Key
        +string FileName
        +string AltText
        +DateTime UpdatedAt
    }

    class AdminAuditEvent {
        +int Id
        +string ActorUserId
        +string TargetUserId
        +string Action
        +string DetailsJson
        +DateTime CreatedAt
    }

    class SurveyTemplate {
        +int Id
        +string Name
        +string Description
        +bool IsActive
        +DateTime CreatedAt
        +ICollection~SurveyQuestion~ Questions
    }

    class SurveyQuestion {
        +int Id
        +int TemplateId
        +SurveyQuestionType Type
        +string Text
        +int Order
        +bool IsRequired
        +string OptionsJson
        +SurveyTemplate Template
    }

    class SurveyResponse {
        +int Id
        +int TemplateId
        +int TallerId
        +string UserId
        +DateTime CreatedAt
        +SurveyTemplate Template
        +ICollection~SurveyAnswer~ Answers
    }

    class SurveyAnswer {
        +int Id
        +int ResponseId
        +int QuestionId
        +string Value
        +SurveyResponse Response
    }

    %% Relaciones
    ApplicationUser ||--o{ Inscripcion : "tiene"
    ApplicationUser ||--o{ Certificado : "tiene"
    ApplicationUser ||--o{ EncuestaSatisfaccion : "tiene"
    
    Curso ||--o{ Taller : "contiene"
    
    Taller ||--o{ Inscripcion : "tiene"
    Taller ||--o{ Certificado : "genera"
    Taller ||--o{ EncuestaSatisfaccion : "tiene"
    
    SurveyTemplate ||--o{ SurveyQuestion : "contiene"
    SurveyTemplate ||--o{ SurveyResponse : "tiene"
    SurveyResponse ||--o{ SurveyAnswer : "contiene"
```

## Enumeraciones

### EstatusCurso
- `Activo = 1`
- `Inactivo = 2`

### EstatusTaller
- `Abierto = 1`
- `Cerrado = 2`
- `Cancelado = 3`
- `Finalizado = 4`

### ModalidadTaller
- `Presencial = 1`
- `Virtual = 2`
- `Hibrido = 3`

### EstadoInscripcion
- `Activa = 1`
- `Cancelada = 2`

### EstadoCertificado
- `Pendiente = 1`
- `Aprobado = 2`
- `Rechazado = 3`

### EstadoMensaje
- `Nuevo = 1`
- `Leido = 2`
- `Archivado = 3`

### SurveyQuestionType
- `Rating1To5 = 1` - Radio buttons 1-5
- `Text = 2` - Texto corto/medio
- `SingleChoice = 3` - Opciones de radio
- `ScoreNumber = 4` - Puntuación numérica
- `Description = 5` - Texto largo/descripción
