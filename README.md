# Trading Platform (WIP)

> **Work in progress** — A trading platform built with .NET 10, featuring portfolio management, order placement, and event-driven architecture with Kafka.

## Features

- **Portfolio Management** — Deposit and withdraw funds, view portfolio with holdings
- **Order Management** — Place, cancel, and query orders
- **Event-Driven** — Domain events published to Kafka for downstream processing
- **Clean Architecture** — Domain, Application, Infrastructure, and API layers
- **CQRS** — Commands and queries via MediatR

## Tech Stack

| Layer | Technologies |
|-------|--------------|
| API | ASP.NET Core 10, OpenAPI/Swagger |
| Application | MediatR, FluentValidation |
| Domain | Entities, Events, Interfaces |
| Infrastructure | Entity Framework Core 10, PostgreSQL, Kafka |
| Testing | xUnit, WebApplicationFactory |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for PostgreSQL, Kafka, Kafdrop)

## Getting Started

### 1. Start Infrastructure (Docker)

```bash
docker compose up -d
```

This starts:

| Service | Port | Description |
|---------|------|-------------|
| PostgreSQL | 5432 | Database |
| Kafka | 9092 | Event streaming |
| Kafdrop | 19000 | Kafka UI — http://localhost:19000 |

### 2. Run the API

```bash
dotnet run --project TradingPlatform.Api
```

The API runs at **http://localhost:5119**. Swagger UI is available at `/swagger` in Development.

### 3. Run Tests

```bash
# Unit tests
dotnet test TradingPlatform.UnitTests

# Integration tests (uses in-memory DB)
dotnet test TradingPlatform.IntegrationTests

# All tests
dotnet test
```

## API Endpoints

### Portfolio (`/api/portfolio`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/portfolio?userId={userId}` | Get portfolio by user ID |
| POST | `/api/portfolio/deposit` | Deposit funds (creates portfolio if needed) |
| POST | `/api/portfolio/withdraw` | Withdraw funds |

### Orders (`/api/orders`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders` | Place a new order |
| GET | `/api/orders/{orderId}` | Get order by ID |
| GET | `/api/orders?userId={userId}` | Get all orders for a user |
| DELETE | `/api/orders/{orderId}` | Cancel a pending order |

### Example Requests

**Deposit funds**
```json
POST /api/portfolio/deposit
{ "userId": "user-1", "amount": 1000 }
```

**Place order**
```json
POST /api/orders
{ "userId": "user-1", "symbol": "AAPL", "quantity": 10, "price": 150.50 }
```

## Project Structure

```
TradingPlatform-POC/
├── TradingPlatform.Api/           # Web API, controllers
├── TradingPlatform.Application/   # Commands, queries, DTOs, validators
├── TradingPlatform.Domain/        # Entities, events, interfaces
├── TradingPlatform.Infrastructure/# EF Core, repositories, Kafka
├── TradingPlatform.UnitTests/
├── TradingPlatform.IntegrationTests/
├── docker-compose.yml
└── .github/workflows/main.yml     # CI (build, test, publish)
```

## Configuration

Connection strings and settings are in `TradingPlatform.Api/appsettings.json`. The default PostgreSQL connection matches the Docker Compose setup:

- **Host:** localhost:5432  
- **Database:** TradingPlatform  
- **User/Password:** tradingplatform  

Integration tests use an in-memory database (`UseInMemoryDatabase: true`).

## Roadmap

- [ ] **Deploy to Kubernetes (k8s)** — Container orchestration and production deployment
- [ ] **Redis caching** — Cache frequently accessed data (portfolios, orders)
- [ ] **Notification service** — Real-time or async notifications (e.g. order fills, balance alerts)

## CI/CD

GitHub Actions runs on push/PR to `main`:

1. **Build** — Restore, build, publish
2. **Test** — Unit and integration tests

## License

MIT
