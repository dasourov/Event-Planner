<div align="center">

# 🎈 GatherPulse

### *Discover your next experience.*

A full-stack event platform — create gatherings, fill them up, and chat about them live — built on **.NET Aspire**, **ASP.NET Core 10**, **MongoDB**, and a **React 19 / Vite** SPA.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Aspire](https://img.shields.io/badge/.NET%20Aspire-orchestration-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/aspire/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=white)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![MongoDB](https://img.shields.io/badge/MongoDB-driver%203.x-47A248?logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![SignalR](https://img.shields.io/badge/SignalR-realtime-0A66C2)](https://learn.microsoft.com/aspnet/core/signalr/introduction)
[![MediatR](https://img.shields.io/badge/CQRS-MediatR-orange)](https://github.com/jbogard/MediatR)
[![License](https://img.shields.io/badge/license-academic%20project-lightgrey)](#)

</div>

---

## The idea

Most "event app" student projects stop at a CRUD form for creating events. GatherPulse was built to go a step further and feel like a real product: organizers should be able to draft an event privately, publish it when it's ready, watch attendees fill the seats, and talk to them in real time — without a page refresh in sight.

That meant the team had to solve four problems at once: a clean way to grow the backend without endpoints turning into spaghetti (vertical-slice CQRS via MediatR), a single source of truth for who's allowed to do what (JWT + role policies), a live channel for conversation under each event (SignalR), and a development setup that "just runs" for every contributor regardless of whether they have MongoDB installed locally (.NET Aspire orchestration spinning up a containerized database on demand).

The result is a small but complete event lifecycle: **draft → publish → fill up → discuss → cancel/close** — with an admin layer sitting on top to moderate the community.

## Highlights

| Area | What it does |
|---|---|
| 🔐 **Auth** | Registration + login issuing JWTs; passwords hashed with ASP.NET Core Identity's PBKDF2 hasher; role-based policies (`User` / `Admin`) |
| 🗂️ **Event lifecycle** | Events are created as `Draft`, explicitly `Published` by the organizer, and can be `Cancelled` — never silently visible before they're ready |
| 🎟️ **Bookings** | Join/leave an event with live attendee counts and capacity enforcement (`MaxAttendees`) |
| 💬 **Real-time comments** | Per-event chat broadcast over SignalR (`/hubs/comments`) — created/updated/deleted comments push instantly to everyone viewing that event |
| 🗺️ **Map-ready events** | Events carry optional latitude/longitude for a map view (`GET /api/events/map`) |
| 🛡️ **Admin console** | Ban users, manage categories, force-delete any event or comment, independent of the organizer/owner check on the normal endpoints |
| 🧱 **Vertical-slice CQRS** | Every feature is a self-contained folder (Command/Query, Handler, Endpoint, Validator, Response) — no shared "god" controllers or services |
| 🚀 **One-command dev environment** | `.NET Aspire` AppHost provisions MongoDB (container or Atlas), the API, and the Vite frontend together, wired up automatically |

## Architecture

```
                         ┌────────────────────────────────┐
                         │       EventPlanner.AppHost        │
                         │      (.NET Aspire orchestrator)   │
                         └────────────────┬───────────────────┘
                                          │ provisions + injects connection strings / env vars
              ┌────────────────────────────┼────────────────────────────┐
              ▼                            ▼                            ▼
   ┌────────────────────┐      ┌─────────────────────────┐   ┌───────────────────────┐
   │      MongoDB          │◄────┤   EventPlanner.Server     │────►│  frontend (Vite/React)  │
   │  container or Atlas   │      │   ASP.NET Core 10 API      │      │   served via dev proxy   │
   └────────────────────┘      └─────────────────────────┘   └───────────────────────┘
                                          ▲      ▲
                                  /api/*  │      │ /hubs/comments (WebSocket)
                                          │      │
                                   REST requests   live comment events
```

**Request pipeline inside the API**, for every command/query:

```
HTTP request
   │
   ▼
Minimal API Endpoint  (Features/*/Endpoint.cs — auto-discovered via IEndpoint reflection scan)
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
MongoDB
```

Comments additionally fan out through `CommentHub` to every client subscribed to that event's SignalR group, so a posted/edited/deleted comment appears for every other viewer without polling.

## How it works

1. **Browse** — anyone (even logged out) can explore published events, filter by category, and search by title/description (`GET /api/events`).
2. **Sign up / log in** — registration validates username/email/password and hashes the password; login returns a JWT that the SPA stores and attaches as `Authorization: Bearer <token>` on every subsequent call.
3. **Open an event** — the detail page loads the event, its attendee list, and its comment thread, then opens a SignalR connection and joins that event's group (`JoinEventGroup`).
4. **Join / leave** — authenticated users book a seat (capped by `MaxAttendees` if set) or release it; the attendee list and capacity bar update immediately.
5. **Chat live** — posting, editing, or deleting a comment broadcasts `CommentCreated` / `CommentUpdated` / `CommentDeleted` to everyone in the group in real time.
6. **Organize** — the event's organizer (or an admin) can publish a draft, cancel a published event, edit details, or delete it outright.
7. **Moderate** — admins get a separate panel to manage categories, ban misbehaving users, and force-delete any event or comment regardless of ownership.

## Tech stack

| Layer | Technology |
|---|---|
| Backend runtime | ASP.NET Core 10 Minimal APIs, `net10.0` |
| Backend architecture | Vertical-slice CQRS via **MediatR**, **FluentValidation**, custom pipeline behaviors |
| Auth | **JWT Bearer** (`System.IdentityModel.Tokens.Jwt`), ASP.NET Core Identity password hasher |
| Database | **MongoDB** (`MongoDB.Driver` 3.x), repository pattern per aggregate |
| Real-time | **SignalR** (`CommentHub`), with bearer-token-over-querystring support for WebSocket auth |
| Orchestration | **.NET Aspire** (`EventPlanner.AppHost`) — local MongoDB container *or* Atlas connection string, health-check-gated startup, env var injection |
| Frontend | **React 19** + **TypeScript**, built with **Vite 7** |
| Real-time client | `@microsoft/signalr` |
| Styling | Tailwind (CDN) + Material Symbols |
| Tests | **xUnit** + **Moq** (unit), `WebApplicationFactory` (integration) |

## API surface

| Method | Route | Auth | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | – | Create an account |
| POST | `/api/auth/login` | – | Exchange credentials for a JWT |
| GET | `/api/auth/me` | user | Current user profile |
| GET | `/api/categories` | – | List categories |
| POST/PUT/DELETE | `/api/admin/categories[/{id}]` | admin | Manage categories |
| GET | `/api/events` | – | List events (`categoryId`, `searchTerm` filters) |
| GET | `/api/events/map` | – | Events with coordinates, for map views |
| GET | `/api/events/{id}` | – | Event details |
| POST | `/api/events` | user | Create a draft event |
| PUT / DELETE | `/api/events/{id}` | owner/admin | Update / delete an event |
| POST | `/api/events/{id}/publish` | owner/admin | Draft → Published |
| POST | `/api/events/{id}/cancel` | owner/admin | Published → Cancelled |
| GET | `/api/events/{id}/attendees` | – | Attendee list |
| POST | `/api/bookings/{eventId}/join` | user | Book a seat |
| DELETE | `/api/bookings/{eventId}/leave` | user | Release a seat |
| GET | `/api/bookings/my` | user | My bookings |
| GET / POST | `/api/events/{id}/comments` | – / user | Read / post comments |
| PUT / DELETE | `/api/comments/{id}` | owner | Edit / delete own comment |
| GET | `/api/admin/users` | admin | List all users |
| POST | `/api/admin/users/{userId}/ban` | admin | Ban a user |
| DELETE | `/api/admin/events/{id}` | admin | Force-delete any event |
| DELETE | `/api/admin/comments/{id}` | admin | Force-delete any comment |
| WS | `/hubs/comments` | optional | SignalR hub — `JoinEventGroup`/`LeaveEventGroup`, pushes `CommentCreated`/`Updated`/`Deleted` |

## What's in this repo

```
Event-Planner/
├── EventPlanner.AppHost/        # .NET Aspire orchestrator — wires Mongo + API + frontend together
├── EventPlanner.Server/         # ASP.NET Core 10 API
│   ├── Domain/
│   │   ├── Entities/             # User, Event, Booking, Comment, Category
│   │   └── Enums/                 # EventStatus, UserRole
│   ├── Features/                 # one folder per use case (vertical slices)
│   │   ├── Auth/                  # Register, Login, GetCurrentUser
│   │   ├── Categories/             # GetCategories
│   │   ├── Events/                 # CreateEvent, GetEvents, GetEventById, UpdateEvent,
│   │   │                            # DeleteEvent, PublishEvent, CancelEvent, GetMapEvents
│   │   ├── Bookings/                # JoinEvent, LeaveEvent, GetMyBookings, GetEventAttendees
│   │   ├── Comments/                 # CreateComment, GetEventComments, UpdateComment, DeleteComment
│   │   └── Admin/                     # GetUsers, BanUser, Create/Update/DeleteCategory,
│   │                                    # ForceDeleteEvent, ForceDeleteComment
│   ├── Infrastructure/
│   │   ├── Auth/                  # PasswordHasher, JwtTokenService, JwtSettings
│   │   ├── Persistence/            # MongoDbContext, MongoDbSettings, MongoDbSeeder
│   │   ├── Repositories/            # IXRepository + MongoXRepository per aggregate
│   │   └── SignalR/                  # CommentHub
│   └── Common/
│       ├── Behaviors/              # ExceptionHandling / Logging / Validation MediatR behaviors
│       └── Endpoints/               # IEndpoint + reflection-based auto-registration
├── frontend/                     # React 19 + TypeScript + Vite SPA
│   └── src/
│       ├── components/             # Navbar, Hero, CategoryFilters, EventCard, AuthModal, Pagination
│       ├── api.ts                   # typed fetch wrapper for the whole REST surface
│       ├── CommentHub.ts             # SignalR client wrapper
│       └── App.tsx                    # view-state driven SPA (explore/detail/create/edit/my-events/admin)
└── tests/
    ├── EventPlanner.UnitTests/        # handler tests with Moq'd repositories
    └── EventPlanner.IntegrationTests/  # WebApplicationFactory end-to-end checks
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

The database seeds itself on first run with four categories (Technology, Sports, Music, Art) and two accounts:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@eventplanner.com` | `Admin123!` |
| User | `user@eventplanner.com` | `User123!` |

**Running the pieces by hand** (without Aspire) also works, with one caveat: the frontend's dev proxy (`vite.config.ts`) reads `SERVER_HTTPS`/`SERVER_HTTP` to know where to forward `/api` and `/hubs` traffic, and those variables are normally injected by Aspire. If you run `npm run dev` standalone, export one of them yourself first:

```bash
# Terminal 1 — API
cd EventPlanner.Server
dotnet run   # listens on http://localhost:5513 and https://localhost:7570

# Terminal 2 — frontend
cd frontend
npm install
SERVER_HTTP=http://localhost:5513 npm run dev   # http://localhost:5173
```

## Tests

```bash
dotnet test
```

Runs the xUnit unit tests (handlers exercised against mocked repositories) and the integration tests (a real `WebApplicationFactory<Program>` hitting the API surface).

## Team

Built as a group project for the Web Services course at TH Rosenheim.

- **Didarul Alam Sourov** ([@dasourov](https://github.com/dasourov)) — project setup, backend architecture, auth, frontend integration
- **Abhishek Bhardwaj** — bookings & comments, error handling, unit tests
- **Wiem** — API documentation & exception handling
- **Jeel Sidpara** — event filtering, search & pagination
