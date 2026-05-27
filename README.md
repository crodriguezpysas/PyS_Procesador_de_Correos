# Procesador Correos PYS

Aplicación WPF .NET 8 (MVVM) para descarga/procesamiento de correos Exchange/M365 con trazabilidad operativa, deduplicación, generación de PDF/stickers y exportación incremental CSV/Excel.

## Estructura

- `src/ProcesadorCorreosPYS.Domain`
- `src/ProcesadorCorreosPYS.Application`
- `src/ProcesadorCorreosPYS.Infrastructure`
- `src/ProcesadorCorreosPYS.Wpf`
- `tests/ProcesadorCorreosPYS.Tests`
- `deploy/`

## Requisitos

- Visual Studio 2026 (Desktop development with .NET)
- .NET SDK 8+
- Acceso Exchange Online con app registration válida para EWS/OAuth
- (Opcional) `wkhtmltopdf` para generación PDF por motor nativo

## Configuración segura

1. Copie `src/ProcesadorCorreosPYS.Wpf/appsettings.example.json` a `src/ProcesadorCorreosPYS.Wpf/appsettings.json`.
2. Configure:
   - `OAuth.TenantId`
   - `OAuth.ClientId`
   - `OAuth.Mailbox` (si se usa impersonación app-only)
3. **Nunca** guarde secretos en repositorio. Use variables de entorno:
   - `OAuth__ClientSecret`
4. Seleccione ambiente con `DOTNET_ENVIRONMENT` (`Development` / `Production`).

## Ejecución

```bash
dotnet restore
dotnet build ProcesadorCorreosPYS.slnx
dotnet test tests/ProcesadorCorreosPYS.Tests/ProcesadorCorreosPYS.Tests.csproj
```

En Visual Studio:
1. Abrir `ProcesadorCorreosPYS.slnx`
2. Startup project: `ProcesadorCorreosPYS.Wpf`
3. Ejecutar

## Funcionalidades implementadas para salida productiva

- Cliente EWS + OAuth (MSAL) configurable en modo delegado o app-only.
- Flujo manual y automático de procesamiento con ventana horaria.
- Deduplicación:
  - Primaria: `processed_ids.txt` diario
  - Secundaria: SHA256 (16 hex) en `UniqueId`
- Estructura operativa:
  - `BaseFolder\yyMM\yyMMdd\0001\...`
  - `Reports\yyyyMMdd\...`
- Generación:
  - `0.html`
  - `0.pdf` (wkhtmltopdf + fallback managed QuestPDF)
  - sticker PDF por correo `PSyyMMddNNNN.pdf`
- Adjuntos:
  - Descarga con reintentos
  - Adjuntos vacíos no descartados y registrados
  - Sanitización y resolución de duplicados
  - Placeholder para DOC/DOCX
- Exportación incremental:
  - CSV `|`
  - Excel Resumen + Alternativo (12 columnas)
- Copia a `BCS_IMG\AAMM\AAMMDD` con control de recopiado.
- Logging estructurado en UI y archivo con etiquetas: `DUP`, `ATT`, `PDF`, `STICKER`, `FILE`, `WAIT`.

## Publicación e instalador

### Publicación self-contained

```powershell
cd deploy
./Publish.ps1 -Configuration Release -Runtime win-x64 -Output ../publish/win-x64
```

### Instalador Inno Setup

1. Publicar binarios (paso anterior)
2. Abrir `deploy/ProcesadorCorreosPYS.iss` con Inno Setup 6
3. Compilar instalador

## Checklist de liberación

- [ ] Credenciales OAuth por variables de entorno (sin secretos en git)
- [ ] Validación de acceso EWS (delegado o app-only)
- [ ] Prueba manual completa (procesar, extracción offline, sticker, excel)
- [ ] Prueba de ciclo automático en ventana y fuera de ventana
- [ ] Verificación de CSV/Excel incrementales y copia a BCS_IMG
- [ ] Build/Test/Validación de seguridad aprobados
- [ ] Plan de rollback documentado

## Troubleshooting

- Si no hay correos procesados, validar permisos OAuth y buzón objetivo.
- Si falla wkhtmltopdf, dejar `Processing.WkHtmlToPdfPath` vacío para usar fallback managed.
- Si hay `PENDIENTE_REVISION`, revisar incidencias de adjuntos/PDF en metadata y logs.
