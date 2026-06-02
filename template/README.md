# Ambev Developer Evaluation - DeveloperStore

Sales management system with quantity-based discount rules, built with .NET 8 (backend) and Angular 21 (frontend).

---

## Architecture

### Backend (.NET 8 / C#)

Follows **Clean Architecture** with clear layer separation:

```
backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.WebApi        # REST API, controllers, Swagger, middleware
│   ├── Ambev.DeveloperEvaluation.Application   # Use cases (CQRS via MediatR), handlers, validators
│   ├── Ambev.DeveloperEvaluation.Domain        # Entities, enums, business rules, events, exceptions
│   ├── Ambev.DeveloperEvaluation.ORM           # EF Core (PostgreSQL), MongoDB repos, Redis cache, migrations
│   ├── Ambev.DeveloperEvaluation.IoC           # Centralised dependency injection
│   └── Ambev.DeveloperEvaluation.Common        # Shared utilities (JWT, hashing, validation, health checks)
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit          # Unit tests
│   ├── Ambev.DeveloperEvaluation.Integration   # Integration tests
│   └── Ambev.DeveloperEvaluation.Functional    # Functional tests
├── docker-compose.yml                          # Orchestrates every service
├── seed-data.sql                               # SQL script with schema + seed
└── seed-via-api.sh                             # End-to-end seed through the API
```

**Patterns used:**
- **CQRS** &mdash; Commands and Queries separated via MediatR.
- **Domain Events** &mdash; `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, `ItemCancelledEvent` persisted in the event store.
- **Repository Pattern** &mdash; abstractions in `Domain`, implementations in `ORM`.
- **Event store + Read model (CQRS read side)** &mdash; MongoDB stores events and a denormalised projection (`sales_read`) for fast queries.
- **Cache-aside** &mdash; Redis caches the read endpoints with prefix-based invalidation.
- **Pipeline behavior (MediatR)** &mdash; `ValidationBehavior<TRequest,TResponse>` validates commands before handlers run.
- **Side-effect coordinator** &mdash; `ISaleSideEffects` centralises the write-side fan-out (event store + read model + cache invalidation) so command handlers stay focused on orchestrating domain logic.
- **Global exception middleware** &mdash; maps `DomainException`/`InvalidOperationException` to 400, `KeyNotFoundException` to 404 and `UnauthorizedAccessException` to 401.

**Main entities:**

| Entity | Description |
|--------|-------------|
| `User` | Users with roles (`Admin`, `Manager`, `Customer`) and status (`Active`, `Inactive`, `Suspended`). |
| `Sale` | Sales with number, customer, branch, status and items. Exposes `IsActive` and `EnsureActive(operation)` to guard write operations. |
| `SaleItem` | Sale items with automatic discount calculation based on quantity. |

**Discount rules:**

| Quantity | Discount |
|----------|----------|
| 1-3 items | 0% |
| 4-9 items | 10% |
| 10-20 items | 20% |
| > 20 items | Not allowed |

**API endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth` | Authentication (returns JWT) |
| POST | `/api/users` | Create user |
| GET | `/api/users/{id}` | Get user |
| DELETE | `/api/users/{id}` | Delete user |
| POST | `/api/sales` | Create sale |
| GET | `/api/sales` | List sales (paginated, with filters) |
| GET | `/api/sales/{id}` | Sale details |
| PUT | `/api/sales/{id}` | Update sale |
| DELETE | `/api/sales/{id}` | Cancel sale (soft delete) |
| PATCH | `/api/sales/{saleId}/items/{itemId}/cancel` | Cancel a specific item |

**Listing query parameters (`GET /api/sales`):**

- `_page`, `_size` &mdash; pagination
- `_order` &mdash; ordering (e.g. `saleDate desc`)
- `CustomerName`, `BranchName`, `Status` &mdash; per-field filters
- `StartDate`, `EndDate` &mdash; date range filter

---

### Frontend (Angular 21)

