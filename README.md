# PMS Backend - Parking Management System

A microservices-based parking management system built with .NET 8.0, featuring event-driven architecture, Docker Swarm deployment, and comprehensive monitoring.

## Overview

PMS Backend is a production-ready microservices system for managing parking facilities. It provides hierarchical site management, ticket booking, and automated invoice generation with external ERP integration.

### Key Features

- Hierarchical parking site management with geographic boundaries
- Real-time ticket booking system
- Automatic invoice generation and ERB/ERP integration
- Event-driven communication between services
- Comprehensive logging and monitoring with ELK stack
- Docker Swarm orchestration for high availability
- CI/CD pipeline with Jenkins

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Applications                       │
└──────────────┬──────────────┬──────────────┬───────────────────┘
               │              │              │
               ▼              ▼              ▼
        ┌──────────┐   ┌──────────┐   ┌──────────┐
        │   Site   │   │ Booking  │   │ Invoice  │
        │   API    │   │   API    │   │   API    │
        │  :5003   │   │  :5001   │   │  :5002   │
        └─────┬────┘   └─────┬────┘   └─────┬────┘
              │              │              │
              │    ┌─────────▼──────────────▼─────┐
              │    │     Apache Kafka :9092        │
              │    │  (Event Message Bus)          │
              │    └─────────▲──────────────────┬──┘
              │              │                  │
              └──────────────┴──────────────────┘
                             │
                      ┌──────▼───────┐
                      │  SQL Server  │
                      │    :1433     │
                      │              │
                      │ ┌──────────┐ │
                      │ │PMS_Site  │ │
                      │ │PMS_Booking│ │
                      │ │PMS_Invoice│ │
                      │ └──────────┘ │
                      └──────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Monitoring & Logging Stack                    │
│  ┌─────────┐  ┌──────────────┐  ┌────────┐  ┌──────────────┐  │
│  │Filebeat │─▶│Elasticsearch │◀─│ Kibana │  │  Kafka UI    │  │
│  │         │  │    :9200     │  │ :5601  │  │    :8090     │  │
│  └─────────┘  └──────────────┘  └────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Design Patterns

- **Microservices Architecture**: Three independent services with dedicated databases
- **Event-Driven Architecture**: Async communication via Kafka
- **Repository Pattern**: Data access abstraction
- **Unit of Work Pattern**: Transaction management with event publishing
- **Clean Architecture**: Separation of concerns (API, Application, Infrastructure, Model)
- **Saga Pattern**: Distributed transactions through event choreography

## Technology Stack

### Backend

- **.NET 8.0** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0.22** - ORM with Code-First migrations
- **FluentValidation 11.3.1** - Input validation

### Messaging & Events

- **Apache Kafka 7.6.0** (Confluent Platform) - Message broker
- **Zookeeper 7.6.0** - Kafka coordination
- **Confluent.Kafka** - .NET Kafka client

### Data Storage

- **SQL Server 2022** - Relational database
- **Three separate databases**: PMS_Site, PMS_Booking, PMS_Invoice

### Logging & Monitoring

- **Serilog** - Structured logging
- **Elasticsearch 8.11.0** - Log storage and search
- **Kibana 8.11.0** - Log visualization and dashboards
- **Filebeat 8.11.0** - Log shipping agent
- **Kafka UI** - Kafka topic and message monitoring

### DevOps

- **Docker & Docker Compose** - Containerization
- **Docker Swarm** - Container orchestration
- **Jenkins** - CI/CD automation
- **Swagger/OpenAPI** - API documentation

## Services

### 1. Site Service (Port 5003)

Manages hierarchical parking site structure with geographic boundaries.

**Responsibilities:**
- Create and manage parent sites (e.g., buildings, malls)
- Create leaf sites (actual parking zones) with polygon boundaries
- Maintain site hierarchy with path-based organization
- Publish site creation events

**Database:** PMS_Site

### 2. Booking Service (Port 5001)

Handles ticket booking and maintains local site data.

**Responsibilities:**
- Create parking tickets for customers
- Sync site data from Site Service via Kafka
- Calculate booking duration and pricing
- Publish booking creation events

**Database:** PMS_Booking

### 3. Invoice Service (Port 5002)

Generates invoices and integrates with external ERB/ERP system.

**Responsibilities:**
- Auto-generate invoices from bookings
- Create HTML invoice documents
- Calculate taxes (10% default)
- Send invoices to external ERB system
- Azure OneDrive integration

**Database:** PMS_Invoice

