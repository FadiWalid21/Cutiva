# Cutiva frontend

Angular **22** app (`cutiva-web`).

## Run

```bash
cd frontend/cutiva-web
npm install
npm start
```

Dev server defaults to `http://localhost:4200`. API base URL is `http://localhost:5216` (see `src/environments`).

## Structure

- `src/app/core` — language service, HTTP interceptor (`Accept-Language`)
- `src/app/layout` — shell layout + language toggle
- `src/app/features` — feature areas (home + placeholders)
- `src/app/shared` — shared UI (empty for now)
- `public/i18n` — `en.json` / `ar.json`

Brand colors and fonts are placeholders in `src/styles.scss` until provided.
