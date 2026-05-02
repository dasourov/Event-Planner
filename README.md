# Event Management Planner 🎉

Event Management Planner is a full-stack web application built with ASP.NET (C#) and a modern frontend, designed to help users create, manage, and join events.
This project demonstrates backend architecture, API design, authentication, and real-time-ready features.

## 🚀 Features
- 🔐 **User Authentication** (JWT-based)
- 📅 Create, update, and delete events
- 👥 Join and leave events
- 📋 View personal and public events
- ⚡ Scalable backend with clean architecture principles
- 🌐 Frontend UI for seamless interaction

## 🧱 Tech Stack
**Backend**
- ASP.NET Core (Minimal APIs)
- Entity Framework Core
- JWT Authentication
- ASP.NET Aspire (for orchestration)

**Frontend**
- Vite + TypeScript
- Modern UI

## 📂 Project Structure
```text
EventPlanner/
 ├── EventPlanner.AppHost     # Aspire orchestration
 ├── EventPlanner.Server      # Backend API
 │    ├── Endpoints/          # API route definitions
 │    ├── Services/           # Business logic
 │    ├── Models/             # Database entities
 │    ├── DTOs/               # Request/Response models
 │    ├── Data/               # DbContext
 │    ├── Interfaces/         # Contracts
 │    ├── Auth/               # JWT handling
 │    └── Middleware/         # Error handling
 │
 ├── frontend/                # Frontend application
```

## 🧠 Architecture Overview

The backend follows a layered architecture:

**Frontend → API Endpoints → Services → Data (EF Core) → Database**
- **Endpoints** handle HTTP requests (Minimal APIs with route groups)
- **Services** contain business logic
- **EF Core** manages database interactions

## 🌐 API Overview

### 🔹 Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### 🔹 Events
- `GET /api/events` → Get all events
- `GET /api/events/{id}` → Get event details
- `POST /api/events` → Create event
- `PUT /api/events/{id}` → Update event
- `DELETE /api/events/{id}` → Delete event

### 🔹 Bookings
- `POST /api/bookings/{eventId}/join`
- `DELETE /api/bookings/{eventId}/leave`
- `GET /api/bookings/my`

## 🗄️ Database Design

**Entities:**
- User
- Event
- Booking

**Relationships:**
- A user can create many events
- A user can join many events
- An event can have many participants

## ⚙️ Setup Instructions

**1️⃣ Clone the repository**
```bash
git clone https://github.com/dasourov/Event-Planner.git
cd Event-Planner
```

**2️⃣ Run Backend**
```bash
cd EventPlanner.Server
dotnet run
```

**3️⃣ Run Frontend**
```bash
cd frontend
npm install
npm run dev
```

**4️⃣ Access Application**
- **Frontend:** http://localhost:5173
- **API:** http://localhost:5000

## 🔐 Authentication

This project uses JWT (JSON Web Tokens) for securing endpoints.

Protected routes require:
```http
Authorization: Bearer <token>
```

## ⚙️ Optional Features Implemented
- ⚡ **SignalR** (planned for real-time updates)
- 🧠 **Caching** (planned with HybridCache)
- 🔄 **Background Services** (event reminders)

## 🎨 UI Workflow
`Home` → `Login/Register` → `Dashboard` → `Create Event` → `Join Event`

## 🧪 Future Improvements
- 🔔 Real-time notifications (SignalR)
- 📊 Event analytics dashboard
- 🌍 Location-based event filtering
- 📱 Mobile responsiveness improvements

## 👨‍💻 Authors
Developed as a university project

## 📜 License

This project is for educational purposes.

## ⭐ Final Note

This project demonstrates:
- Clean backend architecture
- RESTful API design
- Authentication & authorization
- Full-stack integration
