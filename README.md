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
- SignalR
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
- `GET /api/dashboard`
- `POST /api/events`
- `GET /api/events`
- `POST /api/alerts`
- `GET /api/alerts`
- `PATCH /api/alerts/{id}/resolve`
- `POST /api/health-checks`
- `GET /api/health-checks`
- `WS /hubs/metrics` (SignalR)

Implemented schema entities:
- `Users`
- `Applications`
- `Metrics`
- `Events`
- `Alerts`
- `ApiHealthChecks`

Background worker:
- `FakeMetricsWorker` writes demo CPU, memory, request count, and error rate metrics continuously.
- Worker also broadcasts live metric batches to SignalR clients using event: `metrics:batch`.
- Worker writes event records and triggers/resolves high-error alerts.
- `HealthMonitorWorker` runs endpoint probes and stores `ApiHealthChecks`.
- `MetricsRetentionWorker` prunes old metric records by retention policy.

SignalR usage:
- Clients should call `JoinApplication(applicationId)` after connect.
- Clients receive app-scoped metric batches via `metrics:batch`.
- Clients can call `LeaveApplication(applicationId)` when switching context.
- Clients receive alert notifications via `alerts:new` and `alerts:resolved`.

Caching:
- Distributed cache is wired for metrics and dashboard queries.
- Set `Cache:EnableRedis=true` and provide `ConnectionStrings:Redis` to use Redis.
- If Redis is disabled/unavailable, API falls back to in-memory distributed cache.

## PostgreSQL Configuration

Edit:
- `LiveMetricStack.WebApi/appsettings.json`
- `LiveMetricStack.WebApi/appsettings.Development.json`

Required keys:
- `ConnectionStrings:PostgreSql`
- `ConnectionStrings:Redis`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key` (minimum 32 characters)
- `Jwt:ExpirationMinutes`
- `MetricsGenerator:*`
- `MetricsRetention:*`
- `HealthMonitor:*`
- `Cache:*`

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

1. Add Docker Compose for PostgreSQL + Redis local stack.
2. Build React dashboard with SignalR subscription.
3. Add auth/role UX (admin operations, alert workflows).
