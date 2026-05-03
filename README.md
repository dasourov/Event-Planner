# Event Planner

A full-stack event management platform built with ASP.NET Core 10 and React.
Developed as a university project for the Web Services with .NET course.

---

## Tech Stack

**Backend**
- ASP.NET Core 10 — Minimal APIs with route groups
- Vertical Slice Architecture + MediatR (CQRS)
- Entity Framework Core 10 + PostgreSQL
- FluentValidation + Problem Details (RFC 9457)
- JWT Bearer Authentication
- SignalR — real-time attendee count and live comment feed
- HybridCache — L1 memory + L2 distributed cache
- Background Service — event reminders
- OpenAPI (built-in .NET 10) + Scalar UI
- .NET Aspire — orchestration, service discovery, observability

**Frontend**
- Vite + React + TypeScript
- Leaflet.js + OpenStreetMap — interactive event map

---

## Project Structure

```
EventPlanner/
├── EventPlanner.AppHost/          # Aspire orchestration (PostgreSQL, API, frontend)
├── EventPlanner.Server/           # Backend API
│   ├── Features/
│   │   ├── Auth/                  # Register · Login · GetMe
│   │   ├── Events/                # CRUD · Publish · Cancel · Map
│   │   ├── Bookings/              # Join · Leave · MyBookings · Attendees
│   │   ├── Comments/              # Create · List · Edit · Delete
│   │   └── Categories/            # List
│   ├── Common/
│   │   ├── Behaviors/             # MediatR logging + validation pipeline
│   │   └── Errors/                # ValidationExceptionHandler
│   └── Data/                      # AppDbContext + Entities
├── EventPlanner.Tests/            # Integration tests (WebApplicationFactory + Testcontainers)
├── frontend/                      # Vite + React + TypeScript
└── docs/
    ├── erd.drawio                 # Entity relationship diagram
    └── usecase.drawio             # Use case diagram
```

---

## Architecture

**Vertical Slice Architecture** — each feature lives in a single file containing the request record, handler, response DTO, and validator. No shared service layer.

```
Request
  └── Endpoint (Minimal API route group)
        └── MediatR
              ├── LoggingBehavior
              ├── ValidationBehavior (FluentValidation)
              └── Handler
                    └── EF Core → PostgreSQL
```

---

## API Endpoints

### Auth

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Create a new account |
| POST | `/api/auth/login` | No | Login — returns JWT token |
| GET | `/api/auth/me` | Yes | Current user profile |

### Events

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/events` | No | Paginated list — `?search=` `?category=` `?status=` |
| GET | `/api/events/map` | No | Lightweight list with lat/lng for map pins |
| GET | `/api/events/{id}` | No | Event detail with attendee count |
| POST | `/api/events` | Yes | Create event (status: Draft) |
| PUT | `/api/events/{id}` | Yes | Update event (organizer only) |
| DELETE | `/api/events/{id}` | Yes | Delete event (organizer only) |
| POST | `/api/events/{id}/publish` | Yes | Publish event — makes it publicly visible |
| POST | `/api/events/{id}/cancel` | Yes | Cancel event |

### Bookings

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/bookings/{eventId}/join` | Yes | Join event (validates capacity) |
| DELETE | `/api/bookings/{eventId}/leave` | Yes | Leave event |
| GET | `/api/bookings/my` | Yes | Current user's booked events |
| GET | `/api/events/{id}/attendees` | Yes | List of event attendees |

### Comments

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/events/{eventId}/comments` | Yes | Post comment (broadcasts via SignalR) |
| GET | `/api/events/{eventId}/comments` | No | List comments with nested replies |
| PUT | `/api/events/{eventId}/comments/{commentId}` | Yes | Edit own comment |
| DELETE | `/api/events/{eventId}/comments/{commentId}` | Yes | Delete own comment |

### Categories

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/categories` | No | List all categories |

### Admin

> All admin endpoints require authentication and the `Admin` role.

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/admin/categories` | Admin | Create a new category |
| PUT | `/api/admin/categories/{id}` | Admin | Update a category |
| DELETE | `/api/admin/categories/{id}` | Admin | Delete a category |
| GET | `/api/admin/users` | Admin | List all registered users |
| PATCH | `/api/admin/users/{id}/ban` | Admin | Ban / deactivate a user |
| DELETE | `/api/admin/events/{id}` | Admin | Force-delete any event (moderation) |
| DELETE | `/api/admin/comments/{id}` | Admin | Force-delete any comment (moderation) |

---

## Data Model

Five entities: `User`, `Event`, `Booking`, `Comment`, `Category`.

`User` has a `Role` field — `Member` (default) or `Admin`. Role is assigned at registration or promoted manually. JWT claims carry the role so authorization policies can enforce it on every request.

`Event` uses an EF Core **Owned Entity** for the address — `Street`, `City`, `Country`, `PostalCode`, `Latitude`, `Longitude` are stored as flat columns in the Events table. No extra join needed, and `Latitude`/`Longitude` feed the map feature.

`Comment` supports threaded replies via a self-referencing `ParentCommentId` foreign key.

See [`docs/erd.drawio`](docs/erd.drawio) for the full entity relationship diagram.
See [`docs/usecase.drawio`](docs/usecase.drawio) for the use case diagram.

---

## Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL runs in a container via Aspire)
- [Node.js 20+](https://nodejs.org/)

### Run with Aspire (recommended)

```bash
cd EventPlanner.AppHost
dotnet run
```

Aspire starts PostgreSQL automatically, runs the API, and serves the frontend.
Open the Aspire Dashboard at `http://localhost:15888` to monitor all services.

### API Documentation

Once running, open Scalar at:

```
https://localhost:{port}/scalar/v1
```

### Manual run (without Aspire)

```bash
# 1. Start PostgreSQL
docker run -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres

# 2. Apply migrations
cd EventPlanner.Server
dotnet ef database update

# 3. Start the API
dotnet run

# 4. Start the frontend
cd ../frontend
npm install
npm run dev
```

---

## Authentication

Protected endpoints require a Bearer token in the `Authorization` header:

```http
Authorization: Bearer <your-jwt-token>
```

Obtain a token via `POST /api/auth/login`. The `EventPlanner.Server.http` file contains ready-to-run request examples for all endpoints, including chained auth flows.

---

## Running Tests

```bash
cd EventPlanner.Tests
dotnet test
```

Tests use `WebApplicationFactory` + `Testcontainers` (real PostgreSQL) + `Respawn` for fast database reset between test runs.

---

## Team

University project — Web Services with .NET course.

| Name | GitHub |
|------|--------|
| — | — |
| — | — |

---

## License

Educational purposes only.
