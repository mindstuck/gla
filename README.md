# GLA — Guitar Learning App

Learning guitar songs should be fun and structured. Upload a tab/score file and get
difficulty analysis per fragment, detected techniques, a recommended learning order
and a focused practice mode.

This repository currently contains **Phase v1**: the song page — score rendering and
playback via alphaTab, with its backend infrastructure. See
[`docs/PHASES.md`](docs/PHASES.md) for the phase plan and
[`docs/GLA.md`](docs/GLA.md) for the product vision.

## Stack

| Layer    | Technology |
| -------- | ---------- |
| Backend  | ASP.NET Core (.NET 10), EF Core, SQL Server (Docker), System.Text.Json |
| Frontend | Vue 3, Vite, TypeScript, Tailwind CSS 4, Vue Router, Pinia, alphaTab |
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

Both dev servers bind to all interfaces, so from any device on your LAN you can
open the app with your laptop's IP instead of `localhost` (e.g.
`http://192.168.1.23:5173` — find the address with `ipconfig`). The Vite dev
server proxies `/api/*` to the backend, so no CORS setup is needed in
development (the CORS policy is registered for direct/production access) —
except if a client hits `http://<lan-ip>:5233` directly, in which case its
origin must be added to `Cors:AllowedOrigins` in `GLA/appsettings.Development.json`.
If other devices cannot connect at all, allow `node` and `dotnet` through the
Windows firewall for private networks.

Score files are read from `storage/` at the repo root (gitignored — drop your own
Guitar Pro files there; the seeded *Stairway to Heaven* expects
`storage/led-zeppelin-stairway_to_heaven.gp4`).

## Scripts

```bash
dotnet test GLA.slnx      # backend unit tests
npm run build             # client type-check + production build (client/)
npm run type-check        # client type-check only (client/)
```

## API

| Method | Route              | Description                                   |
| ------ | ------------------ | --------------------------------------------- |
| GET    | `/songs`           | All songs                                     |
| GET    | `/songs/{id}`      | One song, `404` if missing                    |
| GET    | `/songs/{id}/file` | Score file bytes, `404` if missing            |

Response:

```json
{ "id": 1, "title": "Stairway to Heaven", "author": "Led Zeppelin", "filePath": "led-zeppelin-stairway_to_heaven.gp4" }
```

`filePath` is relative to `storage/` (forward slashes, no leading slash); the file
endpoint streams it to the client, where alphaTab renders it in the score viewport.

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
├── storage/             Score files (gitignored)
└── docker-compose.yml   SQL Server
```

## Development notes

- **Migrations**: `dotnet tool restore`, then
  `dotnet ef migrations add <Name> --project GLA --startup-project GLA`
- **Seed data**: 5 songs are seeded — `ReSeedSongs` (ids 1–4) and
  `AddGnossienneSeed` (id 5, Satie's *Gnossienne No. 1*)
- **Connection string**: `GLA/appsettings.Development.json` (dev-only SA password,
  mirrored in `docker-compose.yml`)
- **alphaTab assets**: Bravura fonts and the SONiVOX soundfont are copied from
  `node_modules` into `client/public/` by the Vite plugin on dev/build (gitignored).
  On a *fresh clone* the dev server can start before `client/public/` exists
  (the copy runs in the plugin's `buildStart`, after Vite mounted its static
  middleware) — alphaTab then logs font/soundfont loading errors because the
  assets come back as `index.html`. Restart `npm run dev` once the directory
  exists (or run `npm run build` first).
- **Next up**: Phase v2 (score track controls + songs list page), see [`docs/PHASES.md`](docs/PHASES.md)
