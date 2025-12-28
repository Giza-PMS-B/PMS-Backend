# DevOps Technical Documentation

This document provides comprehensive technical documentation for the DevOps infrastructure, deployment processes, and operational procedures for the PMS Backend system.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Infrastructure Components](#infrastructure-components)
- [Docker Swarm Deployment](#docker-swarm-deployment)
- [CI/CD Pipeline](#cicd-pipeline)
- [Service Configuration](#service-configuration)
- [Networking](#networking)
- [Secrets Management](#secrets-management)
- [Monitoring and Logging](#monitoring-and-logging)
- [Health Checks](#health-checks)
- [Resource Management](#resource-management)
- [Troubleshooting](#troubleshooting)

---

## Architecture Overview

The PMS Backend system is deployed as a microservices architecture using Docker Swarm for orchestration. The system consists of:

- **Application Services**: Booking, Invoice, and Site services (ASP.NET Core)
- **Message Broker**: Kafka with Zookeeper
- **Database**: SQL Server 2022
- **Logging Stack**: Elasticsearch, Kibana, and Filebeat
- **Monitoring**: Kafka UI

All services communicate through a shared overlay network and use Kafka for event-driven communication.

---

## Infrastructure Components

### Application Services

#### Booking Service
- **Image**: `omareldamaty/pms-booking-service:latest`
- **Port**: `5001:8080`
- **Database**: `PMS_Booking`
- **Kafka Client ID**: `BookingService`
- **Kafka Consumer Group**: `BookingServiceGroup`
- **Replicas**: 2
- **Resources**: 0.5 CPU / 256MB memory (limits), 0.25 CPU / 128MB memory (reservations)

#### Invoice Service
- **Image**: `omareldamaty/pms-invoice-service:latest`
- **Port**: `5002:8080`
- **Database**: `PMS_Invoice`
- **Kafka Client ID**: `InvoiceService`
- **Kafka Consumer Group**: `InvoiceServiceGroup`
- **Replicas**: 2
- **Resources**: 0.5 CPU / 256MB memory (limits), 0.25 CPU / 128MB memory (reservations)
- **Additional Config**: Azure credentials for OneDrive integration

#### Site Service
- **Image**: `omareldamaty/pms-site-service:latest`
- **Port**: `5003:8080`
- **Database**: `PMS_Site`
- **Kafka Client ID**: `SiteService`
- **Kafka Consumer Group**: `SiteServiceGroup`
- **Replicas**: 2
- **Resources**: 0.5 CPU / 256MB memory (limits), 0.25 CPU / 128MB memory (reservations)

### Message Broker Stack

#### Zookeeper
- **Image**: `confluentinc/cp-zookeeper:7.6.0`
- **Port**: `2181`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 0.5 CPU / 256MB memory (limits), 0.25 CPU / 128MB memory (reservations)
- **Health Check**: TCP port check on 2181
- **Purpose**: Coordination service for Kafka

#### Kafka
- **Image**: `confluentinc/cp-kafka:7.6.0`
- **Port**: `9092`
- **Hostname**: `kafka`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 1 CPU / 512MB memory (limits), 0.5 CPU / 256MB memory (reservations)
- **Health Check**: Kafka broker API versions check
- **Configuration**:
  - `KAFKA_BROKER_ID`: 1
  - `KAFKA_ZOOKEEPER_CONNECT`: zookeeper:2181
  - `KAFKA_ADVERTISED_LISTENERS`: PLAINTEXT://kafka:9092
  - `KAFKA_AUTO_CREATE_TOPICS_ENABLE`: true
  - `KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR`: 1

#### Kafka UI
- **Image**: `provectuslabs/kafka-ui:latest`
- **Port**: `8090:8080`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 0.5 CPU / 256MB memory (limits), 0.25 CPU / 128MB memory (reservations)
- **Purpose**: Web-based UI for Kafka cluster management and monitoring

### Database

#### SQL Server
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Port**: `1433`
- **Hostname**: `sqlserver`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 2 CPU / 2GB memory (limits), 1 CPU / 1.5GB memory (reservations)
- **Memory Limit**: 1536MB
- **Health Check**: SQL query check using sqlcmd
- **Databases**:
  - `PMS_Booking`
  - `PMS_Invoice`
  - `PMS_Site`
- **Authentication**: SA account with password from Docker secret

### Logging Stack

#### Elasticsearch
- **Image**: `docker.elastic.co/elasticsearch/elasticsearch:8.11.0`
- **Ports**: `9200` (HTTP), `9300` (Transport)
- **Hostname**: `elasticsearch`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 1 CPU / 512MB memory (limits), 0.5 CPU / 256MB memory (reservations)
- **Configuration**:
  - `discovery.type`: single-node
  - `xpack.security.enabled`: false
  - `ES_JAVA_OPTS`: -Xms256m -Xmx512m
- **Health Check**: Cluster health API check

#### Kibana
- **Image**: `docker.elastic.co/kibana/kibana:8.11.0`
- **Port**: `5601`
- **Hostname**: `kibana`
- **Replicas**: 1
- **Placement**: Manager node only
- **Resources**: 0.5 CPU / 512MB memory (limits), 0.25 CPU / 256MB memory (reservations)
- **Configuration**:
  - `ELASTICSEARCH_HOSTS`: http://elasticsearch:9200
- **Health Check**: API status check
- **Purpose**: Log visualization and analysis dashboard

#### Filebeat
- **Image**: `docker.elastic.co/beats/filebeat:8.11.0`
- **Hostname**: `filebeat`
- **Deployment Mode**: Global (runs on every node)
- **Resources**: 0.5 CPU / 512MB memory (limits), 0.25 CPU / 256MB memory (reservations)
- **Volumes**:
  - `/var/lib/docker/containers`: Read-only access to container logs
  - `/var/run/docker.sock`: Read-only access to Docker socket
- **Configuration**: Managed via Docker config (`filebeat_config`)
- **Purpose**: Collects logs from all Docker containers and ships to Elasticsearch

---

## Docker Swarm Deployment

### Prerequisites

1. **Initialize Docker Swarm**:
   ```bash
   docker swarm init
   ```

2. **Create Overlay Network**:
   ```bash
   docker network create --driver overlay --attachable pms-network
   ```

3. **Create Docker Secret**:
   ```bash
   echo "YourStrong@Passw0rd" | docker secret create sa_password -
   ```

4. **Create Filebeat Config**:
   ```bash
   docker config create filebeat_config filebeat.yml
   ```

5. **Build and Push Images** (if not using CI/CD):
   ```bash
   docker build -f Booking.API/Dockerfile -t omareldamaty/pms-booking-service:latest .
   docker build -f Invoice.API/Dockerfile -t omareldamaty/pms-invoice-service:latest .
   docker build -f Site.API/Dockerfile -t omareldamaty/pms-site-service:latest .
   
   docker push omareldamaty/pms-booking-service:latest
   docker push omareldamaty/pms-invoice-service:latest
   docker push omareldamaty/pms-site-service:latest
   ```

### Deployment Commands

**Deploy Stack**:
```bash
docker stack deploy -c docker-compose.swarm.yml pms-backend
```

**Deploy with Custom Image Tag**:
```bash
export IMAGE_TAG=v1.0.0
docker stack deploy -c docker-compose.swarm.yml pms-backend
```

**Update Stack**:
```bash
# After modifying docker-compose.swarm.yml
docker stack deploy -c docker-compose.swarm.yml pms-backend
```

**Remove Stack**:
```bash
docker stack rm pms-backend
```

### Service Management

**List Services**:
```bash
docker stack services pms-backend
```

**List Service Tasks**:
```bash
docker service ps pms-backend_booking-service
```

**Scale Service**:
```bash
docker service scale pms-backend_booking-service=3
```

**Update Service**:
```bash
docker service update --force pms-backend_booking-service
```

**View Service Logs**:
```bash
docker service logs pms-backend_booking-service --tail 100 -f
```

---

## CI/CD Pipeline

### Jenkins Pipeline Overview

The CI/CD pipeline is defined in `Jenkinsfile` and includes the following stages:

1. **Checkout**: Clones the repository from GitHub
2. **Verify Docker Swarm**: Ensures Swarm is active
3. **Docker Login**: Authenticates with Docker Hub
4. **Build Images**: Builds all three service images
5. **Deploy Stack**: Deploys the stack to Swarm
6. **Health Check**: Verifies all services are running
7. **Run Migrations**: Executes database migrations

### Pipeline Configuration

**Environment Variables**:
- `STACK_NAME`: `pms-backend`
- `DOCKER_REPO`: `omareldamaty`
- `IMAGE_TAG`: `${BUILD_NUMBER}`

**Required Jenkins Credentials**:
- `swarm-id`: GitHub credentials for repository access
- `Docker-PAT`: Docker Hub Personal Access Token

### Pipeline Stages

#### Stage 1: Checkout Backend Repo
- Clones from `https://github.com/Giza-PMS-B/PMS-Backend`
- Branch: `main`
- Uses credentials: `swarm-id`

#### Stage 2: Verify Docker Swarm
- Checks if Docker Swarm is active
- Fails pipeline if Swarm is not initialized

#### Stage 3: Docker Login
- Authenticates with Docker Hub using PAT
- Required for pushing images (currently commented out)

#### Stage 4: Build Backend Images
- Builds all three service images
- Tags images with build number and `latest`
- Images:
  - `omareldamaty/pms-booking-service:${BUILD_NUMBER}`
  - `omareldamaty/pms-invoice-service:${BUILD_NUMBER}`
  - `omareldamaty/pms-site-service:${BUILD_NUMBER}`

#### Stage 5: Deploy Backend Stack
- Deploys stack using `docker-compose.swarm.yml`
- Sets `IMAGE_TAG` environment variable

#### Stage 6: Health Check
- Waits 240 seconds for services to stabilize
- Checks service replicas (retries up to 5 times)
- Fails if services don't become healthy

#### Stage 7: Run Database Migrations
- Waits 20 seconds for SQL Server readiness
- Executes `run-migrations.sh` script
- Creates databases and triggers migrations

### Post-Build Actions

**Always**:
- Logs out from Docker Hub

**On Success**:
- Logs deployment success message

**On Failure**:
- Displays service status
- Shows last 50 lines of logs for each service

---

## Service Configuration

### Environment Variables

All .NET services share common environment variables:

```yaml
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_HTTP_PORTS: 8080
Kafka__BootstrapServers: kafka:9092
Kafka__Producer__Acks: all
Kafka__Producer__MessageTimeoutMs: "30000"
Kafka__Consumer__EnableAutoCommit: "false"
Kafka__Consumer__AutoOffsetReset: earliest
```

### Service-Specific Configuration

Each service has unique configuration:

**Booking Service**:
```yaml
Kafka__ClientId: BookingService
Kafka__Consumer__GroupId: BookingServiceGroup
ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=PMS_Booking;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

**Invoice Service**:
```yaml
Kafka__ClientId: InvoiceService
Kafka__Consumer__GroupId: InvoiceServiceGroup
ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=PMS_Invoice;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
CLIENT_ID: ${CLIENT_ID}
CLIENT_SECRET: ${CLIENT_SECRET}
TENANT_ID: ${TENANT_ID}
ONEDRIVE_FOLDER: ${ONEDRIVE_FOLDER}
```

**Site Service**:
```yaml
Kafka__ClientId: SiteService
Kafka__Consumer__GroupId: SiteServiceGroup
ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=PMS_Site;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

### Startup Behavior

All application services include a startup script that:

1. Waits for Kafka and SQL Server DNS resolution (up to 30 attempts)
2. Checks if `kafka` and `sqlserver` hostnames are resolvable
3. Starts the .NET application once dependencies are available

This ensures services don't start before their dependencies are ready.

---

## Networking

### Network Architecture

**Network Name**: `pms-network`
**Type**: Overlay network (required for Docker Swarm)
**Driver**: Overlay
**Attachable**: Yes

### Service Discovery

Services communicate using Docker Swarm's built-in DNS:

- **Kafka**: `kafka:9092`
- **SQL Server**: `sqlserver:1433`
- **Elasticsearch**: `elasticsearch:9200`
- **Kibana**: `kibana:5601`

### Port Mapping

**External Access**:
- Booking Service: `5001`
- Invoice Service: `5002`
- Site Service: `5003`
- Kafka: `9092`
- SQL Server: `1433`
- Kafka UI: `8090`
- Elasticsearch: `9200`
- Kibana: `5601`

**Internal Ports**:
- All .NET services: `8080`
- SQL Server: `1433`
- Kafka: `9092`
- Zookeeper: `2181`

---

## Secrets Management

### Docker Secrets

**SA Password Secret**:
- **Name**: `sa_password`
- **Type**: External secret
- **Usage**: SQL Server authentication
- **Mount Path**: `/run/secrets/sa_password`
- **Permissions**: 0400 (read-only for owner)

### Creating Secrets

**Create SA Password Secret**:
```bash
echo "YourStrong@Passw0rd" | docker secret create sa_password -
```

**List Secrets**:
```bash
docker secret ls
```

**Inspect Secret**:
```bash
docker secret inspect sa_password
```

**Remove Secret**:
```bash
docker secret rm sa_password
```

**Note**: Secrets cannot be removed while services are using them.

### Secret Usage in Services

All application services mount the `sa_password` secret:
- **Source**: `sa_password`
- **Target**: `sa_password`
- **UID/GID**: `1000`
- **Mode**: `0400`

SQL Server uses the secret via environment variable:
```yaml
MSSQL_SA_PASSWORD_FILE: /run/secrets/sa_password
```

---

## Monitoring and Logging

### Log Collection Architecture

1. **Filebeat** collects logs from all Docker containers
2. **Elasticsearch** stores logs in daily indices (`filebeat-YYYY.MM.DD`)
3. **Kibana** provides visualization and search interface

### Accessing Logs

**Kibana Dashboard**:
- URL: `http://localhost:5601`
- Navigate to "Discover" to view logs
- Filter by:
  - Container name
  - Service name
  - Log level
  - Time range

**Docker Service Logs**:
```bash
# View logs for a service
docker service logs pms-backend_booking-service --tail 100 -f

# View logs for all services
docker stack services pms-backend --format "{{.Name}}" | xargs -I {} docker service logs {} --tail 50
```

### Filebeat Configuration

Filebeat configuration is managed via Docker config:

**Create/Update Config**:
```bash
docker config create filebeat_config filebeat.yml
# or update existing
docker config rm filebeat_config
docker config create filebeat_config filebeat.yml
docker service update --force pms-backend_filebeat
```

**Filebeat Features**:
- Automatic Docker metadata enrichment
- Host metadata addition
- Daily index rotation
- Compression enabled
- 7-day log retention

### Monitoring Tools

**Kafka UI**:
- URL: `http://localhost:8090`
- Monitor topics, consumers, producers
- View message content
- Check consumer lag

**Service Health**:
```bash
# Check service status
docker stack services pms-backend

# Check service tasks
docker service ps pms-backend_booking-service

# Check service health
docker service inspect pms-backend_booking-service --pretty
```

---

## Health Checks

### Application Services

All application services use TCP port health checks:

```yaml
healthcheck:
  test: ["CMD-SHELL", "bash -c '</dev/tcp/localhost/8080' || exit 1"]
  interval: 30s
  timeout: 5s
  retries: 3
  start_period: 120s
```

**Health Check Details**:
- **Interval**: Check every 30 seconds
- **Timeout**: 5 seconds per check
- **Retries**: 3 consecutive failures before marking unhealthy
- **Start Period**: 120 seconds grace period on startup

### Infrastructure Services

**SQL Server**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$$(cat /run/secrets/sa_password)\" -C -Q \"SELECT 1\" || exit 1"]
  interval: 15s
  timeout: 10s
  retries: 10
  start_period: 60s
```

**Kafka**:
```yaml
healthcheck:
  test: ["CMD", "kafka-broker-api-versions", "--bootstrap-server", "localhost:9092"]
  interval: 15s
  timeout: 15s
  retries: 10
  start_period: 60s
```

**Zookeeper**:
```yaml
healthcheck:
  test: ["CMD", "nc", "-z", "localhost", "2181"]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 30s
```

**Elasticsearch**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost:9200/_cluster/health?wait_for_status=yellow&timeout=1s || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 60s
```

**Kibana**:
```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost:5601/api/status || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 120s
```

---

## Resource Management

### Resource Limits

All services have CPU and memory limits configured:

**Application Services**:
- **Limits**: 0.5 CPU / 256MB memory
- **Reservations**: 0.25 CPU / 128MB memory

**SQL Server**:
- **Limits**: 2 CPU / 2GB memory
- **Reservations**: 1 CPU / 1.5GB memory
- **Memory Limit**: 1536MB (via MSSQL_MEMORY_LIMIT_MB)

**Kafka**:
- **Limits**: 1 CPU / 512MB memory
- **Reservations**: 0.5 CPU / 256MB memory

**Elasticsearch**:
- **Limits**: 1 CPU / 512MB memory
- **Reservations**: 0.5 CPU / 256MB memory
- **Java Heap**: 256MB - 512MB

### Placement Constraints

Most services are constrained to manager nodes:
```yaml
placement:
  constraints:
    - node.role == manager
```

**Exceptions**:
- **Filebeat**: Global deployment (runs on all nodes)

### Restart Policies

**Application Services**:
```yaml
restart_policy:
  condition: any
  delay: 5s
  max_attempts: 0  # Unlimited
  window: 120s
```

**Infrastructure Services**:
```yaml
restart_policy:
  condition: on-failure
```

### Update Configuration

**Application Services**:
```yaml
update_config:
  parallelism: 1
  delay: 10s
  failure_action: rollback
  order: start-first
```

**Infrastructure Services**:
```yaml
update_config:
  parallelism: 1
  delay: 30s
  failure_action: rollback
```

---

## Troubleshooting

### Common Issues

#### Services Not Starting

**Symptoms**: Services show `0/2` replicas or keep restarting

**Diagnosis**:
```bash
# Check service status
docker service ps pms-backend_booking-service --no-trunc

# Check service logs
docker service logs pms-backend_booking-service --tail 100

# Check if dependencies are ready
docker service ps pms-backend_kafka
docker service ps pms-backend_sqlserver
```

**Solutions**:
- Verify Kafka and SQL Server are healthy
- Check DNS resolution (services wait up to 60 seconds)
- Review service logs for connection errors
- Verify network connectivity

#### Database Connection Issues

**Symptoms**: Services can't connect to SQL Server

**Diagnosis**:
```bash
# Test SQL Server connectivity
docker exec -it $(docker ps --filter "name=sqlserver" --format "{{.ID}}" | head -1) \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C -Q "SELECT 1"

# Check connection string
docker service inspect pms-backend_booking-service --format '{{range .Spec.TaskTemplate.ContainerSpec.Env}}{{println .}}{{end}}' | grep ConnectionStrings
```

**Solutions**:
- Verify SQL Server is running and healthy
- Check connection string format (should use `sqlserver,1433`)
- Ensure `TrustServerCertificate=True` is set
- Verify secret is mounted correctly

#### Kafka Connection Issues

**Symptoms**: Services can't connect to Kafka

**Diagnosis**:
```bash
# Check Kafka status
docker service ps pms-backend_kafka

# Test Kafka connectivity
docker exec -it $(docker ps --filter "name=kafka" --format "{{.ID}}" | head -1) \
  kafka-broker-api-versions --bootstrap-server localhost:9092

# Check Zookeeper
docker service ps pms-backend_zookeeper
```

**Solutions**:
- Verify Zookeeper is running (Kafka depends on it)
- Check Kafka health status
- Verify network connectivity
- Review Kafka logs for errors

#### Logs Not Appearing in Kibana

**Symptoms**: No logs visible in Kibana Discover

**Diagnosis**:
```bash
# Check Filebeat status
docker service ps pms-backend_filebeat

# Check Filebeat logs
docker service logs pms-backend_filebeat --tail 100

# Check Elasticsearch
curl http://localhost:9200/_cat/indices

# Verify Filebeat config
docker config inspect filebeat_config --pretty
```

**Solutions**:
- Verify Filebeat is running (global service)
- Check Elasticsearch is healthy
- Verify Filebeat config is correct
- Check Docker socket access permissions
- Wait 1-2 minutes for logs to appear

#### Migration Issues

**Symptoms**: Tables not created in databases

**Diagnosis**:
```bash
# Check if databases exist
docker exec -it $(docker ps --filter "name=sqlserver" --format "{{.ID}}" | head -1) \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "SELECT name FROM sys.databases WHERE name LIKE 'PMS_%'"

# Check migration history
docker exec -it $(docker ps --filter "name=sqlserver" --format "{{.ID}}" | head -1) \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -d PMS_Booking -Q "SELECT * FROM __EFMigrationsHistory"
```

**Solutions**:
- Run `run-migrations.sh` script
- Check service logs for migration errors
- Verify Entity Framework Core is configured correctly
- Ensure connection strings are correct

### Useful Commands

**View All Services**:
```bash
docker stack services pms-backend
```

**View Service Details**:
```bash
docker service inspect pms-backend_booking-service --pretty
```

**View Service Logs**:
```bash
docker service logs pms-backend_booking-service --tail 100 -f
```

**Check Service Health**:
```bash
docker service ps pms-backend_booking-service
```

**View Stack Status**:
```bash
docker stack ps pms-backend
```

**Check Network**:
```bash
docker network inspect pms-network
```

**View Secrets**:
```bash
docker secret ls
docker secret inspect sa_password
```

**View Configs**:
```bash
docker config ls
docker config inspect filebeat_config
```

**Restart Service**:
```bash
docker service update --force pms-backend_booking-service
```

**Scale Service**:
```bash
docker service scale pms-backend_booking-service=3
```

**Remove Service**:
```bash
docker service rm pms-backend_booking-service
```

**Remove Stack**:
```bash
docker stack rm pms-backend
```

---

## Best Practices

### Deployment

1. **Always verify Swarm is active** before deployment
2. **Check service health** after deployment
3. **Monitor logs** during initial startup
4. **Run migrations** after successful deployment
5. **Verify all services** are running before considering deployment complete

### Maintenance

1. **Update services** one at a time to minimize downtime
2. **Use rolling updates** (configured via `update_config`)
3. **Monitor resource usage** and adjust limits as needed
4. **Regularly check logs** for errors or warnings
5. **Backup databases** before major updates

### Security

1. **Use Docker secrets** for sensitive data (passwords, keys)
2. **Limit network exposure** (only expose necessary ports)
3. **Keep images updated** with security patches
4. **Review service logs** for security issues
5. **Use strong passwords** for database and secrets

### Monitoring

1. **Set up alerts** for service failures
2. **Monitor resource usage** (CPU, memory, disk)
3. **Track service health** via health checks
4. **Review logs regularly** in Kibana
5. **Monitor Kafka consumer lag** via Kafka UI

---

## Additional Resources

- **Docker Swarm Documentation**: https://docs.docker.com/engine/swarm/
- **Kafka Documentation**: https://kafka.apache.org/documentation/
- **Elasticsearch Documentation**: https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html
- **Kibana Documentation**: https://www.elastic.co/guide/en/kibana/current/index.html
- **Filebeat Documentation**: https://www.elastic.co/guide/en/beats/filebeat/current/index.html