## Getting Started

### Prerequisites

- **Docker Desktop** (with Docker Compose)
- **.NET 8.0 SDK** (for local development)
- **Git**
- **Docker Swarm** (for production deployment)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PMS-Backend-main
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Verify services are running**
   ```bash
   docker-compose ps
   ```

4. **Access the services**
   - Site API: http://localhost:5003/swagger
   - Booking API: http://localhost:5001/swagger
   - Invoice API: http://localhost:5002/swagger
   - Kibana (Logs): http://localhost:5601
   - Kafka UI: http://localhost:8090

5. **Initialize databases** (automatic on first startup)
   Migrations run automatically when services start.

### Production Deployment (Docker Swarm)

1. **Initialize Docker Swarm**
   ```bash
   docker swarm init
   ```

2. **Create secrets**
   ```bash
   echo "YourStrongPassword123!" | docker secret create sa_password -
   ```

3. **Set environment variables** (create `.env` file)
   ```bash
   CLIENT_ID=your_azure_client_id
   CLIENT_SECRET=your_azure_client_secret
   TENANT_ID=your_azure_tenant_id
   ONEDRIVE_FOLDER=your_folder_path
   ```

4. **Deploy the stack**
   ```bash
   docker stack deploy -c docker-compose.swarm.yml pms-backend
   ```

5. **Verify deployment**
   ```bash
   docker stack services pms-backend
   docker service logs pms-backend_booking-service
   ```

6. **Run migrations**
   ```bash
   chmod +x run-migrations.sh
   ./run-migrations.sh
   ```

## API Documentation

### Site Service API

#### Create Parent Site
```http
POST /api/site/add/parent
Content-Type: application/json

{
  "nameEn": "Main Building",
  "nameAr": "المبنى الرئيسي",
  "integrationCode": "MB001",
  "parentId": null
}
```

#### Create Leaf Site (Parking Zone)
```http
POST /api/site/add/leaf
Content-Type: application/json

{
  "nameEn": "Zone A - Level 1",
  "nameAr": "المنطقة أ - المستوى 1",
  "pricePerHour": 10.5,
  "numberOfSolts": 50,
  "integrationCode": "ZA-L1",
  "parentId": "parent-site-guid",
  "polygons": [
    {
      "name": "Parking Area",
      "polygonPoints": [
        {"latitude": 30.0444, "longitude": 31.2357},
        {"latitude": 30.0445, "longitude": 31.2358},
        {"latitude": 30.0446, "longitude": 31.2359}
      ]
    }
  ]
}
```

#### Get All Root Sites
```http
GET /api/site/roots
```

#### Get All Leaf Sites (Parking Zones)
```http
GET /api/site/leaves
```

#### Get Child Sites
```http
GET /api/site/children/{parentId}
```

### Booking Service API

#### Create Booking
```http
POST /api/booking
Content-Type: application/json

{
  "siteId": "site-guid",
  "plateNumber": "ABC-1234",
  "phoneNumber": "+201234567890",
  "bookingFrom": "2026-01-01T10:00:00Z",
  "bookingTo": "2026-01-01T14:00:00Z"
}
```

#### Get Available Parking Sites
```http
GET /api/booking/leaves
```

#### Health Check
```http
GET /api/booking/health
```

### Invoice Service API

#### Create Invoice Manually
```http
POST /api/invoice
Content-Type: application/json

{
  "ticketId": "ticket-guid"
}
```

#### Health Check
```http
GET /api/invoice/health
```

### Response Format

**Success Response:**
```json
{
  "statusCode": 200,
  "data": { /* result object */ },
  "message": "Success",
  "isSuccess": true
}
```

**Error Response:**
```json
{
  "statusCode": 400,
  "data": null,
  "message": "Validation failed",
  "isSuccess": false
}
```

## Database Schema

### Site Service Database (PMS_Site)

```sql
-- Sites (Hierarchical)
Table: Sites
- Id (uniqueidentifier, PK)
- Path (nvarchar(max))
- NameEn (nvarchar(100))
- NameAr (nvarchar(100))
- PricePerHour (decimal(18,2), nullable)
- IntegrationCode (nvarchar(50), nullable, unique)
- NumberOfSolts (int, nullable)
- IsLeaf (bit)
- ParentId (uniqueidentifier, nullable, FK -> Sites.Id)

-- Polygons (Geographic boundaries)
Table: Polygons
- Id (uniqueidentifier, PK)
- Name (nvarchar(100))
- SiteId (uniqueidentifier, FK -> Sites.Id)

-- Polygon Points (Coordinates)
Table: PolygonPoints
- PolygonId (uniqueidentifier, PK, FK -> Polygons.Id)
- Latitude (decimal(18,10))
- Longitude (decimal(18,10))
```

