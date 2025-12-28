# User Scripts Documentation

This document provides detailed information about the utility scripts available in the PMS Backend project. These scripts help automate common tasks related to database management, logging configuration, and system verification.

## Table of Contents

- [Database Migration Script](#database-migration-script)
- [Filebeat Configuration Scripts](#filebeat-configuration-scripts)
- [Database Connection Testing](#database-connection-testing)

---

## Database Migration Script

### `run-migrations.sh`

**Purpose:** Creates PMS databases and triggers Entity Framework Core migrations for all services.

**Usage:**
```bash
./run-migrations.sh
```

**Prerequisites:**
- Docker Swarm stack must be deployed (`pms-backend`)
- SQL Server container must be running
- Services must be deployed and accessible

**What it does:**

1. **Verifies SQL Server Container**
   - Checks if SQL Server container is running
   - Exits with error if not found

2. **Waits for SQL Server Readiness**
   - Polls SQL Server every 2 seconds (up to 30 attempts)
   - Ensures database is ready to accept connections

3. **Creates Databases**
   - Creates the following databases if they don't exist:
     - `PMS_Booking`
     - `PMS_Invoice`
     - `PMS_Site`
   - Skips creation if database already exists

4. **Verifies Database Creation**
   - Lists all created databases with their creation dates

5. **Restarts Services**
   - Restarts the following services to trigger migrations:
     - `pms-backend_booking-service`
     - `pms-backend_invoice-service`
     - `pms-backend_site-service`

6. **Waits for Service Initialization**
   - Waits 30 seconds for services to start and run migrations

7. **Checks Migration Logs**
   - Reviews service logs for migration-related messages
   - Displays any errors found

8. **Verifies Tables Created**
   - Checks each database for tables
   - Lists all tables in each database
   - Verifies migration history (`__EFMigrationsHistory` table)

9. **Service Health Check**
   - Displays replica status for all services

**Configuration:**

The script uses the following default values:
- `SA_PASSWORD`: `YourStrong@Passw0rd`
- Databases: `PMS_Booking`, `PMS_Invoice`, `PMS_Site`

**Example Output:**

```
========================================
PMS Database Creation & Migration
========================================

✓ SQL Server container found: abc123def456

Waiting for SQL Server to be ready...
✓ SQL Server is ready

Step 1: Creating Databases
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Creating PMS_Booking... ✓ Created successfully
Creating PMS_Invoice... Already exists (skipping)
Creating PMS_Site... ✓ Created successfully
```

**Troubleshooting:**

- **SQL Server container not found:**
  ```bash
  docker ps | grep sqlserver
  ```
  Ensure SQL Server is running before executing the script.

- **No tables found after migration:**
  ```bash
  docker service logs pms-backend_booking-service --tail 50
  ```
  Check service logs for migration errors.

- **Manual database inspection:**
  ```bash
  docker exec -it <sqlserver-container> /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P 'YourStrong@Passw0rd' -C -d PMS_Booking
  ```

---

## Filebeat Configuration Scripts

### `setup-filebeat-config.sh`

**Purpose:** Creates the Filebeat Docker config for Swarm deployment.

**Usage:**
```bash
./setup-filebeat-config.sh
```

**Prerequisites:**
- `filebeat.yml` file must exist in the current directory
- Docker Swarm must be initialized

**What it does:**

1. Checks if `filebeat_config` already exists
2. Removes old config if present
3. Creates new Docker config from `filebeat.yml`
4. Provides instructions for stack deployment

**Example Output:**

```
Creating Filebeat Docker config...
✅ Filebeat config created successfully!

Now you can deploy the stack:
  docker stack deploy -c docker-compose.swarm.yml pms-backend
```

**Next Steps:**

After running this script, deploy or update your Docker Swarm stack:
```bash
docker stack deploy -c docker-compose.swarm.yml pms-backend
```

---

### `update-filebeat-config.sh`

**Purpose:** Updates the Filebeat configuration in a running Docker Swarm deployment.

**Usage:**
```bash
./update-filebeat-config.sh
```

**Prerequisites:**
- Docker Swarm stack must be deployed
- `filebeat.yml` file must exist in the current directory
- Filebeat service must be running (`pms-backend_filebeat`)

**What it does:**

1. **Removes Old Config**
   - Removes existing `filebeat_config` from Docker Swarm

2. **Creates New Config**
   - Creates new config from updated `filebeat.yml`

3. **Redeploys Filebeat Service**
   - Forces update of Filebeat service to use new configuration
   - Restarts Filebeat with updated settings

**Example Output:**

```
=========================================
Updating Filebeat Configuration
=========================================

1. Removing old Filebeat config...
✅ Old config removed

2. Creating new Filebeat config from filebeat.yml...
✅ New config created

3. Redeploying Filebeat service...
✅ Filebeat service updated

=========================================
Done! Waiting for Filebeat to restart...
=========================================

Wait 1-2 minutes, then check Kibana Discover for docker.* fields
```

**Verification:**

After updating, verify the changes in Kibana:

1. Go to Kibana → Discover
2. Search for fields: `docker.container.name`
3. You should see `docker.*` fields available

**When to Use:**

- After modifying `filebeat.yml` configuration
- When changing log collection settings
- When updating Elasticsearch/Kibana endpoints
- When adjusting log processing rules

---

## Database Connection Testing

### `test-db-connection-usage.sh`

**Purpose:** Verifies that applications are actually using the configured database connection strings.

**Usage:**
```bash
./test-db-connection-usage.sh
```

**Prerequisites:**
- Docker Swarm stack must be deployed
- SQL Server container must be running
- Services must be deployed (at least `invoice-service`)

**What it does:**

1. **Test 1: Check if Tables Exist**
   - Verifies that tables exist in the database
   - Proves the application has connected and created schema
   - Lists all tables found

2. **Test 2: Check Migration History**
   - Verifies `__EFMigrationsHistory` table exists
   - Lists all applied migrations
   - Confirms migrations ran using the connection string

3. **Test 3: Check Logs for Database Activity**
   - Reviews service logs for database-related messages
   - Searches for keywords: database, sql, mssql, connection, migration, entity framework, ef core

4. **Test 4: Verify Connection String Format**
   - Checks service configuration for connection string
   - Verifies:
     - Server format: `sqlserver,1433`
     - Database name matches expected value
     - `TrustServerCertificate=True` is set

**Configuration:**

The script tests the following by default:
- Service: `pms-backend_invoice-service`
- Database: `PMS_Invoice`
- Password: `YourStrong@Passw0rd`

**Example Output:**

```
=========================================
Testing Database Connection Usage
=========================================

✓ Test 1: Check if tables exist in database
----------------------------------------
  ✓ Tables exist (app has connected and created schema):
    - Invoices
    - __EFMigrationsHistory

  ✅ CONFIRMED: Application IS using the connection string!

✓ Test 2: Check if migrations have been applied
----------------------------------------
  ✓ Migrations have been applied:
    - 20240101000000_InitialCreate
    - 20240102000000_AddInvoiceFields

  ✅ CONFIRMED: Migrations ran using the connection string!
```

**Troubleshooting:**

- **No tables found:**
  - Migrations may not have run yet
  - Check service logs for errors
  - Manually trigger migrations if needed

- **Connection string format issues:**
  - Verify service environment variables
  - Check Docker Swarm service configuration
  - Ensure `TrustServerCertificate=True` is set for Docker SQL Server

**Manual Verification:**

To manually verify database connection:

```bash
# Check service logs
docker service logs -f pms-backend_invoice-service

# Inspect service configuration
docker service inspect pms-backend_invoice-service --format '{{range .Spec.TaskTemplate.ContainerSpec.Env}}{{println .}}{{end}}'

# Connect to database directly
docker exec -it $(docker ps --filter 'name=sqlserver' --format '{{.ID}}' | head -n1) \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C -d PMS_Invoice
```

---

## General Script Usage Tips

### Making Scripts Executable

If scripts are not executable, make them executable:

```bash
chmod +x run-migrations.sh
chmod +x setup-filebeat-config.sh
chmod +x update-filebeat-config.sh
chmod +x test-db-connection-usage.sh
```

### Running Scripts in Docker Swarm Context

All scripts are designed to work with Docker Swarm deployments. Ensure:

1. Docker Swarm is initialized:
   ```bash
   docker swarm init
   ```

2. Stack is deployed:
   ```bash
   docker stack deploy -c docker-compose.swarm.yml pms-backend
   ```

3. Services are running:
   ```bash
   docker stack services pms-backend
   ```

### Script Dependencies

- **Bash**: All scripts require Bash shell
- **Docker**: Docker CLI must be installed and accessible
- **Docker Swarm**: Swarm mode must be active for most scripts
- **SQL Server Tools**: SQL Server container includes required tools

### Error Handling

All scripts include error handling:
- Exit on critical errors (`set -e` in some scripts)
- Color-coded output (green for success, red for errors, yellow for warnings)
- Detailed error messages with troubleshooting hints

### Logging

Scripts provide verbose output:
- Step-by-step progress indicators
- Success/failure status for each operation
- Summary information at the end

---

## Support

For issues or questions:

1. Check service logs:
   ```bash
   docker service logs <service-name> --tail 100
   ```

2. Verify service status:
   ```bash
   docker service ps <service-name>
   ```

3. Check Docker Swarm status:
   ```bash
   docker node ls
   docker stack services pms-backend
   ```

