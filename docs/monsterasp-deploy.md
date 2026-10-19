# MonsterASP API deploy (GitHub Git Deploy)

Production API: `https://cutiva.runasp.net`  
Frontend (Vercel): `https://cutiva-kappa.vercel.app`  
Repo: `https://github.com/FadiWalid21/Cutiva.git`

## Why login returns 504

If the site still shows MonsterASP’s **“Your website is ready…”** page, `\wwwroot` has no published API. Fix that with **Git deploy → Build .NET app** (no FTP upload).

DevTools **Source: Service Worker** on the login request is normal for the PWA; it does not cause the 504.

## Primary path: Control Panel Git Deploy

Official help: [Deploy from a GitHub repository](https://help.monsterasp.net/websites/git-deploy/github-repository)

You may **delete and recreate** the FreeSite if an old Git deploy config is messy.

### Configure

1. MonsterASP → **Websites** → your site → **Git deploy**
2. **GitHub repository URL** (HTTPS, ends with `.git`):
   `https://github.com/FadiWalid21/Cutiva.git`
3. If the repo is **private**, paste a GitHub Personal Access Token, then **Load** branches
4. **Branch:** `feature/resources-products-costing`  
   (switch to `main` after you merge this work)
5. **Deployment type:** **Build .NET app**
6. **Project to build** (multi-project repo):  
   `src/CuttingOptimizer.Api/CuttingOptimizer.Api.csproj`
7. Platform: site must allow **.NET 10.x** (already on FreeSite overview)
8. Click **Enable Git deploy**

Enabling does **not** change the live site until you publish.

### Publish

1. **Pull latest** (fetches the branch into staging)
2. **Build & publish** (server runs `dotnet publish` and deploys output to `\wwwroot`)
3. Check **Recent builds** / logs if it fails
4. On the site overview: **Restart**

On **FreeSite**, auto-deploy on every push is usually **not** available (Premium). After each `git push`, repeat **Pull latest** → **Build & publish**.

### Environment variables

Set these in the MonsterASP site settings (ASP.NET Core `__` nesting):

| Variable | Notes |
|----------|--------|
| `ConnectionStrings__DefaultConnection` | Postgres. If password contains `=`, use `Password='…'` |
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

1. `https://cutiva.runasp.net/` — must **not** be the placeholder page
2. From Vercel login: `fadialhallag@gmail.com` / `Admin123!` (unless changed)
3. Network: `POST /api/auth/login` → `200` or `401`, **not** `504`

DB migrations and admin seed were applied earlier against the MonsterASP Postgres database.

## Optional local fallback (not required)

If you ever need a local publish folder:

```bash
./scripts/publish-api-monsterasp.sh
```

Output is `publish/` (gitignored). Prefer Git deploy above instead of uploading this manually.

## Later: push-to-deploy without opening the panel

Premium MonsterASP auto-deploy, or GitHub Actions + WebDeploy (needs WebDeploy secrets from **Deploy → Visual Studio access**). Not configured in this repo yet.
