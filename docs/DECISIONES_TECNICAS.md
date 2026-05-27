# Decisiones técnicas y extensibilidad

## Arquitectura

- Clean architecture por capas:
  - Domain: entidades/reglas base
  - Application: casos de uso y orquestación
  - Infrastructure: EWS/MSAL, PDF, stickers, almacenamiento, exportaciones
  - WPF: MVVM + binding + comandos
- DI con `Microsoft.Extensions.DependencyInjection`.
- Configuración por ambiente (`appsettings.*` + variables de entorno).

## Procesamiento

- Dedupe primaria: `processed_ids.txt` diario.
- Dedupe secundaria: `SHA256(FechaHora|Asunto|ListaAdjuntos)` truncado a 16 hex.
- Estado diario: `lastProcessedState_yyyyMMdd.txt` y `consecutivo_state.json`.
- Persistencia por correo: `metadata.json` por subcarpeta para reconstrucción offline.

## Integraciones

- EWS OAuth2 con MSAL:
  - Delegado: `AcquireTokenInteractive`
  - App-only: `AcquireTokenForClient`
- PDF:
  - preferencia `wkhtmltopdf`
  - fallback managed `QuestPDF`
- Stickers: `QuestPDF`

## Observabilidad

- Logs de archivo con Serilog.
- Log UI en panel Manual/Automático con sink observable.
- Tags operativos: `DUP`, `ATT`, `PDF`, `STICKER`, `FILE`, `WAIT`.

## Seguridad

- Secretos fuera del código (env vars).
- `appsettings.example.json` sin secretos reales.
- `app_config.xml` y `appsettings.json` ignorados por git.

## Operación

- Script de publicación self-contained (`deploy/Publish.ps1`).
- Instalador base Inno Setup (`deploy/ProcesadorCorreosPYS.iss`).
- Recomendación: pipeline CI con build/test/validación seguridad antes de release.
