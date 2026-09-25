# GLA — Guitar Learning App

Learning guitar songs should be fun and structured. Upload a tab/score file and get
difficulty analysis per fragment, detected techniques, a recommended learning order
and a focused practice mode.

This repository currently contains **Phase v1**: the song page with its backend
infrastructure. See [`docs/SONG_PAGE.md`](docs/SONG_PAGE.md) for the feature spec and
[`docs/GLA.md`](docs/GLA.md) for the product vision.

## Stack

| Layer    | Technology |
| -------- | ---------- |
| Backend  | ASP.NET Core (.NET 10), EF Core, SQL Server (Docker), System.Text.Json |
| Frontend | Vue 3, Vite, TypeScript, Tailwind CSS 4, Vue Router, Pinia |
| Tests    | xUnit (backend, SQLite in-memory) |

Details in [`docs/STACK.md`](docs/STACK.md).

## Prerequisites

- .NET SDK 10
- Node.js 20+ (tested on 24)
- Docker (for the SQL Server container)

## Getting started

```bash
# 1. Start SQL Server
docker compose up -d

# 2. Backend — creates/migrates the database and seeds songs on startup (Development)
cd GLA
dotnet run
# → http://localhost:5233

# 3. Frontend — in a second terminal
cd client
npm install
npm run dev
# → http://localhost:5173  (opens on /songs/1)
```

The Vite dev server proxies `/api/*` to the backend, so no CORS setup is needed in
development (the CORS policy is registered for direct/production access).

## Scripts

```bash
dotnet test GLA.slnx      # backend unit tests
npm run build             # client type-check + production build (client/)
npm run type-check        # client type-check only (client/)
```

## API

| Method | Route          | Description                                   |
| ------ | -------------- | --------------------------------------------- |
| GET    | `/songs`       | All songs                                     |
| GET    | `/songs/{id}`  | One song, `404` if missing                    |

Response:

```json
{ "id": 1, "title": "Paranoid Android", "author": "Radiohead", "filePath": "/ParanoidAndroid.gp5" }
```

`filePath` points to the score file's location; serving those files is not part of
Phase v1.

## Project layout

```
├── GLA/                 ASP.NET Core backend
│   ├── Data/            DbContext + migrations
│   ├── Dtos/            API contracts
│   ├── Endpoints/       Route mappings
│   ├── Entities/        EF entities
│   ├── Repositories/    Data access
│   └── Services/        Application logic
├── GLA.Tests/           xUnit tests (SQLite in-memory)
├── client/              Vue 3 frontend (Vite + Tailwind)
│   └── src/
│       ├── components/  ScoreViewport, PlayerControls
│       ├── services/    songApi
│       ├── stores/      Pinia song store
│       ├── types/       API models
│       └── views/       SongView
├── docs/                Product & feature specs
└── docker-compose.yml   SQL Server
```

## Development notes

- **Migrations**: `dotnet tool restore`, then
  `dotnet ef migrations add <Name> --project GLA --startup-project GLA`
- **Seed data**: 3 songs are inserted by the `InitialCreate` migration
- **Connection string**: `GLA/appsettings.Development.json` (dev-only SA password,
  mirrored in `docker-compose.yml`)
- **Next up**: replacing the viewport placeholder with real score rendering (alphaTab),
  then Phase v2 (users)
