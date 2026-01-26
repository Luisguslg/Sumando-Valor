# APIs y Endpoints - Sumando Valor

## Resumen

La aplicación utiliza **Razor Pages** (no Web API tradicional), por lo que los "endpoints" son páginas con handlers `OnGet` y `OnPost`. Este documento lista todas las rutas disponibles, sus parámetros, respuestas y requisitos de autorización.

## Endpoints Públicos

### Homepage
- **Ruta**: `/` o `/Index`
- **Método**: GET
- **Autorización**: Pública
- **Handler**: `IndexModel.OnGetAsync()`
- **Respuesta**: HTML con cursos destacados, talleres próximos, carrusel

### Acerca de
- **Ruta**: `/About`
- **Método**: GET
- **Autorización**: Pública
- **Handler**: `AboutModel.OnGetAsync()`
- **Respuesta**: HTML con información de la fundación

### Contacto
- **Ruta**: `/Contact`
- **Métodos**: GET, POST
- **Autorización**: Pública
- **Handlers**:
  - `ContactModel.OnGetAsync()` - Muestra formulario
  - `ContactModel.OnPostAsync()` - Procesa mensaje
- **Parámetros POST**:
  - `Input.Nombre` (string, required, max 100)
  - `Input.Email` (string, required, max 200, email format)
  - `Input.Titulo` (string, required, max 200)
  - `Input.Mensaje` (string, required, max 2000)
  - `Input.CaptchaToken` (string, opcional según configuración)
- **Respuesta**: Redirect con mensaje flash

### Cursos (Público)
- **Ruta**: `/Cursos`
- **Método**: GET
- **Autorización**: Pública
- **Handler**: `CursosModel.OnGetAsync()`
- **Respuesta**: HTML con listado de cursos activos

### Detalle de Curso
- **Ruta**: `/Cursos/Details?id={id}`
- **Método**: GET
- **Autorización**: Pública
- **Parámetros**:
  - `id` (int, required) - ID del curso
- **Handler**: `DetailsModel.OnGetAsync()`
- **Respuesta**: HTML con detalles del curso y talleres asociados

### Detalle de Taller
- **Ruta**: `/Talleres/Details?id={id}`
- **Método**: GET
- **Autorización**: Pública
- **Parámetros**:
  - `id` (int, required) - ID del taller
- **Handler**: `DetailsModel.OnGetAsync()`
- **Respuesta**: HTML con detalles del taller

## Endpoints de Autenticación

### Login
- **Ruta**: `/Account/Login`
- **Métodos**: GET, POST
- **Autorización**: Pública (usuarios no autenticados)
- **Handlers**:
  - `LoginModel.OnGetAsync(returnUrl?)` - Muestra formulario
  - `LoginModel.OnPostAsync(returnUrl?)` - Procesa login
- **Parámetros POST**:
  - `Input.Email` (string, required, email format)
  - `Input.Password` (string, required)
  - `Input.RememberMe` (bool, opcional)
  - `Input.CaptchaToken` (string, opcional según configuración)
- **Respuesta**: Redirect a `returnUrl` o `/` si éxito, página con errores si falla

### Registro
- **Ruta**: `/Account/Register`
- **Métodos**: GET, POST
- **Autorización**: Pública (usuarios no autenticados)
- **Handlers**:
  - `RegisterModel.OnGet(returnUrl?)` - Muestra formulario
  - `RegisterModel.OnPostAsync(returnUrl?)` - Procesa registro
- **Parámetros POST**:
  - `Input.Email` (string, required, email format)
  - `Input.Password` (string, required, min 8 chars, requiere mayúscula, minúscula, dígito)
  - `Input.ConfirmPassword` (string, required, debe coincidir con Password)
  - `Input.Nombres` (string, required, max 80)
  - `Input.Apellidos` (string, required, max 80)
  - `Input.Cedula` (string, required, max 20, único)
  - `Input.Sexo` (string, required)
  - `Input.FechaNacimiento` (DateTime, required)
  - `Input.TieneDiscapacidad` (bool)
  - `Input.DiscapacidadDescripcion` (string, max 120, opcional)
  - `Input.NivelEducativo` (string, required)
  - `Input.SituacionLaboral` (string, required)
  - `Input.CanalConocio` (string, required)
  - `Input.Estado` (string, required)
  - `Input.Ciudad` (string, required)
  - `Input.Telefono` (string, max 25, opcional)
  - `Input.CaptchaToken` (string, opcional según configuración)
- **Respuesta**: Redirect a `/Account/ConfirmEmail` si éxito, página con errores si falla
- **Efectos secundarios**: Envía email de confirmación

