# ASP.Claims API - Insurance Claims Management System

## 📋 Overview

This project is a modern insurance claims management API built with **.NET 9**, **Azure Cosmos DB**, **Redis Pub/Sub**, and **.NET Aspire** for microservice orchestration. The system handles three types of claims: property, vehicle, and travel, with automated status evaluation, event-driven notifications, and comprehensive audit logging.

---

## 🏗️ System Architecture

The project consists of multiple services that communicate via **Redis Pub/Sub**:

```text
┌─────────────────────────────────────────────────────────────────┐
│                         ASP.Claims.API                          │
│  (Main Application - REST API with CQRS + Event Publishing)    │
└───────────┬─────────────────────────────────────┬───────────────┘
            │                                     │
            │ Publishes Events via Redis          │ HTTP Requests
            │                                     │
            ▼                                     ▼
    ┌───────────────┐                    ┌──────────────┐
    │  Redis Pub/Sub │                    │   Clients    │
    │   (Messaging)  │                    │ (Blazor, etc)│
    └───────┬────────┘                    └──────────────┘
            │
            │ Subscribes to Events
            │
    ┌───────┴──────────────────────┐
    │                              │
    ▼                              ▼
┌─────────────────────┐  ┌──────────────────────┐
│ NotificationsWorker │  │    AuditWorker       │
│  (Background Svc)   │  │  (Background Svc)    │
│                     │  │                      │
│ • Claim Escalated   │  │ • Logs all events    │
│ • Fraud Detected    │  │ • Stores audit trail │
│ • Sends Alerts      │  │ • Cosmos DB storage  │
└─────────────────────┘  └──────────────────────┘
```

### Services

1. **ASP.Claims.API** - Main REST API
   - CQRS with MediatR
   - JWT authentication
   - FluentValidation
   - Event publishing via Redis

2. **ASP.Claims.AuditWorker** - Audit and logging worker
   - Subscribes to all events
   - Stores audit logs in Cosmos DB
   - Tracks status changes, escalations, and fraud warnings

3. **ASP.Claims.NotificationsWorker** - Notification worker
   - Sends notifications for escalated claims
   - Fraud alerts
   - (Can be extended with email, SMS, push notifications)

4. **ASP.Claims.AppHost** - Aspire orchestration
   - Configures all services
   - Manages Redis, Cosmos DB
   - Development environment setup

---

## 📂 Project Structure

```text
ASP.Claims.sln
├── ASP.Claims.AppHost/                    # .NET Aspire orchestrator
│   ├── Program.cs                         # Aspire host configuration
│   └── appsettings.json                   # Orchestrator settings
│
├── ASP.Claims.API/                        # Main API project
│   ├── API/
│   │   ├── Controllers/                   # API controllers
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   ├── Validators/                    # FluentValidation classes
│   │   └── Resources/                     # Localization
│   ├── Application/
│   │   ├── CQRS/
│   │   │   └── Claims/
│   │   │       ├── Commands/              # Command objects
│   │   │       ├── Queries/               # Query objects
│   │   │       ├── CommandHandlers/       # Command handlers
│   │   │       └── QueryHandlers/         # Query handlers
│   │   ├── Interfaces/                    # Service interfaces
│   │   ├── Services/                      # Business logic
│   │   └── Profiles/                      # AutoMapper profiles
│   ├── Domain/
│   │   ├── Entities/                      # Domain models
│   │   ├── Enums/                         # Domain enums
│   │   └── Events/                        # Domain events
│   ├── Infrastructure/
│   │   ├── Repositories/                  # Data layer
│   │   └── Messaging/                     # Event publishing
│   ├── Middleware/                        # Middleware & filters
│   ├── Extensions/                        # Service extensions
│   ├── Settings/                          # Configuration classes
│   └── Program.cs                         # Application entry point
│
├── ASP.Claims.NotificationsWorker/        # Notification service
│   ├── Worker.cs                          # BackgroundService
│   └── Program.cs                         # Worker host
│
├── ASP.Claims.AuditWorker/                # Audit logging service
│   ├── Worker.cs                          # BackgroundService
│   ├── Models/                            # Audit models
│   ├── Repositories/                      # Audit repositories
│   └── Program.cs                         # Worker host
│
├── ASP.Claims.ServiceDefaults/            # Shared defaults
│   ├── Extensions.cs                      # OpenTelemetry, health
│   └── Events/                            # Shared event definitions
│       ├── ClaimEscalatedEvent.cs
│       ├── ClaimFraudFlaggedEvent.cs
│       └── ClaimStatusChangedEvent.cs
│
└── ASP.Claims.API.Test/                   # Test project
    ├── ControllerTests/
    ├── HandlerTests/
    └── RepositoryTests/
```

### Request Flow

```text
User
  ↓
[Validation Filter]
  ↓
API Controller (DTO)
  ↓
[Authorization]
  ↓
AutoMapper (DTO → Command)
  ↓
Application Layer (CQRS Handler)
  ↓
[Business Logic, Status Evaluation]
  ↓
Repository → Cosmos DB (Save Claim)
  ↓
[Event Publisher] → Redis (Fire-and-Forget)
  ↓
Background Workers (Subscribe to Events)
  ├─ NotificationsWorker → Send Notifications
  └─ AuditWorker → Persist Audit Logs
```

---

## 🔄 Event-Driven Architecture (Redis Pub/Sub)

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ASP.Claims.AppHost                       │
│                  (Aspire Orchestrator)                      │
└─────────────────────────────────────────────────────────────┘
         │
         ├─── Starts Redis Container (Docker)
         ├─── Starts API (connects to Redis)
         ├─── Starts NotificationsWorker (connects to Redis)
         └─── Starts AuditWorker (connects to Redis)

