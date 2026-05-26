# Procesador Correos PYS

Aplicación WPF .NET 8 (MVVM) para operación manual/automática de procesamiento de correos, con arquitectura por capas:

- `src/ProcesadorCorreosPYS.Domain`
- `src/ProcesadorCorreosPYS.Application`
- `src/ProcesadorCorreosPYS.Infrastructure`
- `src/ProcesadorCorreosPYS.Wpf`
- `tests/ProcesadorCorreosPYS.Tests`

## Requisitos

- Visual Studio 2026 (Desktop development with .NET)
- .NET SDK 8+

## Configuración segura

1. Copie `src/ProcesadorCorreosPYS.Wpf/appsettings.example.json` a `src/ProcesadorCorreosPYS.Wpf/appsettings.json`.
2. Defina secretos por variable de entorno (ejemplo):
   - `OAuth__ClientSecret`
3. **No** subir secretos al repositorio (`app_config.xml` y `appsettings.json` están ignorados).

## Ejecución local

```bash
dotnet restore
dotnet build ProcesadorCorreosPYS.slnx
```

En Visual Studio: abrir `ProcesadorCorreosPYS.slnx`, establecer `ProcesadorCorreosPYS.Wpf` como startup y ejecutar.

## Funcionalidad implementada (base operativa)

- UI con pestañas **Manual / Automático / Ayuda-Requerimientos**.
- Servicios base:
  - Deduplicación secundaria SHA256 (16 hex) (`UniqueIdService`).
  - Sanitización de nombres de adjunto compatible Windows (`FileNameSanitizer`).
  - Parser de rango/lista para subcarpetas (`FolderRangeParser`).
  - Persistencia de estado de último correo (`FileStateStore`).
  - Exportación CSV incremental con delimitador `|` (`CsvService`).
  - Exportación Excel incremental (Resumen + Alternativo 12 columnas) con ClosedXML (`ExcelService`).
- Logging a archivo con Serilog.

## Pruebas

```bash
dotnet test tests/ProcesadorCorreosPYS.Tests/ProcesadorCorreosPYS.Tests.csproj
```

Cobertura mínima incluida:
- dedupe/hash
- sanitización de nombres
- parser rango/lista
- persistencia de estado

## Troubleshooting

- Si compila en Linux/macOS, el proyecto WPF usa `EnableWindowsTargeting=true` para restaurar/compilar.
- Si no encuentra dependencias NuGet, reintente `dotnet restore`.