### Logout
- **Ruta**: `/Account/Logout`
- **Método**: POST (GET también disponible)
- **Autorización**: Autenticado
- **Handler**: `LogoutModel.OnPostAsync()`
- **Respuesta**: Redirect a `/Account/Login`

### Olvidé mi Contraseña
- **Ruta**: `/Account/ForgotPassword`
- **Métodos**: GET, POST
- **Autorización**: Pública
- **Handlers**:
  - `ForgotPasswordModel.OnGetAsync()` - Muestra formulario
  - `ForgotPasswordModel.OnPostAsync()` - Envía email de recuperación
- **Parámetros POST**:
  - `Input.Email` (string, required, email format)
- **Respuesta**: Redirect a `/Account/ForgotPasswordConfirmation`

### Resetear Contraseña
- **Ruta**: `/Account/ResetPassword?code={code}&email={email}`
- **Métodos**: GET, POST
- **Autorización**: Pública (con token válido)
- **Parámetros GET**:
  - `code` (string, required) - Token de reset
  - `email` (string, required) - Email del usuario
- **Parámetros POST**:
  - `Input.Email` (string, required)
  - `Input.Password` (string, required)
  - `Input.ConfirmPassword` (string, required)
  - `Input.Code` (string, required) - Token
- **Respuesta**: Redirect a `/Account/ResetPasswordConfirmation` si éxito

### Confirmar Email
- **Ruta**: `/Account/ConfirmEmail?userId={userId}&code={code}`
- **Método**: GET
- **Autorización**: Pública (con token válido)
- **Parámetros**:
  - `userId` (string, required)
  - `code` (string, required) - Token de confirmación
- **Handler**: `ConfirmEmailModel.OnGetAsync()`
- **Respuesta**: HTML con estado de confirmación

## Endpoints de Usuario (Beneficiario)

### Perfil
- **Ruta**: `/Profile`
- **Método**: GET
- **Autorización**: `[Authorize(Roles = "Beneficiario")]`
- **Handler**: `ProfileModel.OnGetAsync()`
- **Respuesta**: HTML con datos del perfil

### Mis Talleres
- **Ruta**: `/Profile/Talleres`
- **Método**: GET
- **Autorización**: `[Authorize(Roles = "Beneficiario")]`
- **Handler**: `TalleresModel.OnGetAsync()`
- **Respuesta**: HTML con listado de inscripciones del usuario

### Encuesta de Satisfacción
- **Ruta**: `/Profile/Encuesta?tallerId={id}`
- **Métodos**: GET, POST
- **Autorización**: `[Authorize(Roles = "Beneficiario")]`
- **Parámetros GET**:
  - `tallerId` (int, required)
- **Handlers**:
  - `EncuestaModel.OnGetAsync()` - Muestra formulario de encuesta
  - `EncuestaModel.OnPostSubmitEncuestaAsync()` - Procesa encuesta
- **Parámetros POST**:
  - `EncuestaInput.TallerId` (int, required)
  - `EncuestaInput.Answers` (Dictionary<int, string>) - Respuestas por QuestionId
- **Respuesta**: Redirect con mensaje flash

### Descargar Certificado
- **Ruta**: `/Certificates/Download?id={id}`
- **Método**: GET
- **Autorización**: Autenticado (solo el dueño del certificado)
- **Parámetros**:
  - `id` (int, required) - ID del certificado
- **Handler**: `DownloadModel.OnGetAsync()`
- **Respuesta**: PDF file (application/pdf) o 404/403

## Endpoints de Administración

**Nota**: Todos los endpoints de `/Admin/*` requieren `[Authorize(Roles = "Admin")]` o `[Authorize(Roles = "SuperAdmin")]`

### Dashboard
- **Ruta**: `/Admin` o `/Admin/Index`
- **Método**: GET
- **Autorización**: `[Authorize(Roles = "Admin")]`
- **Handler**: `AdminModel.OnGetAsync()`
- **Respuesta**: HTML con estadísticas (cursos activos, talleres abiertos, inscripciones del mes, % cupos ocupados)

### Cursos

#### Listado
- **Ruta**: `/Admin/Cursos`
- **Método**: GET
- **Handler**: `CursosModel.OnGetAsync()`
- **Respuesta**: HTML con listado de cursos

#### Crear
- **Ruta**: `/Admin/Cursos/Create`
- **Métodos**: GET, POST
- **Handlers**:
  - `CreateModel.OnGetAsync()` - Muestra formulario
  - `CreateModel.OnPostAsync()` - Crea curso
