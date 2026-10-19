# MonsterASP API deploy checklist

Production API host: `https://cutiva.runasp.net`  
Site: FreeSite / IIS (e.g. `site100135`) — website root `\wwwroot`

## Why login returns 504

If the site preview still shows **“Your website is ready…”**, `\wwwroot` has **no** published API. Vercel calls `https://cutiva.runasp.net/api/auth/login` and IIS returns **504 Gateway Timeout**.

DevTools **Source: Service Worker** is normal for the Angular PWA; it does **not** cause this 504.

## 1. Publish locally

From the repo root:

```bash
chmod +x scripts/publish-api-monsterasp.sh
./scripts/publish-api-monsterasp.sh
```

Output folder: `publish/api/` (includes `web.config` + DLLs). Do **not** commit this folder.

## 2. Upload via WebFTP

1. Open [WebFTP](https://webftp.monsterasp.net) (or FTP/SFTP).
2. Host example: `site100135.siteasp.net` — login from MonsterASP **FTP / SFTP** panel.
3. Go to `\wwwroot`.
4. Delete placeholder / default files if present.
5. Upload **all files and folders** from `publish/api/` into `\wwwroot`  
   (including `web.config`, `CuttingOptimizer.Api.dll`, `web.config`, deps, runtimes, etc.).
6. Confirm `web.config` is in `\wwwroot` (not only in a subfolder).

## 3. Environment variables

In MonsterASP site settings (ASP.NET Core `__` nesting):

| Variable | Notes |
|----------|--------|
| `ConnectionStrings__DefaultConnection` | Postgres. If password contains `=`, use `Password='…'` |
| `Jwt__Key` | ≥32 random chars (`openssl rand -base64 48`) |
| `Jwt__Issuer` | `Cutiva` |
| `Jwt__Audience` | `Cutiva` |
| `Brevo__ApiKey` | From Brevo |
| `Brevo__SenderEmail` | Verified sender |
| `Brevo__SenderName` | e.g. `CUTIVA` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Features__ResourcesV2` | `true` when ready |
| `Features__ProductTemplates` | `true` when ready |

CORS already allows `https://cutiva-kappa.vercel.app` and `*.vercel.app`.

## 4. Restart

On the site overview: **Restart** (or Stop then Start).

## 5. Smoke test

1. `https://cutiva.runasp.net/` — must **not** be the MonsterASP placeholder.
2. From https://cutiva-kappa.vercel.app login: `fadialhallag@gmail.com` / `Admin123!` (unless you changed it).
3. Network: `/api/auth/login` → `200` (or `401` for wrong password), **not** `504`.

DB migrations and admin seed were applied earlier against the MonsterASP Postgres database.
