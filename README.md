# Codex Club • Where Code Becomes Science

This solution now consists of an ASP.NET Core 8 REST API (controllers + Swagger + SignalR hubs) and a modern Next.js 16
frontend (Tailwind 4, SWR, Zustand, SignalR, ONNX Runtime Web). Разом вони формують повноцінну "лабораторію"
для керівника клубу програмування:

- **Algorithm Theatre** — SignalR streaming of server-side algorithm simulations drawn on a canvas.
- **Machine-Learning Lab** — ONNX Runtime Web runs in the browser while ML.NET serves predictions from the API.
- **GPU Simulator** — WebGPU-powered reaction-diffusion sandbox with live parameter sliders.
- **Compiler Playground** — Lightweight code editor wired to Roslyn scripting for AST and diagnostics.
- **Codex Dispatch** — WordPress-style news feed з Markdown та формою публікації для адміністраторів.
- **Club Mode** — SignalR-клуб для синхронного редагування коду, кешований через Redis та EF Core.
- **Multilingual UI** — Українська за замовчуванням, англійська як додаткова мова.

## Project structure

```
frontend/     Next.js 16 + Tailwind 4 app (SWR, SignalR, ONNX Runtime Web)
src/
  Server/     ASP.NET Core 8 REST API + SignalR hubs + ML.NET + EF Core + Redis
  Shared/     DTOs & enums reused by the API
```

## Local development

1. **Prerequisites**: .NET 8 SDK, Node.js ≥ 20, PostgreSQL, Redis.
2. Restore server dependencies & generate swagger types if needed:
   ```bash
   dotnet restore
   ```
3. Install frontend dependencies:
   ```bash
   cd frontend
   npm install
   cd ..
   ```
4. Apply database migrations (Identity + News):
   ```bash
   dotnet tool install -g dotnet-ef   # once
   dotnet ef database update --project src/Server/NetAppForVika.Server.csproj --startup-project src/Server/NetAppForVika.Server.csproj
   ```
5. Launch the dev stack with Docker (recommended):
   ```bash
   docker compose -f docker-compose.dev.yml up --build
   ```
   This exposes the API on `http://localhost:5050` (Swagger at `/swagger`) and Next.js on `http://localhost:3000`.
6. Or run manually:
   ```bash
   dotnet watch run --project src/Server/NetAppForVika.Server.csproj --urls http://localhost:5050
   cd frontend && npm run dev
   ```

## Docker

A production-ready container is supplied via the root `Dockerfile`. To build and launch the whole stack:

```bash
docker compose up --build
```

The compose file provisions the ASP.NET Core API, Next.js frontend, PostgreSQL (EF Core persistence) and Redis (SignalR + cache).

## Tailwind workflow

The Next.js app uses Tailwind CSS 4. Utility classes live directly in `frontend/src/app/globals.css`. During dev `npm run dev`
automatically applies changes; production builds use `npm run build`. No separate Tailwind container is needed anymore.

## ML models

Place the trained ML.NET model (`digit-mnist.zip`) under `src/Server/Resources` and the ONNX model (`digit.onnx`)
under `src/Server/Resources` (served by the API at `/Resources`). Both paths are configurable via `MachineLearning` options.

## Identity & admin access

- Accounts are managed via ASP.NET Core Identity with JWT authentication.
- Default seeded admin credentials (change in `appsettings`):
  - Email: `admin@codex.club`
  - Password: `Admin!234`
- Admins can publish new articles via `/admin/news`. Members can register via `/login`.

## Testing notes

The solution emphasises streaming demos and GPU/WebAssembly experiences. Automated tests are not included, but
services are structured for dependency injection to enable unit tests (e.g., mocking `IAlgorithmVisualizer` or
`IDigitPredictionService`).

## Docker Compose profiles

- `docker-compose.yml`: production-style build with precompiled Tailwind and published server image.
- `docker-compose.dev.yml`: development stack with `dotnet watch` and Tailwind watcher.

Run the dev workflow with:
```bash
docker compose -f docker-compose.dev.yml up --build
```
The development server listens on `http://localhost:5050`.
