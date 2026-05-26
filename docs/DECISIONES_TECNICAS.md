# Decisiones técnicas y extensibilidad

## Arquitectura

- Separación por capas (Domain/Application/Infrastructure/Wpf).
- UI desacoplada mediante ViewModel.
- DI centralizada en `Infrastructure.DependencyInjection`.

## Extensión recomendada

- Implementar `IEmailClient` con EWS + OAuth (MSAL) según modo delegado/app-only.
- Agregar `IPdfService` y `IStickerService` para generación real de PDF/stickers.
- Integrar post-procesamiento automático por correo y por lote.
- Incorporar copia a `BCS_IMG` con servicio dedicado y trazabilidad de recopiado.

## Seguridad

- Secretos fuera del código (variables de entorno).
- Plantilla segura en `appsettings.example.json`.
