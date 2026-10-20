# MonsterASP API deploy (GitHub Git Deploy)

Production API: `https://cutiva.runasp.net`  
Frontend (Vercel): `https://cutiva-kappa.vercel.app`  
Repo: `https://github.com/FadiWalid21/Cutiva.git`

## Why login returns 504 / 500.30

| Symptom | Meaning | Fix |
|---------|---------|-----|
| Placeholder “Your website is ready…” | `\wwwroot` has no published API | Git deploy → **Build .NET app** |
| Browser **504** Gateway Timeout on `/api/auth/login` | Often IIS/proxy while the worker is down or restarting | Check if the site itself returns **500.30** |
| **HTTP Error 500.30** — ASP.NET Core app failed to start | Process crashed during startup (seed/DB/env) **before** `app.Run()` | Read stdout/event logs; verify `ConnectionStrings__DefaultConnection` (quote `Password='…'` if it contains `=`); **Restart**. Schema repair is best-effort and must not kill startup. |

DevTools **Source: Service Worker** on the login request is normal for the PWA; it does not cause the 504.

## Primary path: Control Panel Git Deploy

Official help: [Deploy from a GitHub repository](https://help.monsterasp.net/websites/git-deploy/github-repository)

You may **delete and recreate** the FreeSite if an old Git deploy config is messy.

### Configure

1. MonsterASP → **Websites** → your site → **Git deploy**
2. **GitHub repository URL** (HTTPS, ends with `.git`):
   `https://github.com/FadiWalid21/Cutiva.git`
3. If the repo is **private**, paste a GitHub Personal Access Token, then **Load** branches
4. **Branch:** `main` (required — do not leave an old feature branch selected)
5. **Deployment type:** **Build .NET app**
6. **Project to build** (multi-project repo):  
   `src/CuttingOptimizer.Api/CuttingOptimizer.Api.csproj`
7. Platform: site must allow **.NET 10.x** (already on FreeSite overview)
8. Click **Enable Git deploy**

Enabling does **not** change the live site until you publish.

### Publish (every API change)

1. **Pull latest** (fetches `main` into staging)
2. **Build & publish** (server runs `dotnet publish` and deploys output to `\wwwroot`)
3. Check **Recent builds** / logs if it fails
4. On the site overview: **Restart** (required — migrations + `MaterialsSchemaRepair` run on startup)

On **FreeSite**, auto-deploy on every push is usually **not** available (Premium). After each `git push` to `main`, repeat **Pull latest** → **Build & publish** → **Restart**.

Verify the new build is live:

```bash
curl -sS https://cutiva.runasp.net/api/diagnostics/build
```

### Environment variables

Set these in the MonsterASP site settings (ASP.NET Core `__` nesting):

| Variable | Notes |
|----------|--------|
| `ConnectionStrings__DefaultConnection` | Postgres. If the password contains `=`, **quote it**: `Password='yourPasswordHere'`. Full shape: `Server=…;Port=5432;Database=…;User Id=…;Password='…';Ssl Mode=Prefer;` |
| `Jwt__Key` | ≥32 random chars (`openssl rand -base64 48`) |
| `Jwt__Issuer` | `Cutiva` |
| `Jwt__Audience` | `Cutiva` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Brevo__ApiKey` | From Brevo |
| `Brevo__SenderEmail` | Verified sender |
| `Brevo__SenderName` | e.g. `CUTIVA` |
| `Features__ResourcesV2` | `true` when ready |
| `Features__ProductTemplates` | `true` when ready |

CORS already allows `https://cutiva-kappa.vercel.app` and `*.vercel.app`.

### Smoke test

1. `https://cutiva.runasp.net/` — must **not** be the placeholder page or **500.30**
2. `curl -sS https://cutiva.runasp.net/api/diagnostics/build` → **200** JSON
3. From Vercel login: `fadialhallag@gmail.com` / `Admin123!` (unless changed)
4. Network: `POST /api/auth/login` → `200` or `401`, **not** `504` / `500.30`

DB migrations run automatically on API startup via `MigrateAsync()` (IdentityDataSeeder). After each **Build & publish**, click **Restart** so pending EF migrations apply.

If `POST /api/materials` returns Arabic 500 while `GET /api/materials` works:

1. Confirm Git deploy branch is **`main`**, then Pull → Build & publish → **Restart**.
2. Hit `GET /api/diagnostics/build` — if it 404s, the old binary is still live.
3. DB migrations (including `EnsureMaterialsSchemaAligned`) are already applied on production; startup also runs idempotent `MaterialsSchemaRepair`. Current `main` creates materials successfully against this DB when the new binary is running.
4. After restart, create should return **201**. If it still fails, the JSON body `message` field carries the store error.

## Optional local fallback (not required)

If you ever need a local publish folder:

```bash
./scripts/publish-api-monsterasp.sh
```

Output is `publish/` (gitignored). Prefer Git deploy above instead of uploading this manually.

## Later: push-to-deploy without opening the panel

Premium MonsterASP auto-deploy, or GitHub Actions + WebDeploy (needs WebDeploy secrets from **Deploy → Visual Studio access**). Not configured in this repo yet.
