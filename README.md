# Omne CRUD Demo

Small product-management application built with .NET 10, FastEndpoints,
PostgreSQL, React, and Aspire.

## Run locally

Prerequisites:

- .NET 10 SDK
- Node.js 20.19+ or 22.12+
- PostgreSQL running locally on port 5432

Create the development database, then store its connection string in AppHost
user secrets (replace `<password>` with the local PostgreSQL password):

```powershell
psql -h localhost -p 5432 -U postgres -c 'CREATE DATABASE omne;'

dotnet user-secrets set `
  "ConnectionStrings:productsdb" `
  "Host=localhost;Port=5432;Database=omne;Username=postgres;Password=<password>" `
  --project .\Omne-Crud-Demo.AppHost\Omne-Crud-Demo.AppHost.csproj
```

From the repository root:

```powershell
dotnet run --project .\Omne-Crud-Demo.AppHost\Omne-Crud-Demo.AppHost.csproj
```

Aspire uses the configured local PostgreSQL connection, installs the frontend
dependencies, applies the EF Core migrations, starts the API, and then starts
the React frontend. The existing database appears in the Aspire dashboard as
the external `productsdb` resource, but Aspire does not provision or control
the PostgreSQL service. Docker is not required by the application.

The development connection string is intentionally not stored in an
`appsettings.json` file. AppHost reads `ConnectionStrings:productsdb` from its
user-secrets store, wraps it in the secret `productsdb-connection` parameter,
and exposes it as the `productsdb` connection-string resource. Aspire masks the
parameter value in the dashboard. The `WithReference` calls then inject that
resource into the migrator and server as `ConnectionStrings__DefaultConnection`.
The AppHost orchestration tests verify both the secret parameter and the
injected environment-variable name without exposing the credential value.

The server resource exposes an `API docs` link in the Aspire dashboard. Open it
to use the Scalar reference UI for the FastEndpoints OpenAPI document. The
machine-readable document is available at `/openapi/v1.json`, and the
interactive reference is available at `/scalar/v1` while running in Development.

## Tests

Run the frontend checks:

```powershell
cd .\frontend
npm test
npm run lint
npm run build
```

Database-backed integration and end-to-end tests use a separate local database.
Create it and configure the existing shared `TestConnectionString` setting in
the Server user secrets:

```powershell
psql -h localhost -p 5432 -U postgres -c 'CREATE DATABASE omne_test;'

dotnet user-secrets set `
  "ConnectionStrings:TestConnectionString" `
  "Host=localhost;Port=5432;Database=omne_test;Username=postgres;Password=<password>" `
  --project .\Omne-Crud-Demo.Server\Omne-Crud-Demo.Server.csproj
```

All database-backed suites consume that one setting. As a safety guard, they
refuse to run unless the configured database name contains `test`; this prevents
their cleanup operations from targeting the `omne` development database. The
tests are real PostgreSQL tests and do not use mocks, Docker, or Testcontainers.

The end-to-end suite maps `TestConnectionString` to AppHost's `productsdb`
resource, starts the complete Aspire application, and drives
Chromium through the real frontend create, list, update, and delete workflows,
including persistence verification after page reloads. Build the project,
then install the matching browser once:

```powershell
dotnet build .\Tests\Omne-Crud-Demo.EndToEnd.Tests\Omne-Crud-Demo.EndToEnd.Tests.csproj
pwsh .\Tests\Omne-Crud-Demo.EndToEnd.Tests\bin\Debug\net10.0\playwright.ps1 install chromium
```

Run the complete .NET test suite, including E2E:

```powershell
$testProjects = Get-ChildItem .\Tests -Filter *.csproj -Recurse

foreach ($testProject in $testProjects) {
  dotnet test $testProject.FullName
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
```

The loop runs projects sequentially so database-backed suites do not clean the
shared test database while another suite is using it.

## Production migrations

The orchestrated migrator is intended to make local Aspire startup automatic.
For production, publish and review an EF Core migration script or migration
bundle and execute it as a controlled deployment step.
