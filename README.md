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
| Deploy   | Docker Compose (backend, frontend/nginx, cloudflared), Cloudflare Tunnel |

Details in [`docs/STACK.md`](docs/STACK.md).

## Prerequisites

- .NET SDK 10
- Node.js 20+ (tested on 24)
- Docker (for the SQL Server container and the production stack)

## Getting started

```bash
# 1. Start SQL Server (dev database only)
docker compose up -d sqlserver

# 2. Backend — creates/migrates the database and seeds songs on startup (Development)
cd backend
dotnet run
# → http://localhost:5233

# 3. Frontend — in a second terminal
cd frontend
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
origin must be added to `Cors:AllowedOrigins` in `backend/appsettings.Development.json`.
If other devices cannot connect at all, allow `node` and `dotnet` through the
Windows firewall for private networks.

Score files are read from `storage/` at the repo root (gitignored — drop your own
Guitar Pro files there; the seeded *Stairway to Heaven* expects
`storage/led-zeppelin-stairway_to_heaven.gp4`).

## Scripts

```bash
dotnet test GLA.slnx      # backend unit tests
npm run build             # frontend type-check + production build (frontend/)
npm run type-check        # frontend type-check only (frontend/)
```

## API

| Method | Route              | Description                                   |
| ------ | ------------------ | --------------------------------------------- |
| GET    | `/songs`           | All songs                                     |
| GET    | `/songs/{id}`      | One song, `404` if missing                    |
| GET    | `/songs/{id}/file` | Score file bytes, `404` if missing            |
| POST   | `/songs`           | Upload a score file (multipart, one file)     |
| DELETE | `/songs/{id}`      | Delete the song, then its file (best effort)  |

Response:

```json
{ "id": 1, "title": "Stairway to Heaven", "author": "Led Zeppelin", "filePath": "led-zeppelin-stairway_to_heaven.gp4" }
```

`filePath` is relative to `storage/` (forward slashes, no leading slash); the file
endpoint streams it to the client, where alphaTab renders it in the score viewport.

## Deployment

The compose file also defines the production stack: `backend` (ASP.NET Core),
`frontend` (nginx serving the built SPA) and `cloudflared` (Cloudflare Tunnel).
All images are multi-arch and pinned to `linux/arm64` — the deployment target.
Next to each one there is a commented `platform: linux/amd64` line to flip to
for dev testing on x86 hosts.

```bash
# 1. Configure secrets/settings — .env is gitignored, .env.example is the template
cp .env.example .env
#    → set DB_PASSWORD, CF_TUNNEL_TOKEN, WEB_PORT, …

# 2. Build and start everything
docker compose up -d --build

# 3. Smoke test through nginx
curl http://localhost/api/songs    # nginx → backend → SQL Server
open http://localhost/             # the SPA (deep links like /songs/1 included)
```

- **nginx** serves `frontend/dist` with SPA fallback (`/songs/1` loads
  `index.html`) and proxies `/api/*` to the backend with the prefix stripped —
  the same contract as the Vite dev proxy — so the app stays same-origin and
  CORS never comes into play.
- **cloudflared** runs `tunnel --no-autoupdate run --token $CF_TUNNEL_TOKEN`;
  point the tunnel's public hostname at `http://frontend:80` in the Cloudflare
  dashboard (Zero Trust → Tunnels).
- **Database**: on startup the backend runs pending EF migrations (the seed
  songs are part of the migrations, so a fresh database starts populated). It
  talks to the `sqlserver` compose service by default — set `DB_HOST`/`DB_PORT`
  in `.env` to use an external database instead. Note that
  `mcr.microsoft.com/mssql/server` ships **amd64 only**, so the SQL Server
  container runs under emulation on an ARM64 host; an external DB avoids that.
- **Score files** live in `./storage`, bind-mounted into the backend container.

## Project layout

```
├── backend/             ASP.NET Core backend
│   ├── Data/            DbContext + migrations
│   ├── Dtos/            API contracts
│   ├── Endpoints/       Route mappings
│   ├── Entities/        EF entities
│   ├── Repositories/    Data access
│   └── Services/        Application logic
├── GLA.Tests/           xUnit tests (SQLite in-memory)
├── frontend/            Vue 3 frontend (Vite + Tailwind)
│   ├── nginx.conf       Production static server + /api proxy
│   └── src/
│       ├── components/  ScoreViewport, PlayerControls, SongUploadDialog
│       ├── services/    songApi
│       ├── stores/      Pinia song store
│       ├── types/       API models
│       └── views/       SongsView, SongView
├── docs/                Product & feature specs
├── storage/             Score files (gitignored)
├── .env.example         Deployment settings template (copy to .env)
└── docker-compose.yml   SQL Server + production stack (backend/frontend/cloudflared)
```

## Development notes

- **Migrations**: `dotnet tool restore`, then
  `dotnet ef migrations add <Name> --project backend --startup-project backend`
- **Seed data**: 5 songs are seeded — `ReSeedSongs` (ids 1–4) and
  `AddGnossienneSeed` (id 5, Satie's *Gnossienne No. 1*)
- **Connection string**: `backend/appsettings.Development.json` (dev-only SA
  password, mirrored as `DB_PASSWORD` in `.env` for the containers)
- **alphaTab assets**: Bravura fonts and the SONiVOX soundfont are copied from
  `node_modules` into `frontend/public/` by the Vite plugin on dev/build (gitignored).
  On a *fresh clone* the dev server can start before `frontend/public/` exists
  (the copy runs in the plugin's `buildStart`, after Vite mounted its static
  middleware) — alphaTab then logs font/soundfont loading errors because the
  assets come back as `index.html`. Restart `npm run dev` once the directory
  exists (or run `npm run build` first).
- **Next up**: Phase v2 (score track controls + songs list page), see [`docs/PHASES.md`](docs/PHASES.md)