### Booking Service Database (PMS_Booking)

```sql
-- Tickets (Booking records)
Table: Tickets
- Id (uniqueidentifier, PK)
- SiteName (nvarchar(100))
- PlateNumber (nvarchar(50))
- PhoneNumber (nvarchar(20))
- BookingFrom (datetime2)
- BookingTo (datetime2)
- TotalPrice (decimal(18,2))
- SiteId (uniqueidentifier, FK -> Sites.Id)

-- Sites (Replica from Site Service)
Table: Sites
- Id (uniqueidentifier, PK)
- NameEn (nvarchar(100))
- NameAr (nvarchar(100))
- Path (nvarchar(max))
- PricePerHour (decimal(18,2), nullable)
- IntegrationCode (nvarchar(50), nullable)
- NumberOfSolts (int, nullable)
- IsLeaf (bit)
```

### Invoice Service Database (PMS_Invoice)

```sql
-- Invoices
Table: Invoices
- Id (uniqueidentifier, PK)
- HtmlDocument (nvarchar(max))
- TaxAmount (decimal(18,2))
- TotalAmountBeforeTax (decimal(18,2))
- TotalAmountAfterTax (decimal(18,2))
- TicketSerialNumber (nvarchar(9))
- TicketId (uniqueidentifier, FK -> Tickets.Id)

-- Tickets (Replica from Booking Service)
Table: Tickets
- Id (uniqueidentifier, PK)
- SiteName (nvarchar(100))
- PlateNumber (nvarchar(50))
- PhoneNumber (nvarchar(20))
- BookingFrom (datetime2)
- BookingTo (datetime2)
- TotalPrice (decimal(18,2))
```

## Event-Driven Architecture

### Event Flow

```
┌──────────────┐
│ Site Service │
└──────┬───────┘
       │ Creates Site
       │
       ▼
  ┌─────────────────────┐
  │ SiteCreatedEvent    │ ──▶ Kafka Topic: SiteCreatedEvent
  └─────────────────────┘
       │
       ▼
┌──────────────────┐
│ Booking Service  │
└──────┬───────────┘
       │ Stores Site Locally
       │ Customer Creates Booking
       │
       ▼
  ┌─────────────────────┐
  │ BookingCreatedEvent │ ──▶ Kafka Topic: BookingCreatedEvent
  └─────────────────────┘
       │
       ▼
┌──────────────────┐
│ Invoice Service  │
└──────────────────┘
       │ Generates Invoice
       │ Sends to ERB System
       ▼
```

### Integration Events

#### SiteCreatedEvent
Published by: **Site Service**
Consumed by: **Booking Service**

```json
{
  "siteId": "guid",
  "nameEn": "string",
  "nameAr": "string",
  "path": "string",
  "pricePerHour": 10.5,
  "integrationCode": "string",
  "numberOfSolts": 50,
  "isLeaf": true
}
```

#### BookingCreatedEvent
Published by: **Booking Service**
Consumed by: **Invoice Service**

```json
{
  "ticketId": "guid",
  "siteName": "string",
  "plateNumber": "string",
  "phoneNumber": "string",
  "bookingFrom": "2026-01-01T10:00:00Z",
  "bookingTo": "2026-01-01T14:00:00Z",
  "totalPrice": 42.0
}
```

### Event Publishing Pattern

The system uses an **Outbox Pattern** variant to ensure transactional consistency:

1. Business logic enqueues events in memory (`IntegrationEventQueue`)
2. `UOW.SaveChangesAsync()` commits database transaction
3. After successful commit, events are published to Kafka
4. Events are published only if database transaction succeeds

```csharp
// Example: Creating a site with event publishing
await _siteRepo.AddAsync(site);
_eventQueue.EnqueueEvent(new SiteCreatedEvent(site));
await _uow.SaveChangesAsync(); // Commits DB + Publishes events
```

## Deployment

### Docker Swarm Configuration

**Stack Name:** pms-backend

#### Service Replicas

