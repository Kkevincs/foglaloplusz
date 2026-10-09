# FoglalóPlusz

Salon-based **appointment booking** app: guests, professionals, and owners can register, sign in, and manage services, working hours, and bookings from the browser.

## How it works

- **Backend:** ASP.NET Core 8 **REST API**, **JWT** Bearer authentication, **Entity Framework Core** + **PostgreSQL**, with **migrations** applied on startup (production / PostgreSQL). Static uploads (for example salon logos) are stored under `wwwroot/uploads`.
- **Frontend:** **Node.js** + **Express** serves the HTML/JS/CSS views and **proxies** `/api` and `/uploads` requests to the .NET API, so the browser talks to a single port (default: **3000**).
- **Layers:** `Foglaloplusz` (presentation), `Foglaloplusz.BLL`, `Foglaloplusz.DAL`, tests: `Foglaloplusz.Tests`.

## Author

**Sátori Kázmér Kevin** — backend and API, authentication (JWT), Express proxy, frontend integration, Docker and production setup, data model, migrations, salon and owner features, themes and forms, and UI.

## Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS recommended)
- **PostgreSQL** (local or remote)
- Optional: Docker (for the backend container)

## Run locally

1. **Database:** set `ConnectionStrings:DefaultConnection` in `Foglaloplusz/appsettings.Development.json` (or via environment variables) to your PostgreSQL instance. Do not commit passwords — production secrets should come from environment variables.

2. **API** (from the repo root):

   ```bash
   dotnet run --project Foglaloplusz/Foglaloplusz.presentation.csproj
   ```

   Default HTTP profile: **http://localhost:5181** — **Swagger:** `/swagger`.

3. **Browser UI** (separate terminal):

   ```bash
   cd Foglaloplusz/Frontend/express-server
   npm install
   npm start
   ```

   Open: **http://localhost:3000** (or the port set by the `PORT` environment variable).

4. **Proxy target:** if the API is not on `127.0.0.1:5181`, set:

   ```bash
   set SALONIC_API_PROXY_TARGET=http://127.0.0.1:5181
   npm start
   ```

   (Linux/macOS: `export SALONIC_API_PROXY_TARGET=...`)

## Tests

```bash
dotnet test Foglaloplusz.Tests/Foglaloplusz.Tests.csproj
```

## Docker (backend only)

The `Dockerfile` at the repo root publishes the .NET API (for example on **8080**). PostgreSQL must be provided separately; pass the connection string at runtime (environment variable or `appsettings`).

## Documentation

- Detailed descriptions, UML, and written materials: **`docs/`** (Markdown, PlantUML).
- API exploration: **Swagger UI** (opens by default in the development profile).

## License / status

University / exam-project style codebase; no separate license is specified.