- **Parámetros POST**:
  - `Input.Titulo` (string, required, max 150)
  - `Input.Descripcion` (string, required)
  - `Input.PublicoObjetivo` (string, max 500, opcional)
  - `Input.EsPublico` (bool)
  - `Input.Estado` (EstatusCurso)
  - `Input.Orden` (int, opcional)

#### Editar
- **Ruta**: `/Admin/Cursos/Edit?id={id}`
- **Métodos**: GET, POST
- **Parámetros GET**:
  - `id` (int, required)
- **Handlers**:
  - `EditModel.OnGetAsync()` - Muestra formulario
  - `EditModel.OnPostAsync()` - Actualiza curso

### Talleres

#### Listado
- **Ruta**: `/Admin/Talleres`
- **Método**: GET
- **Handler**: `TalleresModel.OnGetAsync()`
- **Respuesta**: HTML con listado de talleres

#### Crear
- **Ruta**: `/Admin/Talleres/Create`
- **Métodos**: GET, POST
- **Parámetros POST**:
  - `Input.CursoId` (int, required)
  - `Input.Titulo` (string, required, max 200)
  - `Input.Descripcion` (string, max 2000, opcional)
  - `Input.FechaInicio` (DateTime, required)
  - `Input.FechaFin` (DateTime, opcional)
  - `Input.HoraInicio` (TimeSpan, required)
  - `Input.Modalidad` (ModalidadTaller, required)
  - `Input.PlataformaDigital` (string, max 200, opcional)
  - `Input.CuposMaximos` (int, required, > 0)
  - `Input.Estatus` (EstatusTaller)
  - `Input.PermiteCertificado` (bool)
  - `Input.RequiereEncuesta` (bool)
  - `Input.FacilitadorTexto` (string, max 200, opcional)

#### Editar
- **Ruta**: `/Admin/Talleres/Edit?id={id}`
- **Métodos**: GET, POST
- **Parámetros GET**:
  - `id` (int, required)

### Inscripciones
- **Ruta**: `/Admin/Inscripciones`
- **Método**: GET
- **Handler**: `InscripcionesModel.OnGetAsync()`
- **Parámetros GET** (opcionales, filtros):
  - `TallerId` (int)
  - `Estado` (int)
  - `Search` (string) - Búsqueda por nombre/email/cédula
  - `page` (int) - Paginación
- **Respuesta**: HTML con listado paginado de inscripciones

### Certificados
- **Ruta**: `/Admin/Certificados`
- **Método**: GET
- **Handler**: `CertificadosModel.OnGetAsync()`
- **Parámetros GET** (opcionales, filtros):
  - `TallerId` (int)
  - `Estado` (int)
  - `Search` (string)
  - `page` (int)
- **Respuesta**: HTML con listado de certificados

#### Aprobar Certificados
- **Ruta**: `/Admin/Certificados`
- **Método**: POST
- **Handler**: `CertificadosModel.OnPostApproveSelectedAsync()`
- **Parámetros POST**:
  - `SelectedInscripcionIds` (List<int>, required) - IDs de inscripciones a aprobar
- **Efectos secundarios**: 
  - Genera PDF del certificado
  - Guarda en `App_Data/Certificates/`
  - Envía email de notificación (si está configurado)
- **Respuesta**: Redirect con mensaje flash

#### Revocar Certificados
- **Ruta**: `/Admin/Certificados`
- **Método**: POST
- **Handler**: `CertificadosModel.OnPostRevokeSelectedAsync()`
- **Parámetros POST**:
  - `SelectedInscripcionIds` (List<int>, required)
- **Efectos secundarios**: Elimina PDF del certificado
- **Respuesta**: Redirect con mensaje flash

### Usuarios

#### Listado
- **Ruta**: `/Admin/Usuarios`
- **Método**: GET
- **Handler**: `UsuariosModel.OnGetAsync()`
- **Parámetros GET** (opcionales):
  - `Search` (string)
  - `Role` (string)
  - `page` (int)
- **Respuesta**: HTML con listado de usuarios

#### Editar Usuario
- **Ruta**: `/Admin/Usuarios/Edit?id={id}`
- **Métodos**: GET, POST
- **Parámetros GET**:
  - `id` (string, required) - UserId
- **Parámetros POST**:
  - `Input.Cedula` (string, required, max 20)
  - `Input.Nombres` (string, required, max 80)
  - `Input.Apellidos` (string, required, max 80)
  - `Input.Sexo` (string, required)
  - `Input.Telefono` (string, max 25, opcional)
  - `Input.IsActive` (bool)
- **Restricciones**: Solo SuperAdmin puede editar SuperAdmin
- **Efectos secundarios**: Registra evento en `AdminAuditEvents`

