DevPulse — Engineering Activity Dashboard

A compact, production‑minded full‑stack project that aggregates engineering signals (GitHub PRs & Issues, Jira tickets, CI hints, Weather, Slack reminders). Built for portfolio/demo use with real integrations, OAuth/JWT auth, background sync, and webhooks.

Stack: .NET 10 Preview (ASP.NET Core Web API) · EF Core 9 · PostgreSQL · Quartz Hosted Jobs · React 18 (Vite + TS) · React Query · Tailwind + shadcn/ui · Docker Compose · GitHub Actions · Infisical/Vault for secrets. I would ideally use Azure KeyVault but who's going to pay for it?! :-D

⸻
DevPulse Backend (API + Infrastructure)

Backend for DevPulse, built with .NET 10 (preview), Entity Framework Core, and SQL Server 2025.
It provides authentication, dashboards, provider connections (GitHub/Jira), and webhook endpoints.

⸻

Prerequisites
	•	.NET 10 SDK (preview)
	•	Docker
	•	Infisical CLI (for local secret injection)
	•	Optional: Azure Data Studio or sqlcmd to inspect SQL Server

⸻

Project Structure

devpulse/api/
├── DevPulse.Api/              # API host (controllers, Program.cs)
├── DevPulse.Application/      # Application services, DTOs, abstractions
├── DevPulse.Domain/           # Domain entities (User, Repo, Issue, PullRequest, etc.)
├── DevPulse.Infrastructure/   # Persistence, EF DbContext, Providers (GitHub), Security
└── DevPulse.Tests/            # Tests


⸻

Environment Configuration (Infisical)

Secrets are injected with Infisical.
You’ll need:
	•	INFISICAL_PROJECT_ID
	•	INFISICAL_TOKEN

Run commands like this:

infisical run \
  --projectId "$INFISICAL_PROJECT_ID" \
  --token "$INFISICAL_TOKEN" \
  --env=development -- \
  dotnet run --project DevPulse.Api/DevPulse.Api.csproj --urls "http://localhost:5140"

In production, replace Infisical with Azure KeyVault.

⸻

SQL Server in Docker

Start SQL Server 2025 in Docker:

docker run -d --name devpulse-mssql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrongPassword" \
  -p 1433:1433 \
  -v devpulse_mssql_data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2025-latest

Create the DevPulse database and user:

docker exec -it devpulse-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U sa -P 'YourStrongPassword' -C -Q "
IF DB_ID('DevPulse') IS NULL CREATE DATABASE DevPulse;
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'devpulse')
  CREATE LOGIN devpulse WITH PASSWORD = 'YourStrongPassword';
USE DevPulse;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'devpulse')
  CREATE USER devpulse FOR LOGIN devpulse;
ALTER ROLE db_owner ADD MEMBER devpulse;
"


⸻

Entity Framework Core (Migrations)

Install EF Core tools if needed:

dotnet tool install --global dotnet-ef

Add a migration:

infisical run --projectId "$INFISICAL_PROJECT_ID" --token "$INFISICAL_TOKEN" --env=development -- \
  dotnet ef migrations add InitialCreate \
    --project DevPulse.Infrastructure/DevPulse.Infrastructure.csproj \
    --startup-project DevPulse.Api/DevPulse.Api.csproj

Apply migrations:

infisical run --projectId "$INFISICAL_PROJECT_ID" --token "$INFISICAL_TOKEN" --env=development -- \
  dotnet ef database update \
    --project DevPulse.Infrastructure/DevPulse.Infrastructure.csproj \
    --startup-project DevPulse.Api/DevPulse.Api.csproj


⸻

Running the API

Start the API with secrets injected:

infisical run \
  --projectId "$INFISICAL_PROJECT_ID" \
  --token "$INFISICAL_TOKEN" \
  --env=development -- \
  dotnet run --project DevPulse.Api/DevPulse.Api.csproj --urls "http://localhost:5140"


⸻

Testing Endpoints
	•	Health check:

curl http://localhost:5140/healthz


	•	Swagger UI:
Open http://localhost:5140/swagger
	•	Register a user:

curl -X POST http://localhost:5140/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"MyPass123!"}'



⸻

Notes
	•	EF may warn about shadow foreign keys (UserId1) — these are fixed in follow-up migrations.
	•	Infisical is used for local development. Replace with Azure KeyVault in production.
	•	Frontend is expected to run at http://localhost:5173 (CORS enabled).

⸻

Sample appsettings.Development.json

This file should exist under DevPulse.Api/.
Most values are injected via Infisical, but here’s the baseline:

{
  "ConnectionStrings": {
    "db": "Server=localhost,1433;Database=DevPulse;User Id=devpulse;Password=YourStrongPassword;TrustServerCertificate=True"
  },
  "Auth": {
    "Issuer": "DevPulse",
    "Audience": "DevPulseClient",
    "SigningKey": "YOUR_DEV_SIGNING_KEY"
  },
  "GitHub": {
    "ClientId": "YOUR_GITHUB_CLIENT_ID",
    "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
  },
  "Jira": {
    "BaseUrl": "https://yourcompany.atlassian.net",
    "ClientId": "YOUR_JIRA_CLIENT_ID",
    "ClientSecret": "YOUR_JIRA_CLIENT_SECRET"
  },
  "OpenWeather": {
    "ApiKey": "YOUR_OPENWEATHER_API_KEY"
  }
}

Infisical overrides these with secure values when you run with infisical run.

⸻

After following this, you can clone the repo, set up Docker, inject secrets with Infisical, run migrations, and launch the API without extra fixes.