# LiveMetricStack

Real-time analytics and monitoring dashboard platform.

## Architecture

This repo now follows 4-layer Clean Architecture:

1. `Domain` (`LiveMetricStack.Domain`)
2. `Application` (`LiveMetricStack.Application`)
3. `Infrastructure` (`LiveMetricStack.Infrastructure`)
4. `WebApi` (`LiveMetricStack.WebApi`)

Dependency direction:
- `Application` -> `Domain`
- `Infrastructure` -> `Application`, `Domain`
- `WebApi` -> `Application`, `Infrastructure`

## Tech Stack

- ASP.NET Core Web API (`net9.0`)
- EF Core + PostgreSQL (Npgsql)
- JWT auth
- SignalR (next phase)
- React + Tailwind (next phase)

## Current Backend Scope (Started)

Implemented endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/applications`
- `GET /api/applications/{id}`
- `POST /api/applications`
- `PATCH /api/applications/{id}/status`
- `POST /api/metrics`
- `GET /api/metrics`
- `GET /api/metrics/{applicationId}/latest`

Implemented schema entities:
- `Users`
- `Applications`
- `Metrics`
- `Events`
- `Alerts`
- `ApiHealthChecks`

Background worker:
- `FakeMetricsWorker` writes demo CPU, memory, request count, and error rate metrics continuously.

## PostgreSQL Configuration

Edit:
- `LiveMetricStack.WebApi/appsettings.json`
- `LiveMetricStack.WebApi/appsettings.Development.json`

Required keys:
- `ConnectionStrings:PostgreSql`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key` (minimum 32 characters)
- `Jwt:ExpirationMinutes`

## Run

1. Restore:
```bash
dotnet restore LiveMetricStack.sln
```

2. Build:
```bash
dotnet build LiveMetricStack.sln
```

3. Run API:
```bash
dotnet run --project LiveMetricStack.WebApi
```

Swagger (Development):
- `https://localhost:7298/swagger`

## Next Build Steps

1. Add `/hubs/metrics` SignalR broadcast.
2. Stream worker-generated metrics to realtime clients.
3. Add events/alerts/health-check modules.
4. Start React dashboard phase.