| Service | Replicas | Memory | CPU | Placement |
|---------|----------|--------|-----|-----------|
| Site API | 2 | 256MB | 0.5 | Any |
| Booking API | 2 | 256MB | 0.5 | Any |
| Invoice API | 2 | 256MB | 0.5 | Any |
| SQL Server | 1 | 2GB | 2.0 | Manager |
| Kafka | 1 | 512MB | 1.0 | Manager |
| Zookeeper | 1 | 256MB | 0.5 | Manager |
| Elasticsearch | 1 | 512MB | 1.0 | Manager |
| Kibana | 1 | 512MB | 0.5 | Manager |
| Filebeat | Global | - | - | All nodes |
| Kafka UI | 1 | 256MB | 0.5 | Manager |

#### Update Strategy

- **Parallelism:** 1 (one service at a time)
- **Order:** start-first (zero downtime)
- **Failure Action:** rollback
- **Monitor:** 30s

#### Health Checks

All services have health checks configured:
- **API Services:** TCP check on port 8080
- **SQL Server:** `sqlcmd` query execution
- **Kafka:** Broker API version check
- **Elasticsearch:** Cluster health API
- **Kibana:** Status API

### CI/CD Pipeline (Jenkins)

**Pipeline Stages:**

1. **Checkout** - Clone from GitHub (`main` branch)
2. **Verify Docker Swarm** - Ensure Swarm mode is active
3. **Docker Login** - Authenticate with Docker Hub
4. **Build Images** - Build all service images with build number tags
5. **Push Images** - Push to Docker registry (optional)
6. **Deploy Stack** - Deploy to Swarm using `docker-compose.swarm.yml`
7. **Health Check** - Wait 240s, verify all services running
8. **Run Migrations** - Execute database setup script

**Jenkinsfile Location:** `./Jenkinsfile`

### Secrets Management

Create Docker secrets before deployment:

```bash
echo "YourStrongPassword123!" | docker secret create sa_password -
```

Secrets are mounted at `/run/secrets/` with mode `0400` (read-only).

## Monitoring

### Kibana (Logs & Analytics)

**URL:** http://localhost:5601

**Features:**
- Real-time log streaming from all services
- Search and filter logs by service, level, timestamp
- Create custom dashboards
- Set up alerts for errors

**Index Pattern:** `filebeat-*`

### Kafka UI

**URL:** http://localhost:8090

**Features:**
- View all Kafka topics
- Inspect messages in real-time
- Monitor consumer lag
- View broker health

**Topics:**
- `SiteCreatedEvent`
- `BookingCreatedEvent`

### Serilog Configuration

Each service logs to:
- **Console** (captured by Docker)
- **File** (`logs/` directory, daily rolling, 7-day retention)

**Log Enrichment:**
- Machine name
- Environment name
- Process ID, Thread ID
- Timestamp (UTC)

## Development

### Project Structure

```
PMS-Backend-main/
├── Site.API/                        # Site microservice
│   ├── Site.API/                    # API controllers
│   ├── Site.Application/            # Business logic
│   ├── Site.Infrastrcure.Persistent/ # EF Core, Migrations
│   └── Site.Model/                  # Domain entities
│
├── Booking.API/                     # Booking microservice
│   ├── Booking.API/
│   ├── Booking.Application/
│   ├── Booking.Infrastrcure.Persistent/
│   └── Booking.Model.Shared/
│
├── Invoice.API/                     # Invoice microservice
│   ├── Invoice.API/
│   ├── Invoice.Application/
│   ├── Invoice.Infrastrcure.Persistent/
│   └── Invoice.Model/
│
├── SharedKernel/                    # Shared libraries
│   ├── SharedKernel.EventDriven/
│   ├── SharedKernel.MessageBus.Kafka/
│   ├── SharedKernel.Infratsrucrure.Persistent/
│   ├── SharedKernel.Logging/
│   └── SharedKernel.Models.Enum/
│
├── ServiceTemplate/                 # Template for new services
│
├── docker-compose.yml               # Local development
├── docker-compose.swarm.yml         # Production deployment
├── Jenkinsfile                      # CI/CD pipeline
└── run-migrations.sh                # Database initialization
```

### Adding a New Service

1. **Copy ServiceTemplate**
   ```bash
   cp -r ServiceTemplate NewService.API
   ```

2. **Update namespaces and project names**

3. **Add to `docker-compose.yml`**

4. **Create database migrations**
   ```bash
   cd NewService.Infrastrcure.Persistent
   dotnet ef migrations add InitialCreate
   ```

5. **Implement business logic**

6. **Add to CI/CD pipeline**

### Running Locally (Without Docker)

