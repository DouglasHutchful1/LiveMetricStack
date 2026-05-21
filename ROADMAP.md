# LiveMetricStack Roadmap

## Current Status (May 21, 2026)

- Architecture baseline switched to 4-layer Clean Architecture.
- Database direction locked to PostgreSQL.
- Phase 1 backend foundation started and compiling.

## Layer Structure

1. Domain: entities and core business model.
2. Application: use-case contracts/services.
3. Infrastructure: EF Core, JWT, data/service implementations.
4. WebApi: controllers, HTTP contracts, composition root.

## Phase 1 - Backend Foundation

Goal: stable API base with auth + applications on PostgreSQL.

Completed:
- [x] Four projects created and wired by layer.
- [x] JWT authentication/authorization configured.
- [x] PostgreSQL `DbContext` and entity mappings added.
- [x] `/api/auth` module implemented.
- [x] `/api/applications` module implemented.
- [x] Solution builds successfully.

Pending:
- [x] Initial EF migration for PostgreSQL.
- [ ] Docker compose for local PostgreSQL/Redis.

Acceptance criteria:
- API starts locally.
- User can register/login and receive JWT.
- Authorized user can manage applications.

## Phase 2 - Metrics Engine

- [x] Add `Metrics` module (`/api/metrics`).
- [x] Add metric generator `BackgroundService`.
- [x] Generate CPU, memory, request count, error rate.

## Phase 3 - Realtime (SignalR)

- [ ] Add `/hubs/metrics` hub.
- [ ] Broadcast metric updates from worker.
- [ ] Broadcast alert events.

## Phase 4 - Frontend Dashboard

- [ ] React + Tailwind setup.
- [ ] Login + protected routes.
- [ ] Live metric cards + realtime charts.
- [ ] Alerts panel + event stream + health history.

## Phase 5 - Alerts and Health Checks

- [ ] Alert rules engine.
- [ ] `/api/alerts` endpoints.
- [ ] `/api/health-checks` endpoints + monitor worker.

## API Module Progress

- [x] `/api/auth`
- [x] `/api/applications`
- [x] `/api/metrics`
- [ ] `/api/events`
- [ ] `/api/alerts`
- [ ] `/api/health-checks`
- [ ] `/api/dashboard`
- [ ] `/hubs/metrics`

## Immediate Next Sprint

1. Wire SignalR hub for live dashboard updates.
2. Broadcast metrics from `FakeMetricsWorker` to connected clients.
3. Add `/api/events` and `/api/dashboard` summary endpoint.
4. Add Docker Compose for PostgreSQL + Redis (optional).
