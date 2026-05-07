# AGENTS.md

## Build & run

```bash
dotnet restore
dotnet build                          # warnings as errors
dotnet run --project src/desktop_app  # launch the tray app
```

## Format, lint, test

```bash
dotnet format --verify-no-changes     # check formatting (CI gate)
dotnet test                           # xUnit — includes headless Avalonia UI tests
```

CI order (ci.yml): `restore → format check → build --no-restore → test --no-build`

## Single-project structure

The solution uses the new `.slnx` format. There is one app + one test project:

- `src/desktop_app` — Avalonia 11 desktop app (WinExe, net10.0)
- `tests/desktop_app.tests` — xUnit tests using NSubstitute + Avalonia.Headless.XUnit

## Key architecture

- **DI pattern** without a DI container — services are manually constructed in `App.axaml.cs:OnFrameworkInitializationCompleted` and passed to constructors.
- **`NowPlayingService`** accepts an optional `TimeProvider?` parameter (defaults to `TimeProvider.System`). Tests inject `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` for deterministic polling checks.
- **ViewModel** uses `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`). WPF-style `OnXxxChanged` partial methods auto-save settings and restart services.
- **ViewLocator** maps `FooViewModel` → `FooView` by convention.

## Testing

- `[Fact]` for unit tests, `[AvaloniaFact]` for headless UI tests (requires Avalonia to be initialized via `TestAppBuilder` / `TestApp.axaml`).
- Use `NSubstitute` for mocking, `FakeTimeProvider` for time-dependent tests.
- Suppress code analysis rules that clash with test naming: CA1707 (underscores in names), CA1816 (Dispose).

## Code conventions (enforced by .editorconfig + analyzers)

| Item | Rule |
|------|------|
| Indent | 4 spaces (C#), 2 spaces (XML/AXAML/JSON/MD) |
| Line endings | CRLF only |
| `using` placement | Outside namespace |
| Namespace style | File-scoped |
| Private instance fields | `_camelCase` |
| Private static fields | `PascalCase` |
| `var` | Always prefer for built-in/apparent types |
| Warnings-as-errors | Yes (TreatWarningsAsErrors) |
| Suppressed rules | CA1848 (LoggerMessage delegates), CS1591 (XML doc missing), CA1707+CA1816 (tests only) |

## Settings & store

- Settings JSON: `%APPDATA%/PlayStatus/settings.json`
- MSAL token cache: `%APPDATA%/PlayStatus/msal_cache.bin`
- Serialized with `JsonNamingPolicy.CamelCase`
- `SettingsService` provides static `GetMsalCachePath()` for MSAL integration

## Publish (CI release)

```bash
dotnet publish src/desktop_app/desktop_app.csproj \
  --configuration Release --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  --runtime <rid> --output ./publish
```

## Product site

The `site/` directory is an Astro 5 static site (landing page + docs) deployed to GitHub Pages via `.github/workflows/deploy-pages.yml`.

```bash
cd site
npm install
npm run dev     # dev server at localhost:4321
npm run build   # production build to site/dist/
```
