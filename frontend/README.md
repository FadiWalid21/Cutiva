# Cutiva frontend

Angular **22** app (`cutiva-web`).

## Run

```bash
cd frontend/cutiva-web
npm install
npm start
```

Dev server defaults to `http://localhost:4200` and proxies `/api` → `http://localhost:5216` (`proxy.conf.json`). Run the API locally first.

## Structure

- `src/app/core` — language service, HTTP interceptor (`Accept-Language`), workshops API client, workshop context
- `src/app/layout` — dark sidebar shell, language toggle, workshop selector
- `src/app/features/dashboard` — live workshop dashboard (KPIs, projected leftovers, recent projects)
- `src/app/features/projects` — create-project form stub
- `public/i18n` — `en.json` / `ar.json`
- Theme tokens: `src/styles.scss` (Inter + Cutiva dark palette)
- Offline mock (opt-in): `useDashboardMock` in environments + `dashboard.mock.ts`

Local design mockups live in repo-root `design-refs/` (gitignored).
