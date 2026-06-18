<div align="center">

# 🎈 GatherPulse

### *Discover your next experience.*

A full-stack event platform — create gatherings, fill them up, and chat about them live — built on **.NET Aspire**, **ASP.NET Core 10**, **MongoDB**, and a **React 19 / Vite** SPA.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Aspire](https://img.shields.io/badge/.NET%20Aspire-orchestration-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/aspire/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=white)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![MongoDB](https://img.shields.io/badge/MongoDB-EF%20Core%20provider-47A248?logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![SignalR](https://img.shields.io/badge/SignalR-realtime-0A66C2)](https://learn.microsoft.com/aspnet/core/signalr/introduction)
[![MediatR](https://img.shields.io/badge/CQRS-MediatR-orange)](https://github.com/jbogard/MediatR)
[![License](https://img.shields.io/badge/license-academic%20project-lightgrey)](#)

</div>

---

## The idea

Most "event app" student projects stop at a CRUD form for creating events. GatherPulse was built to go a step further and feel like a real product: organizers should be able to draft an event privately, publish it when it's ready, watch attendees fill the seats, and talk to them in real time — without a page refresh in sight.

That meant the team had to solve four problems at once: a clean way to grow the backend without endpoints turning into spaghetti (vertical-slice CQRS via MediatR), a single source of truth for who's allowed to do what (JWT + role policies + ownership checks), a live channel for conversation under each event (SignalR, with threaded replies), and a development setup that "just runs" for every contributor regardless of whether they have MongoDB installed locally (.NET Aspire orchestration spinning up a containerized database on demand, with a plain Docker Compose path as a fallback).

The result is a small but complete event lifecycle: **draft → publish → fill up → discuss → cancel/close** — with a versioned, documented API and an admin layer sitting on top to moderate the community.

## Highlights

| Area | What it does |
|---|---|
| 🔐 **Auth** | Registration + login issuing JWTs; live username/email availability check; passwords hashed with ASP.NET Core Identity's PBKDF2 hasher; role-based policies (`User` / `Admin`) plus per-resource ownership checks |
| 🗂️ **Event lifecycle** | Events are created as `Draft`, explicitly `Published` by the organizer, and can be `Cancelled` — never silently visible before they're ready |
| 🎟️ **Bookings** | Join/leave an event with live attendee counts and capacity enforcement (`MaxAttendees`) |
| 💬 **Real-time, threaded comments** | Per-event chat broadcast over SignalR (`/hubs/comments`) — comments support nested replies (`ParentCommentId`), and created/updated/deleted comments push instantly to everyone viewing that event |
| 🖼️ **Image uploads** | Organizers can upload a cover image for their event; files are stored under `wwwroot/uploads` and served back over a versioned API route |
| 🗺️ **Map-ready events** | Events carry optional latitude/longitude and expose a dedicated map-data endpoint (`GET /api/events/map`) |
| 🛡️ **Admin console** | Ban/unban users, manage categories, list every event regardless of status, and force-delete any event or comment, independent of the organizer/owner checks on the normal endpoints |
| 🧱 **Vertical-slice CQRS** | Every feature is a self-contained folder (Command/Query, Handler, Endpoint, Validator, Response) — no shared "god" controllers or services |
| 📜 **Versioned & documented API** | Every route is grouped under `/api/v1`, with OpenAPI + a Scalar UI for interactive docs in development |
| 🚀 **One-command dev environment** | `.NET Aspire` AppHost provisions MongoDB (container or Atlas), the API, and the Vite frontend together, wired up automatically — or use the included `docker-compose.yml` if you'd rather run Mongo by hand |

## Architecture

```
┌───────────────────────────────┐
│     EventPlanner.AppHost      │
│  (.NET Aspire orchestrator)   │
└───────────────────────────────┘
                │
                ▼
                  provisions resources, injects connection
                  strings / env vars
                │
                ▼
┌───────────────────────────────┐
│            MongoDB            │
│     (container or Atlas)      │
└───────────────────────────────┘
                ▲
                │ MongoDB EF Core provider + native driver
                │
┌───────────────────────────────┐
│      EventPlanner.Server      │
│      ASP.NET Core 10 API      │
└───────────────────────────────┘
                │
        ┌───────┴───────┐
        ▼               ▼
    /api/v1/*    /hubs/comments
     (REST)        (WebSocket)
        │               │
        └───────┬───────┘
                ▼
┌───────────────────────────────┐
│     frontend (Vite/React)     │
│     served via dev proxy      │
└───────────────────────────────┘
```

**Request pipeline inside the API**, for every command/query:

```
HTTP request
   │
   ▼
app.UseExceptionHandler()  → GlobalExceptionHandler maps NotFound/Conflict/Forbidden/Validation → ProblemDetails
   │
   ▼
UseAuthentication() → BannedUserMiddleware (rejects banned users immediately) → UseAuthorization()
   │
   ▼
Minimal API Endpoint  (Features/*/Endpoint.cs — auto-discovered via IEndpoint reflection scan, mapped under /api/v1)
   │  sender.Send(command)
   ▼
MediatR pipeline
   ├─ ExceptionHandlingBehavior   → maps exceptions to ProblemDetails
   ├─ LoggingBehavior             → structured request/response logging
   └─ ValidationBehavior          → runs the feature's FluentValidation rules
   │
   ▼
Handler  (business logic)
   │
   ▼
Repository (IEventRepository / IUserRepository / ...)
   │
   ▼
MongoDB  (via the MongoDB EF Core provider, DbContext-based)
```

Comments additionally fan out through `CommentHub` to every client subscribed to that event's SignalR group, so a posted/edited/deleted comment — top-level or a reply — appears for every other viewer without polling.

## How it works

1. **Browse** — anyone (even logged out) can explore published events, filter by category, and search by title/description (`GET /api/v1/events`, paginated).
2. **Sign up / log in** — the signup form checks username/email availability live (`GET /api/v1/auth/check-availability`), validates the rest, and hashes the password; login returns a JWT that the SPA stores and attaches as `Authorization: Bearer <token>` on every subsequent call.
3. **Open an event** — the detail page loads the event, its attendee list, and its comment thread (built as a reply tree), then opens a SignalR connection and joins that event's group (`JoinEventGroup`).
4. **Join / leave** — authenticated users book a seat (capped by `MaxAttendees` if set) or release it; the attendee list and capacity bar update immediately.
5. **Chat live** — posting a top-level comment or a threaded reply, editing, or deleting broadcasts `CommentCreated` / `CommentUpdated` / `CommentDeleted` to everyone in the group in real time.
6. **Organize** — the event's organizer (or an admin) can publish a draft, cancel a published event, edit details, upload a cover image, or delete it outright.
7. **Moderate** — admins get a separate panel to manage categories, list every event regardless of status, ban/unban misbehaving users (a ban takes effect on the very next request, no waiting for the JWT to expire), and force-delete any event or comment regardless of ownership.

## Tech stack

| Layer | Technology |
|---|---|
| Backend runtime | ASP.NET Core 10 Minimal APIs, `net10.0` |
| Backend architecture | Vertical-slice CQRS via **MediatR**, **FluentValidation**, custom pipeline behaviors + a global `IExceptionHandler` |
| Auth | **JWT Bearer** (`System.IdentityModel.Tokens.Jwt`), ASP.NET Core Identity password hasher, a `BannedUserMiddleware` for instant ban enforcement |
| Database | **MongoDB**, accessed through the **`MongoDB.EntityFrameworkCore`** provider (DbContext + repositories) plus a directly-registered **`MongoDB.Driver`** client for seeding |
| API docs | **OpenAPI** (`Microsoft.AspNetCore.OpenApi`) + **Scalar** interactive UI in development |
| Real-time | **SignalR** (`CommentHub`), with bearer-token-over-querystring support for WebSocket auth |
| Orchestration | **.NET Aspire** (`EventPlanner.AppHost`) — local MongoDB container *or* Atlas connection string, health-check-gated startup, env var injection; a root-level `docker-compose.yml` is provided as a non-Aspire alternative |
| Frontend | **React 19** + **TypeScript**, built with **Vite 7** |
| Real-time client | `@microsoft/signalr` |
| Styling | Tailwind (CDN) + Material Symbols |
| Tests | **xUnit** + **Moq** (unit), `WebApplicationFactory` (integration) |

## API surface

All routes below are served under the `/api/v1` prefix (e.g. `GET /api/v1/events`); the frontend's API client rewrites plain `/api/...` calls to `/api/v1/...` automatically.

| Method | Route | Auth | Purpose |
|---|---|---|---|
| POST | `/auth/register` | – | Create an account |
| POST | `/auth/login` | – | Exchange credentials for a JWT |
| GET | `/auth/me` | user | Current user profile |
| GET | `/auth/check-availability` | – | Live username/email availability check |
| GET | `/categories` | – | List categories |
| POST/PUT/DELETE | `/admin/categories[/{id}]` | admin | Manage categories |
| GET | `/events` | – | List events, paginated (`categoryId`, `searchTerm`, `status`, `organizerId` filters) |
| GET | `/events/map` | – | Events with coordinates, for map views |
| GET | `/events/{id}` | – | Event details |
| POST | `/events` | user | Create a draft event |
| PUT / DELETE | `/events/{id}` | owner/admin | Update / delete an event |
| POST | `/events/{id}/publish` | owner/admin | Draft → Published |
| POST | `/events/{id}/cancel` | owner/admin | Published → Cancelled |
| POST | `/events/upload-image` | user | Upload an event cover image, returns its URL |
| GET | `/events/{id}/attendees` | – | Attendee list |
| POST | `/bookings/{eventId}/join` | user | Book a seat |
| DELETE | `/bookings/{eventId}/leave` | user | Release a seat |
| GET | `/bookings/my` | user | My bookings |
| GET / POST | `/events/{eventId}/comments` | – / user | Read (as a reply tree) / post a comment or threaded reply |
| PUT / DELETE | `/events/{eventId}/comments/{commentId}` | owner | Edit / delete own comment |
| GET | `/admin/users` | admin | List all users |
| GET | `/admin/events` | admin | List every event, regardless of status |
| PATCH | `/admin/users/{userId}/ban` | admin | Ban a user |
| PATCH | `/admin/users/{userId}/unban` | admin | Lift a ban |
| DELETE | `/admin/events/{id}` | admin | Force-delete any event |
| DELETE | `/admin/comments/{id}` | admin | Force-delete any comment |
| WS | `/hubs/comments` | optional | SignalR hub — `JoinEventGroup`/`LeaveEventGroup`, pushes `CommentCreated`/`Updated`/`Deleted` |

## What's in this repo

```
Event-Planner/
├── EventPlanner.AppHost/        # .NET Aspire orchestrator — wires Mongo + API + frontend together
├── EventPlanner.Server/         # ASP.NET Core 10 API
│   ├── Domain/
│   │   ├── Entities/             # User, Event, Booking, Comment (ParentCommentId), Category
│   │   └── Enums/                 # EventStatus, UserRole
│   ├── Features/                 # one folder per use case (vertical slices)
│   │   ├── Auth/                  # Register, Login, GetCurrentUser, CheckAvailability
│   │   ├── Categories/             # GetCategories
│   │   ├── Events/                 # CreateEvent, GetEvents, GetEventById, UpdateEvent, DeleteEvent,
│   │   │                            # PublishEvent, CancelEvent, GetMapEvents, UploadImage
│   │   ├── Bookings/                # JoinEvent, LeaveEvent, GetMyBookings, GetEventAttendees
│   │   ├── Comments/                 # CreateComment, GetEventComments, UpdateComment, DeleteComment
│   │   └── Admin/                     # GetUsers, BanUser, UnbanUser, GetEvents, Create/Update/DeleteCategory,
│   │                                    # ForceDeleteEvent, ForceDeleteComment
│   ├── Infrastructure/
│   │   ├── Auth/                  # PasswordHasher, JwtTokenService, JwtSettings, BannedUserMiddleware
│   │   ├── Persistence/            # MongoDbContext (EF Core), MongoDbSettings, MongoDbSeeder
│   │   ├── Repositories/            # IXRepository + MongoXRepository per aggregate
│   │   └── SignalR/                  # CommentHub
│   ├── Common/
│   │   ├── Behaviors/              # ExceptionHandling / Logging / Validation MediatR behaviors
│   │   ├── Errors/                  # AppExceptions + GlobalExceptionHandler (IExceptionHandler)
│   │   └── Endpoints/               # IEndpoint + reflection-based auto-registration under /api/v1
│   └── wwwroot/uploads/            # uploaded event cover images
├── frontend/                     # React 19 + TypeScript + Vite SPA
│   └── src/
│       ├── components/             # Navbar, Hero, CategoryFilters, EventCard, AuthModal, Pagination
│       ├── api.ts                   # typed fetch wrapper for the whole REST surface
│       ├── CommentHub.ts             # SignalR client wrapper
│       └── App.tsx                    # view-state driven SPA (explore/detail/create/edit/my-events/admin)
├── tests/
│   ├── EventPlanner.UnitTests/        # handler tests with Moq'd repositories
│   └── EventPlanner.IntegrationTests/  # WebApplicationFactory end-to-end checks
├── docker-compose.yml             # alternative to Aspire: plain MongoDB + mongo-express container
└── seed.js                        # standalone mongosh script to seed categories/sample data
```

## Running it

**Prerequisites:** .NET 10 SDK, Node.js, and either Docker Desktop (for a local Mongo container) or a MongoDB Atlas connection string.

```bash
git clone https://github.com/dasourov/Event-Planner.git
cd Event-Planner
cp .env.example .env   # fill in MONGODB_CONNECTION_STRING / JWT_SECRET if you have them — both are optional
```

The simplest path is to let Aspire run everything:

```bash
cd EventPlanner.AppHost
dotnet run
```

This will:
- spin up a MongoDB container automatically (or use `MONGODB_CONNECTION_STRING` from `.env` if set, e.g. an Atlas cluster),
- start the API once its `/health` check passes,
- start the Vite frontend and point its dev proxy at the API automatically,
- open the **Aspire dashboard** (URL printed in the console, typically `https://localhost:17171`) where you can see logs, traces, and the live ports for every resource.

The database seeds itself on first run with categories and two accounts:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@eventplanner.com` | `Admin123!` |
| User | `user@eventplanner.com` | `User123!` |

**Without Aspire**, you have two options:

1. Run `docker compose up -d` (uses the root `docker-compose.yml`) to get a MongoDB container plus a `mongo-express` admin UI at `http://localhost:8081`, then run the API and frontend as below.
2. Point `MONGODB_CONNECTION_STRING` at any reachable MongoDB instance (local or Atlas).

Either way, the frontend's dev proxy (`vite.config.ts`) reads `SERVER_HTTPS`/`SERVER_HTTP` to know where to forward `/api`, `/hubs`, and `/uploads` traffic — normally injected by Aspire, so export one yourself if running standalone:

```bash
# Terminal 1 — API
cd EventPlanner.Server
dotnet run   # listens on http://localhost:5513 and https://localhost:7570

# Terminal 2 — frontend
cd frontend
npm install
SERVER_HTTP=http://localhost:5513 npm run dev   # http://localhost:5173
```

In development, interactive API docs are available via the Scalar UI mapped by the server (see the root `/` endpoint response for the exact path).

## Tests

```bash
dotnet test
```

Runs the xUnit unit tests (handlers exercised against mocked repositories) and the integration tests (a real `WebApplicationFactory<Program>` hitting the `/api/v1` surface).

## Team

Built as a group project for the Web Services course at TH Rosenheim.

- **Didarul Alam Sourov** ([@dasourov](https://github.com/dasourov)) — project setup, backend architecture, auth, frontend integration
- **Abhishek Bhardwaj** — Bookings & comments, use case diagram and unit testing
- **Wiem** — API documentation, event category, systema architecture diagram & exception handling
- **Jeel Sidpara** — event filtering, search & pagination
