# Cutiva frontend

Angular **22** app (`cutiva-web`).

## Run

```bash
# Terminal 1 — API (repo root)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CuttingOptimizer.Api --urls http://localhost:5216

# Terminal 2 — UI
cd frontend/cutiva-web
npm install
npm start
```

Dev server: `http://localhost:4200`.  
`npm start` proxies `/api` → `http://localhost:5216` via `proxy.conf.json` (avoids CORS).

## Structure

- `src/app/core` — language interceptor, workshop context, HTTP API clients
- `src/app/layout` — dark sidebar shell, language + workshop selectors
- `src/app/features/dashboard` — live workshop dashboard
- `src/app/features/projects` — list, create, workspace (sections + cuts)
- `src/app/features/materials` — materials + stock bars
- `src/app/features/cutting-plans` — generate + plan detail (approve)
- `src/app/features/settings` — workshops + sectors
- `public/i18n` — `en.json` / `ar.json`
- Theme tokens: `src/styles.scss`

Offline dashboard mock (opt-in): `useDashboardMock` in environments + `dashboard.mock.ts`.

Local design mockups live in repo-root `design-refs/` (gitignored).
