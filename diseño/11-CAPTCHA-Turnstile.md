# CAPTCHA en Sumando Valor

## Opción por defecto: CAPTCHA matemático (Math)

**No hace falta configurar nada.** La aplicación usa por defecto un CAPTCHA matemático: una pregunta tipo *"¿Cuánto es 5 + 3?"* que el usuario responde en un cuadro de texto. Se valida en el servidor con la sesión; no hay cuentas externas ni claves.

- **Dónde se usa**: Login, Registro y formulario de Contacto.
- **Configuración**: En `appsettings.json` → `Captcha:Provider` = **"Math"** (es el valor por defecto).
- **Para desactivar**: Pon `"Provider": "None"` si no quieres ninguna verificación.

---

## Opción alternativa: Cloudflare Turnstile

Si prefieres un CAPTCHA externo (Cloudflare Turnstile, más invisible para el usuario pero requiere claves):

### ¿Qué es Turnstile?

**Turnstile** es un servicio de **Cloudflare** que sustituye los CAPTCHA clásicos. Suele mostrar una verificación casi invisible o muy rápida para usuarios reales y bloquea bots. Es gratuito.

### Cómo activar Turnstile

1. **Obtener las claves**: Entra en [dash.cloudflare.com](https://dash.cloudflare.com) → **Turnstile** → crea un widget. Cloudflare te da una **Site Key** (pública) y una **Secret Key** (privada).
2. **Configurar**: En `appsettings.json` (o variables de entorno en producción):
   ```json
   "Captcha": {
     "Provider": "Turnstile",
     "CloudflareTurnstile": {
       "SiteKey": "TU_SITE_KEY",
       "SecretKey": "TU_SECRET_KEY"
     }
   }
   ```
3. **Probar en desarrollo**: Puedes usar las claves de prueba de Cloudflare (siempre pasan): SiteKey `1x00000000000000000000AA`, SecretKey `1x0000000000000000000000000000000AA`. En producción usa tus propias claves.

Documentación oficial: [developers.cloudflare.com/turnstile](https://developers.cloudflare.com/turnstile/).

---

## Resumen

| Valor de `Captcha:Provider` | Comportamiento |
|-----------------------------|----------------|
| **Math** (por defecto) | Pregunta numérica ("¿Cuánto es 5 + 3?"). Sin cuentas ni claves. |
| **Turnstile** | Widget de Cloudflare. Requiere Site Key y Secret Key. |
| **None** | No se muestra ninguna verificación (no recomendado en producción). |

En **Admin → Seguridad** puedes ver qué proveedor está activo y si las claves de Turnstile están configuradas (sin mostrar las claves).