SPA built with Angular 21 standalone components, signals for state management and Tailwind CSS for styling.

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts        # Auth via signals (login, logout, token)
│   │   │   │   ├── auth.guard.ts          # Route guard (redirects to /login)
│   │   │   │   └── auth.interceptor.ts    # HTTP interceptor (injects JWT)
│   │   │   ├── http/
│   │   │   │   └── unwrap.interceptor.ts  # Flattens legacy double-nested ApiResponse envelopes
│   │   │   └── services/
│   │   │       └── sales.service.ts       # Sales CRUD over HttpClient
│   │   ├── features/
│   │   │   ├── login/login.ts             # Login screen
│   │   │   └── sales/
│   │   │       ├── sales-list/            # Paginated list with filters
│   │   │       ├── sales-form/            # Create / edit form
│   │   │       └── sales-detail/          # Details with item cancellation
│   │   └── shared/
│   │       ├── confirm-dialog/            # Reusable confirm dialog
│   │       └── pagination/                # Pagination component
│   ├── environments/                      # Per-environment config
│   ├── styles.css                         # Global styles
│   └── index.html                         # Loads Tailwind via Play CDN (see Decisions)
├── angular.json
├── Dockerfile                             # Multi-stage build (Node + Nginx)
└── nginx.conf                             # Reverse proxy to the API
```

**Routes:**

| Route | Component | Auth required |
|-------|-----------|---------------|
| `/login` | `LoginComponent` | No |
| `/sales` | `SalesListComponent` | Yes |
| `/sales/new` | `SalesFormComponent` | Yes |
| `/sales/:id` | `SalesDetailComponent` | Yes |
| `/sales/:id/edit` | `SalesFormComponent` | Yes |

---

## Technical decisions

### Backend

| Decision | Rationale |
|----------|-----------|
| **Clean Architecture** | Decouples layers; the Domain has no infrastructure dependencies. |
| **CQRS with MediatR** | Splits reads from writes and keeps handlers small. `ValidationBehavior` runs FluentValidation on every command/query before it reaches the handler, so controllers stay thin. |
| **PostgreSQL (EF Core)** | Relational store for transactional data (`Users`, `Sales`, `SaleItems`). |
| **MongoDB** | Event store for auditing and denormalised read models for fast list queries (CQRS read side). |
| **Redis** | Distributed cache for list responses, invalidated by prefix on every write. |
| **BCrypt** | Password hashing with automatic salt. |
| **JWT (Bearer)** | Stateless authentication with role claims. |
| **FluentValidation** | Declarative validation on commands; auto-triggered via MediatR `ValidationBehavior`. |
| **`ISaleSideEffects`** | The 4 sale write handlers (`Create`, `Update`, `Delete`, `CancelItem`) used to repeat the same 4 lines (store event &rarr; upsert read model &rarr; invalidate caches). Centralising it removes ~12 lines of duplication per handler and keeps the side-effect order consistent. |
| **`GlobalExceptionMiddleware`** | A single middleware translates known exceptions to proper HTTP envelopes (`ApiResponse`). Before it existed, `KeyNotFoundException` leaked as a 500 with a `text/plain` body and broke the UI error handling. |
| **`Sale.EnsureActive(operation)`** | Replaces the old `ActiveSaleSpecification` &mdash; an intent-revealing instance method on the aggregate is simpler than a separate type for `sale.Status == Active`. |
| **Soft delete** | Cancelled sales keep their history rather than being physically removed. |
| **Npgsql legacy timestamp** | Enabled in `Program.cs` so `DateTime` values without a timezone (e.g. dates coming from `<input type="date">`) are accepted in `timestamp with time zone` columns. |

### Frontend

| Decision | Rationale |
|----------|-----------|
| **Angular 21 standalone components** | No `NgModule` boilerplate; imports live next to the component. |
| **Signals** | First-party reactive state management; no external library needed (NgRx, etc.). |
| **Tailwind CSS via Play CDN** | Tailwind v4 with the Angular 21 esbuild builder did not generate utility classes through PostCSS in this template. Loading the Play CDN keeps the templates working as-is; for production swap in a properly configured PostCSS pipeline. |
| **Lazy loading** | Routes use `loadComponent()` so each feature ships as its own chunk. |
| **`authInterceptor`** | Adds the `Authorization: Bearer …` header on every authenticated request and triggers logout on 401. |
| **`unwrapInterceptor`** | Defensive interceptor that flattens the legacy double-nested `ApiResponse` envelope when it appears. With the current backend it is a no-op safety net. |
| **Nginx** | Serves the static build and reverse-proxies `/api/` to the .NET container in Docker. |

---

## Running locally

### Prerequisites

- Docker and Docker Compose
- (Optional, for local dev outside Docker) .NET 8 SDK, Node.js 20+, npm

### Bring everything up with Docker (recommended)

```bash
cd template/backend
docker-compose --project-name ambev_eval up --build -d
```

This boots:

| Service | Port | Description |
|---------|------|-------------|
| API (.NET) | `8080` | Swagger at http://localhost:8080/swagger |
| PostgreSQL | `5432` | Relational database |
| MongoDB | `27017` | Event store + read models |
| Redis | `6379` | Cache |
| Frontend (Angular) | `4200` | App at http://localhost:4200 |

> **Note on port 5432.** If you already have a local PostgreSQL listening on `5432`, the database container will fail to bind. Stop the local instance first (e.g. `sudo launchctl unload /Library/LaunchDaemons/postgresql-XX.plist` on macOS) or change the port mapping in `docker-compose.yml`.

### Load seed data

After the stack is up:

```bash
cd template/backend
./seed-via-api.sh
```

The script:

1. Truncates `Sales` / `SaleItems` in Postgres and clears `sales_read` / `sale_events` in Mongo.
2. Signs in as admin and uses the JWT to create 10 sales via `POST /api/sales` (populates both Postgres and Mongo via the side-effects pipeline).
3. Cancels `SALE-005` via `DELETE /api/sales/{id}`.
4. Cancels one item of `SALE-006` via `PATCH /api/sales/{saleId}/items/{itemId}/cancel`.

**Postgres-only alternative** (no Mongo / no events &mdash; the list endpoint will be empty because reads come from Mongo):

```bash
docker exec -i ambev_developer_evaluation_database \
  psql -U developer -d developer_evaluation < seed-data.sql