┌──────────────────────────────────────────────────────────────┐
│                      Redis Container                         │
│              (In-Memory Pub/Sub Message Bus)                 │
│                                                              │
│  Channels:                                                   │
│    • ClaimEscalatedEvent                                    │
│    • ClaimFraudFlaggedEvent                                 │
│    • ClaimStatusChangedEvent                                │
└──────────────────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │ Publish            │ Subscribe          │ Subscribe
         │                    │                    │
    ┌────┴────┐         ┌────┴────┐         ┌────┴────┐
    │   API   │         │ Notif.  │         │  Audit  │
    │         │         │ Worker  │         │ Worker  │
    └─────────┘         └─────────┘         └─────────┘
```

### Event Flow Example

**Scenario:** User creates a high-value property claim

```
1. User → POST /api/v1/claims/property (EstimatedDamageCost: $50,000)
         ↓
2. API → CreatePropertyClaimHandler
         ├─ Evaluates status → ClaimStatus.Escalated (high value)
         ├─ Saves to Cosmos DB ✅
         └─ Fires background task to publish events
               ↓
3. API → ClaimEventPublisher.PublishClaimEventsAsync
         ├─ Detects Escalated flag
         └─ Publishes ClaimEscalatedEvent to Redis
               ↓
4. Redis → Broadcasts message to channel "ClaimEscalatedEvent"
         ↓                           ↓
5a. NotificationsWorker       5b. AuditWorker
    ├─ Receives event             ├─ Receives event
    ├─ Logs notification          ├─ Saves to AuditDb (Cosmos)
    └─ (TODO: send email)         └─ Logs audit trail

6. User ← Returns 201 Created (API doesn't wait for workers)
```

**Timeline:**
- **0-200ms**: API saves claim, returns response
- **200-500ms**: Event published to Redis (async)
- **500ms+**: Workers process events independently

### Components

- **Redis**: In-memory pub/sub message bus for asynchronous event broadcasting
- **API**: Publishes domain events when claims are created or updated
- **NotificationsWorker**: Background service that sends notifications for escalated/fraud events
- **AuditWorker**: Background service that persists immutable audit logs to separate Cosmos DB

---

## 📚 Selected Libraries

### Asp.Versioning

**Purpose:** Handle and expose different versions of the API.

**Why:** Enables continuous development and backward compatibility, clear versioning in URLs and headers.

### MediatR

**Purpose:** Implements the CQRS pattern with commands, queries, and handlers.

**Why:** Clear separation of business logic and presentation, easy testing and maintenance.

### FluentValidation

**Purpose:** Define and apply validation rules on domain models in a clear and reusable way.

**Why:**
- Separate validation logic from models and UI
- Support for complex rules and cross-property validation
- Easy to test and maintain

### FluentResults

**Purpose:** Standardize the handling of results and errors from services and repositories.

**Why:**
- Clear separation between successful and failed operations without exceptions for control flow
- Facilitates error handling and presentation of error messages
- Support for sending error codes, messages, and reasons

### AutoMapper

**Purpose:** Simplify object-to-object mapping between DTOs, commands, and domain entities.

**Why:**
- Reduce boilerplate code
- Maintain clear separation between API contracts and domain models
- Easy to configure and test

### StackExchange.Redis

**Purpose:** High-performance Redis client for pub/sub messaging.

**Why:**
- Fast, in-memory message broker
- Simple pub/sub pattern for event-driven architecture
- Built-in connection pooling and retry logic

### .NET Aspire

**Purpose:** Cloud-native application orchestration and observability.

**Why:**
- Simplified local development with automatic service discovery
- Built-in OpenTelemetry for distributed tracing
- Dashboard for monitoring all services
- Easy container management (Redis, Cosmos DB Emulator)

---

## 🎯 Summary

This architecture enables:

• Clear separation of concerns (presentation, business logic, data, validation)  
• Event-driven, asynchronous processing with Redis pub/sub  
• Unified and testable error handling with FluentResults  
• Efficient and reusable validation with FluentValidation  
• Scalable and maintainable codebase for future development  
• Comprehensive observability with .NET Aspire and OpenTelemetry  
• Independent scaling of API and background workers  

---

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Docker Desktop (for Redis and optional Cosmos DB)
- Azure Cosmos DB Emulator or Azure Cosmos DB account
- Visual Studio 2022 or VS Code

### Running the Application

```powershell
# Clone the repository
git clone https://github.com/Hawkrin/A-hub-Claims-api-project

# Navigate to the solution
cd A-hub-Claims-api-project

# Start Cosmos DB Emulator (if using local)
.\scripts\Start-CosmosEmulator.ps1

# Run via Aspire (starts all services)
dotnet run --project ASP.Claims.AppHost
```

### Accessing Services

- **Aspire Dashboard**: http://localhost:15888
- **API (Scalar)**: http://localhost:5021/scalar/v1
- **API (Swagger)**: http://localhost:5021/swagger
- **Health Check**: http://localhost:5021/health

---

## 📖 Documentation

Additional documentation can be found in the repository:

- [Event-Driven Architecture Details](PUBSUB_REDIS_SUMMARY.md)
- [Production Deployment Guide](PRODUCTION_SETUP_GUIDE.md)
- [Cosmos DB Setup](COSMOS_STANDALONE_EMULATOR_GUIDE.md)
- [Aspire Configuration](ASPIRE_SETUP.md)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## 📄 License

This project is licensed under the MIT License.