1. **Start SQL Server**
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
     -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. **Start Kafka**
   ```bash
   docker-compose up -d kafka zookeeper
   ```

3. **Update appsettings.Development.json**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=PMS_Site;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
     },
     "Kafka": {
       "BootstrapServers": "localhost:9092"
     }
   }
   ```

4. **Run each service**
   ```bash
   cd Site.API/Site.API
   dotnet run

   cd Booking.API/Booking.API
   dotnet run

   cd Invoice.API/Invoice.API
   dotnet run
   ```

### Code Quality Tools

**FluentValidation Example:**

```csharp
public class CreateTicketDTOValidator : AbstractValidator<CreateTicketDTO>
{
    public CreateTicketDTOValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .Matches(@"^[A-Z]{3}-\d{4}$")
            .WithMessage("Plate number must be in format: ABC-1234");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.BookingTo)
            .GreaterThan(x => x.BookingFrom)
            .WithMessage("End time must be after start time");
    }
}
```

## Configuration

### Environment Variables

#### Database Configuration

```bash
# SQL Server Connection
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=PMS_Site;User Id=sa;Password=SECRET;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

#### Kafka Configuration

```bash
# Kafka Broker
Kafka__BootstrapServers=kafka:9092

# Producer Settings
Kafka__Producer__Acks=all
Kafka__Producer__MessageTimeoutMs=30000

# Consumer Settings
Kafka__Consumer__GroupId=BookingServiceGroup
Kafka__Consumer__EnableAutoCommit=false
Kafka__Consumer__AutoOffsetReset=earliest
```

#### Azure Configuration (Invoice Service)

```bash
# Azure Active Directory
CLIENT_ID=your_client_id
CLIENT_SECRET=your_client_secret
TENANT_ID=your_tenant_id

# OneDrive
ONEDRIVE_FOLDER=/Invoices
```

#### Logging Configuration

```bash
# Serilog
Serilog__MinimumLevel__Default=Information
Serilog__MinimumLevel__Override__Microsoft=Warning
```

#### ERB/ERP Integration

```bash
# ERB API (configured in code)
ERB_BASE_URL=http://localhost:8080/api
ERB_USERNAME=admin@gmail.com
ERB_PASSWORD=root
```

## Troubleshooting

### Services Not Starting

**Check service status:**
```bash
docker-compose ps
docker service ls  # For Swarm
```

**View logs:**
```bash
docker-compose logs booking-service
docker service logs pms-backend_booking-service  # For Swarm
```

**Common issues:**
- SQL Server not ready: Wait 30s and restart services
- Kafka connection failed: Ensure Kafka and Zookeeper are healthy
- Port conflicts: Check if ports 5001-5003 are available

### Database Connection Issues

**Test SQL Server connection:**
```bash
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourPassword123!' \
  -Q "SELECT @@VERSION"
```

**Reset databases:**
```bash
docker-compose down -v  # Removes volumes
docker-compose up -d
```

### Kafka Message Not Consumed

**Check Kafka UI:** http://localhost:8090
- Verify topic exists
- Check consumer group lag
- Inspect message format

**Manual consume test:**
```bash
docker exec -it kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic SiteCreatedEvent \
  --from-beginning
```

### Migration Errors

**Manually run migrations:**
```bash
cd Site.API/Site.Infrastrcure.Persistent
dotnet ef database update

cd Booking.API/Booking.Infrastrcure.Persistent
dotnet ef database update

cd Invoice.API/Invoice.Infrastrcure.Persistent
dotnet ef database update
```

### Performance Issues

**Check resource usage:**
```bash
docker stats
docker service ps pms-backend_booking-service  # For Swarm
```

**Scale services:**
```bash
docker service scale pms-backend_booking-service=4
```

### Logs Not Appearing in Kibana

1. **Check Filebeat status:**
   ```bash
   docker-compose logs filebeat
   ```

2. **Verify Elasticsearch:**
   ```bash
   curl http://localhost:9200/_cluster/health
   ```

3. **Check index pattern in Kibana:**
   - Go to Stack Management > Index Patterns
   - Create `filebeat-*` if missing

### ERB Integration Failing

**Check Invoice Service logs:**
```bash
docker-compose logs invoice-service | grep ERB
```

**Verify ERB API is accessible:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@gmail.com","password":"root"}'
```

## License

[Add your license information here]

## Contact

[Add contact information here]

## Contributing

[Add contribution guidelines here]

---

**Built with .NET 8.0 | Powered by Apache Kafka | Orchestrated with Docker Swarm**