```

### Test credentials

| Email | Password | Role | Status |
|-------|----------|------|--------|
| admin@ambev.com | Test@123 | Admin | Active |
| manager@ambev.com | Test@123 | Manager | Active |
| customer@ambev.com | Test@123 | Customer | Active |
| inactive@ambev.com | Test@123 | Customer | Inactive (login blocked) |
| suspended@ambev.com | Test@123 | Manager | Suspended (login blocked) |

### Scenarios covered by the seed

| # | Scenario | Sale number |
|---|----------|-------------|
| 1 | Sale with no discount (qty < 4) | SALE-001 |
| 2 | 10% discount (4-9 items) | SALE-002 |
| 3 | 20% discount (10-20 items) | SALE-003 |
| 4 | Mixed discounts across items | SALE-004 |
| 5 | Cancelled sale | SALE-005 |
| 6 | Sale with a single cancelled item | SALE-006 |
| 7 | Sale from a different branch (Salvador) | SALE-007 |
| 8 | High value sale (R$ 5,600) | SALE-008 |
| 9 | Boundary quantity (20 units) | SALE-009 |
| 10 | Sale created today (date filter check) | SALE-010 |

### Run the backend without Docker

```bash
cd template/backend/src/Ambev.DeveloperEvaluation.WebApi

# Requires PostgreSQL, MongoDB and Redis running locally
# (e.g. via `docker-compose up -d database nosql cache`).
dotnet run
```

The API listens on `http://localhost:8080` with Swagger enabled.

### Run the frontend without Docker

```bash
cd template/frontend
npm install
npm start
```

The app listens on `http://localhost:4200` and targets the API at `http://localhost:8080/api` (see `src/environments/environment.ts`).

---

## Tech stack

| Layer | Tech |
|-------|------|
| Backend | .NET 8, C# 12, ASP.NET Core Web API |
| ORM | Entity Framework Core + Npgsql |
| Relational database | PostgreSQL 13 |
| NoSQL | MongoDB 8.0 |
| Cache | Redis 7.4 (StackExchange.Redis) |
| In-process messaging | MediatR |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Authentication | JWT Bearer |
| Password hashing | BCrypt.Net |
| Logging | Serilog |
| Frontend | Angular 21, TypeScript 5.9 |
| CSS | Tailwind CSS 4 (via Play CDN) |
| Container runtime | Docker, Docker Compose |
| Frontend web server | Nginx Alpine |
