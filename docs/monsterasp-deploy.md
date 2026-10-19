# MonsterASP API deploy checklist

Production API host: `https://cutiva.runasp.net`

## Current status

If the homepage still shows MonsterASP’s “Your website is ready…” page, the API is **not** published yet. Login from Vercel will fail with **504 Gateway Timeout** until the API is deployed.

## Deploy

1. Publish `CuttingOptimizer.Api` to the site bound to `cutiva.runasp.net` (GitHub Actions / MonsterASP Git deploy / FTP publish).
2. After deploy, opening `https://cutiva.runasp.net/` must **not** be the placeholder page.
3. Enable HTTPS on the site.

## Environment variables

Set these on MonsterASP (ASP.NET Core `__` nesting):

| Variable | Notes |
|----------|--------|
| `ConnectionStrings__DefaultConnection` | Postgres string. If the password contains `=`, quote it: `Password='…'` |
| `Jwt__Key` | ≥32 random chars (`openssl rand -base64 48`) |
| `Jwt__Issuer` | `Cutiva` |
| `Jwt__Audience` | `Cutiva` |
| `Brevo__ApiKey` | From Brevo |
| `Brevo__SenderEmail` | Verified sender |
| `Brevo__SenderName` | e.g. `CUTIVA` |
| `Features__ResourcesV2` | `true` when ready |
| `Features__ProductTemplates` | `true` when ready |

CORS already allows `https://cutiva-kappa.vercel.app` and `*.vercel.app`.

## Smoke test

1. `https://cutiva.runasp.net/` — app response (not placeholder).
2. From Vercel login: `fadialhallag@gmail.com` / seeded admin password (`Admin123!` unless changed).

DB migrations and admin seed were applied earlier against the MonsterASP Postgres database.
