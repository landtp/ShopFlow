# ShopFlow — E-Commerce Microservices

A production-ready e-commerce order system built with .NET 10,
demonstrating Clean Architecture, DDD, CQRS, Event-Driven Architecture,
and cloud-native patterns.

## Architecture Overview
```
Client → API Gateway (YARP) → Identity Service
                             → Order Service
                             → Payment Service
```
```
Order Service → Kafka (order-created) → Payment Service
Payment Service → Kafka (payment-completed/failed) → Order Service
```

## Tech Stack

| Category | Technology |
|---|---|
| Language | C# 12 / .NET 10 |
| Architecture | Clean Architecture + DDD + CQRS |
| Messaging | Apache Kafka 3.7 |
| Cache | Redis 7 |
| Database | SQL Server 2022 |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Auth | JWT + BCrypt + Refresh Token Rotation |
| ORM | Entity Framework Core 10 |
| Validation | FluentValidation |
| Mediator | MediatR 12 |
| Logging | Serilog |
| Container | Docker + Docker Compose |

## Services

### Identity Service (:5002)
- User registration + login
- JWT access token (15 min) + refresh token rotation (7 days)
- Token blacklist via Redis
- BCrypt password hashing (WorkFactor 12)

### Order Service (:5001)
- Create / Get / Cancel orders
- DDD Aggregate Root with domain events
- Outbox Pattern — guaranteed event delivery
- Redis Cache-Aside for order queries
- Optimistic Locking (RowVersion) for concurrency
- Choreography Saga — listens to payment results

### Payment Service (:5004)
- Consumes OrderCreated events from Kafka
- Idempotency pattern — prevents duplicate payments
- Publishes PaymentCompleted / PaymentFailed events
- Choreography Saga — triggers order status update

### API Gateway (:7000)
- Single entry point for all clients
- JWT validation + authorization policies
- Injects X-User-Id header for downstream services
- Route-based forwarding via YARP

## Design Patterns
```
Outbox Pattern         — Save event to DB + publish async
                         Guarantees no event loss if Kafka is down

Choreography Saga      — Distributed transaction without 2PC
                         Order → Payment → Order (confirm/cancel)

Idempotency Pattern    — ProcessedMessages table
                         Kafka at-least-once safe

Cache-Aside Pattern    — Check Redis → miss → DB → write cache
                         Invalidate on mutation

Optimistic Locking     — EF Core RowVersion (byte[])
                         Prevents oversell race condition

Domain Events          — Raised in Aggregate, dispatched after SaveChanges
                         Decouples domain from infrastructure
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker Desktop
- SQL Server 2022

### Run Infrastructure
```bash
docker-compose -f docker-compose.infra.yml up -d
```

This starts:
- Apache Kafka + Kafka UI (http://localhost:8080)
- Redis + Redis Commander (http://localhost:8081)

### Run Services
```bash
# Terminal 1
dotnet run --project src/Services/Identity/IdentityService.API

# Terminal 2
dotnet run --project src/Services/Order/OrderService.API

# Terminal 3
dotnet run --project src/Services/Payment/PaymentService.API

# Terminal 4
dotnet run --project src/ApiGateway
```

### Run Full Stack (Docker)
```bash
docker-compose up -d --build
```

## API Endpoints

### Auth
```
POST /api/v1/auth/register   — Register new user
POST /api/v1/auth/login      — Login, returns JWT
POST /api/v1/auth/refresh    — Refresh access token
POST /api/v1/auth/logout     — Blacklist token (requires JWT)
```

### Orders (requires JWT)
```
POST   /api/v1/orders        — Create order
GET    /api/v1/orders        — Get orders (cached)
GET    /api/v1/orders/{id}   — Get order by id
DELETE /api/v1/orders/{id}   — Cancel order
```

## Event Flow
```
1. POST /api/v1/orders
   → Order created (status: Pending)
   → DomainEvent raised
   → OutboxMessage saved (same transaction)

2. OutboxProcessor (every 5s)
   → Reads unprocessed OutboxMessages
   → Publishes to Kafka topic: order-created
   → Marks message as processed

3. Payment Service
   → Consumes order-created
   → Idempotency check
   → Processes payment (simulate Stripe)
   → Publishes payment-completed OR payment-failed

4. Order Service
   → Consumes payment-completed → Order: Confirmed
   → Consumes payment-failed    → Order: Cancelled (compensating)
```

## Project Structure
```
ShopFlow/
├── src/
│   ├── BuildingBlocks/          # Shared: events, messaging, logging
│   ├── ApiGateway/              # YARP reverse proxy
│   └── Services/
│       ├── Identity/            # Auth service
│       │   ├── Domain/
│       │   ├── Application/
│       │   ├── Infrastructure/
│       │   └── API/
│       ├── Order/               # Order service
│       │   ├── Domain/
│       │   ├── Application/
│       │   ├── Infrastructure/
│       │   └── API/
│       └── Payment/             # Payment service
│           ├── Domain/
│           ├── Application/
│           ├── Infrastructure/
│           └── API/
├── docker-compose.yml           # Full stack
├── docker-compose.infra.yml     # Infrastructure only
└── README.md
```

## Key Decisions & Trade-offs

### Why Choreography Saga over Orchestration?
Services are simple with few steps — choreography keeps them
decoupled. Orchestration would be better if flow exceeded 5+ steps
or required complex compensation logic.

### Why YARP over Nginx?
Team is .NET-focused. YARP allows custom C# middleware for JWT
injection and header manipulation. Trade-off: ~30% lower throughput
vs Nginx at high load.

### Why Outbox Pattern?
Direct Kafka publish in DomainEventHandler risks event loss if Kafka
is temporarily down. Outbox saves event to DB in same transaction,
background processor retries until successful.

### Why Optimistic over Pessimistic Locking?
Low conflict frequency for inventory — optimistic locking avoids
blocking threads. Pessimistic locking would be better for
high-contention scenarios like flash sales.