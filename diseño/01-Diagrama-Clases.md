# Diagrama de Clases - Sumando Valor

## Modelo de Dominio

Este documento describe las clases principales del modelo de dominio de la aplicación.

```mermaid
classDiagram
    %% Entidades principales
    class ApplicationUser {
        +string Id
        +string Email
        +string Nombres
        +string Apellidos
        +string Cedula
        +DateTime CreatedAt
        +ICollection~Inscripcion~ Inscripciones
        +ICollection~Certificado~ Certificados
        +ICollection~EncuestaSatisfaccion~ Encuestas
    }

    class Curso {
        +int Id
        +string Titulo
        +string Descripcion
        +EstatusCurso Estado
        +ICollection~Taller~ Talleres
    }

    class Taller {
        +int Id
        +string Titulo
        +DateTime FechaInicio
        +int CuposMaximos
        +EstatusTaller Estatus
        +Curso Curso
        +ICollection~Inscripcion~ Inscripciones
        +ICollection~Certificado~ Certificados
        +ICollection~EncuestaSatisfaccion~ Encuestas
    }

    class Inscripcion {
        +int Id
        +EstadoInscripcion Estado
        +bool Asistencia
        +Taller Taller
    }

    class Certificado {
        +int Id
        +EstadoCertificado Estado
        +string UrlPdf
        +Taller Taller
    }

    class EncuestaSatisfaccion {
        +int Id
        +int Rating1_5
        +Taller Taller
    }

    %% Sistema de Encuestas
    class SurveyTemplate {
        +int Id
        +string Name
        +bool IsActive
        +ICollection~SurveyQuestion~ Questions
        +ICollection~SurveyResponse~ Responses
    }

    class SurveyQuestion {
        +int Id
        +string Text
        +SurveyQuestionType Type
        +SurveyTemplate Template
    }

    class SurveyResponse {
        +int Id
        +SurveyTemplate Template
        +ICollection~SurveyAnswer~ Answers
    }

    class SurveyAnswer {
        +int Id
        +string Value
        +SurveyResponse Response
    }

    %% Entidades auxiliares
    class MensajeContacto {
        +int Id
        +string Nombre
        +string Email
    }

    class CarouselItem {
        +int Id
        +string FileName
        +bool IsActive
    }

    class SiteImage {
        +int Id
        +string Key
        +string FileName
    }

    class AdminAuditEvent {
        +int Id
        +string Action
        +DateTime CreatedAt
    }

    %% Relaciones principales - Core del negocio
    ApplicationUser "1" --> "*" Inscripcion : tiene
    ApplicationUser "1" --> "*" Certificado : tiene
    ApplicationUser "1" --> "*" EncuestaSatisfaccion : tiene
    
    Curso "1" --> "*" Taller : contiene
    
    Taller "1" --> "*" Inscripcion : tiene
    Taller "1" --> "*" Certificado : genera
    Taller "1" --> "*" EncuestaSatisfaccion : tiene

    %% Relaciones - Sistema de Encuestas
    SurveyTemplate "1" --> "*" SurveyQuestion : contiene
    SurveyTemplate "1" --> "*" SurveyResponse : tiene
    SurveyResponse "1" --> "*" SurveyAnswer : contiene
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
