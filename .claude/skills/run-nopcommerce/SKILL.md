---
name: run-nopcommerce
description: Build and run this nopCommerce store locally against the local SQL Server (server `.`, database `TmTm`), including first-time installation if the database is empty. Use when asked to run, start, launch, restart, or smoke-test the app locally, or to reinstall/reset the local store database.
---

# Run nopCommerce locally

Local setup that is already known-good on this machine — don't re-derive it.

| | |
|---|---|
| Repo root | `D:\Work\TmTm\Code` |
| App project | `src/Presentation/Nop.Web` |
| URL | `http://nomo.local:5000` — **always**, never `localhost` (no `launchSettings.json`; always pass `--urls`) |
| DB | SQL Server 2025 at `.`, database `TmTm`, `sa` / `asdASD@1234` |
| Admin login | `admin@yourstore.com` / `asdASD@1234` |
| Config written by installer | `src/Presentation/Nop.Web/App_Data/appsettings.json` (gitignored) |

Branch `TmTm_release_4_90_6` targets **net10.0** (`global.json` pins SDK 10.0.100, `rollForward: latestFeature`; 10.0.400 is installed).

## Already running? Just confirm it

Check before building or starting anything — a rebuild/restart costs ~3 minutes
and drops whatever the user has open in the browser:

```bash
UA='Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36'
curl -s -A "$UA" -o /dev/null -w 'store=%{http_code}\n' --max-time 5 http://nomo.local:5000/
```

`200` means it is up: say so and stop. Do **not** rebuild, restart, or start a
second instance. Only restart when the user asks for one or when you changed
C# / `.cshtml` (CSS and static files need no restart — WebOptimizer re-reads
them and its `?v=` hash busts the browser cache on its own).

If it answers but you need to know whether it is *your* build, check the
process start time against your last build:

```powershell
Get-Process Nop.Web | Select-Object Id, StartTime
```

## Fast path — app installed but not running

`App_Data/appsettings.json` exists and `TmTm` has ~132 tables:

```bash
dotnet build src/NopCommerce.sln -c Debug -v minimal        # skip if nothing changed
dotnet run --project src/Presentation/Nop.Web --no-build --urls http://nomo.local:5000
```

Run it as a **background** task, then poll until it answers — first request after a build is slow (Razor compilation):

```bash
for i in $(seq 1 60); do
  code=$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://nomo.local:5000/ 2>/dev/null)
  [ "$code" = "200" ] && break
done
curl -s http://nomo.local:5000/ | grep -oE '<title>[^<]*</title>'   # expect "Your store. Home page title"
```

`/Admin` correctly answers `200` at `/login?returnUrl=%2FAdmin` when signed out — that is success, not a failure.

Stop it with TaskStop on the background task id — **then confirm the process
actually died**. TaskStop kills the `dotnet run` wrapper, but `Nop.Web.exe`
survives and keeps holding port 5000, so the next start dies with a Kestrel
bind error (`exit code 127`) while the old build keeps serving:

```powershell
Get-Process Nop.Web -ErrorAction SilentlyContinue | Stop-Process -Force
``` A code change needs a full stop → build → start; there is no hot reload in this setup.

## First-time install (empty or missing `TmTm`)

The app redirects everything to `/install` until `App_Data/appsettings.json` has a connection string. Drive the wizard over HTTP rather than asking the user to click through a browser.

Two traps, both already hit and solved:

1. **Antiforgery is validated globally.** A bare POST returns 400, and nopCommerce's bad-request handler then throws `The ConnectionString property has not been initialized` — a misleading error that is really "your POST was rejected". Always GET `/install` first, keep the cookie jar, and send `__RequestVerificationToken`.
2. **Let the installer create the database** (`CreateDatabaseIfNotExists=true`). No need to `CREATE DATABASE` by hand. `BuildConnectionString` sets `TrustServerCertificate=true`, so there is no TLS problem with a local SQL Server.

```bash
cd /tmp && rm -f nopcookies.txt
curl -s -c nopcookies.txt -o install-page.html http://nomo.local:5000/install
TOKEN=$(grep -oE 'name="__RequestVerificationToken"[^>]*value="[^"]+"' install-page.html \
        | head -1 | sed 's/.*value="//;s/"$//')

curl -s -b nopcookies.txt -o install-result.html -w 'status=%{http_code} time=%{time_total}s\n' \
  --max-time 900 -X POST http://nomo.local:5000/install \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  --data-urlencode "AdminEmail=admin@yourstore.com" \
  --data-urlencode "AdminPassword=asdASD@1234" \
  --data-urlencode "ConfirmPassword=asdASD@1234" \
  --data-urlencode "DataProvider=1" \
  --data-urlencode "ConnectionStringRaw=false" \
  --data-urlencode "ServerName=." \
  --data-urlencode "DatabaseName=TmTm" \
  --data-urlencode "IntegratedSecurity=false" \
  --data-urlencode "Username=sa" \
  --data-urlencode "Password=asdASD@1234" \
  --data-urlencode "CreateDatabaseIfNotExists=true" \
  --data-urlencode "UseCustomCollation=false" \
  --data-urlencode "InstallSampleData=true" \
  --data-urlencode "SubscribeNewsletters=false"
```

`DataProvider=1` is `DataProviderType.SqlServer`. Takes ~30s with sample data. Drop `InstallSampleData` to `false` for an empty catalog.

**A 200 here does not prove success** — a validation failure also renders 200. Verify against the database, then restart the app (the running process still believes it is uninstalled):

```bash
sqlcmd -S . -U sa -P 'asdASD@1234' -C -d TmTm \
  -Q "SELECT COUNT(*) AS Tables FROM sys.tables; SELECT COUNT(*) AS Products FROM Product;"
# expect ~132 tables, 47 products with sample data
```

## Reset the store

```bash
sqlcmd -S . -U sa -P 'asdASD@1234' -C -Q "ALTER DATABASE TmTm SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TmTm;"
rm -f src/Presentation/Nop.Web/App_Data/appsettings.json \
      src/Presentation/Nop.Web/App_Data/plugins.json
```

Then start the app and re-run the install block. Stop the app before dropping the database, or the drop blocks on open connections.

## Notes

- **Always send a browser User-Agent with curl.** nopCommerce classifies an
  unknown UA as a search engine and hands the request the built-in search-engine
  customer, so the auth cookie is issued at login and then ignored on every
  later request — `/Admin` and `/customer/info` bounce to `/login` forever. This
  looks exactly like broken authentication and is not.
- **Never use `localhost:5000`.** `nomo.local` is the store URL in the `Store`
  table, and NopStation plugin licenses are domain-bound — their widgets
  (product ribbons, carousels, …) silently render **nothing** on `localhost`.

- Client-side libs live in committed `wwwroot/lib_npm`, so **no `npm install` / gulp is needed** to run. Only run gulp if those assets are actually missing or you changed `package.json`.
- Building `NopCommerce.sln` also builds every plugin straight into `Nop.Web/Plugins/` (~70s cold). Building only `Nop.Web.csproj` is faster but leaves plugin DLLs stale.
- `App_Data/appsettings.json`, `App_Data/plugins.json`, and `Nop.Web/Plugins/*` are gitignored build/install artifacts — never commit them.