#### Toggle Activo/Inactivo
- **Ruta**: `/Admin/Usuarios`
- **Método**: POST
- **Handler**: `UsuariosModel.OnPostToggleActiveAsync()`
- **Parámetros POST**:
  - `userId` (string, required)
- **Restricciones**: No se puede desactivar el último Admin/SuperAdmin
- **Efectos secundarios**: Registra evento en `AdminAuditEvents`

#### Asignar/Remover Rol Admin
- **Ruta**: `/Admin/Usuarios`
- **Método**: POST
- **Handlers**:
  - `UsuariosModel.OnPostMakeAdminAsync()`
  - `UsuariosModel.OnPostRemoveAdminAsync()`
- **Parámetros POST**:
  - `userId` (string, required)
- **Restricciones**: Solo SuperAdmin puede modificar roles Admin
- **Efectos secundarios**: Registra evento en `AdminAuditEvents`

### Carrusel
- **Ruta**: `/Admin/Carrusel`
- **Métodos**: GET, POST
- **Handler**: `CarruselModel.OnGetAsync()`
- **POST Handlers**:
  - `OnPostUploadAsync()` - Sube nueva imagen
  - `OnPostDeleteAsync()` - Elimina imagen
  - `OnPostToggleActiveAsync()` - Activa/desactiva
  - `OnPostUpdateOrderAsync()` - Actualiza orden
- **Parámetros POST (Upload)**:
  - `file` (IFormFile, required, max 4MB, jpg/png/webp)
  - `altText` (string, required, max 200)
  - `sortOrder` (int)

### Imágenes del Sitio
- **Ruta**: `/Admin/Imagenes`
- **Métodos**: GET, POST
- **Handler**: `ImagenesModel.OnGetAsync()`
- **POST Handlers**:
  - `OnPostUploadAboutMainAsync()` - Imagen "Conócenos"
  - `OnPostDeleteAboutMainAsync()`
  - `OnPostUploadWorkshopCardAsync()` - Imagen por defecto "Mis Talleres"
  - `OnPostDeleteWorkshopCardAsync()`
  - `OnPostUploadHomePillarsAsync()` - Imagen "Formación con propósito"
  - `OnPostDeleteHomePillarsAsync()`
- **Parámetros POST (Upload)**:
  - `file` (IFormFile, required, max 4MB, jpg/png/webp)
  - `altText` (string, required, max 200)
- **Efectos secundarios**: Elimina imagen anterior al subir nueva

### Plantillas de Encuesta

#### Listado
- **Ruta**: `/Admin/SurveyTemplates`
- **Método**: GET
- **Handler**: `IndexModel.OnGetAsync()`

#### Crear
- **Ruta**: `/Admin/SurveyTemplates/Create`
- **Métodos**: GET, POST
- **Parámetros POST**:
  - `Input.Name` (string, required, max 200)
  - `Input.Description` (string, max 4000, opcional)
  - `Input.IsActive` (bool)
  - `Input.Questions` (List<QuestionInput>) - Preguntas del template

#### Editar
- **Ruta**: `/Admin/SurveyTemplates/Edit?id={id}`
- **Métodos**: GET, POST
- **Parámetros GET**:
  - `id` (int, required)

### Encuestas (Visualización)
- **Ruta**: `/Admin/Encuestas`
- **Método**: GET
- **Handler**: `EncuestasModel.OnGetAsync()`
- **Respuesta**: HTML con estadísticas y respuestas de encuestas

### Diagnóstico de Email (Solo Desarrollo)
- **Ruta**: `/Dev/Emails`
- **Método**: GET
- **Autorización**: Solo en entorno Development
- **Handler**: `EmailsModel.OnGetAsync()`
- **Respuesta**: HTML con listado de emails enviados (guardados en disco)

## Códigos de Respuesta HTTP

- **200 OK**: Página renderizada correctamente
- **302 Found (Redirect)**: Redirect después de POST exitoso
- **400 Bad Request**: Validación fallida (ModelState inválido)
- **401 Unauthorized**: Usuario no autenticado
- **403 Forbidden**: Usuario autenticado pero sin permisos
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

## Validaciones Comunes

### Archivos
- **Tamaño máximo**: 4MB
- **Formatos permitidos**: `.jpg`, `.jpeg`, `.png`, `.webp`
- **Validación de contenido**: Verificación de magic bytes del archivo

### Strings
- Validación de longitud según campo
- Sanitización de entrada (trim, encoding)
- Validación de formato (email, etc.)

### Autenticación
- Requiere email confirmado para login
- Lockout después de 5 intentos fallidos (10 minutos)
- Cookies HttpOnly, Secure en producción
