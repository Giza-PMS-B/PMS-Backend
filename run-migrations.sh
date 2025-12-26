#!/bin/bash

# Script to create PMS databases and trigger migrations
# Usage: ./create-databases.sh

set -e

echo "========================================"
echo "PMS Database Creation & Migration"
echo "========================================"
echo ""

# Configuration
SA_PASSWORD="YourStrong@Passw0rd"
DATABASES=("PMS_Booking" "PMS_Invoice" "PMS_Site")
SQLSERVER_CONTAINER=$(docker ps -qf "name=sqlserver" | head -1)

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

if [ -z "$SQLSERVER_CONTAINER" ]; then
    echo -e "${RED}✗ SQL Server container not found${NC}"
    echo "Make sure SQL Server is running: docker ps | grep sqlserver"
    exit 1
fi

echo -e "${GREEN}✓ SQL Server container found: $SQLSERVER_CONTAINER${NC}"
echo ""

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
for i in {1..30}; do
    if docker exec $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ SQL Server is ready${NC}"
        break
    fi
    echo "  Attempt $i/30: Waiting for SQL Server..."
    sleep 2
done
echo ""

# Create databases
echo "Step 1: Creating Databases"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
for db in "${DATABASES[@]}"; do
    echo -n "Creating $db... "
    
    # Check if database already exists
    db_exists=$(docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -h -1 -W \
        -Q "SELECT name FROM sys.databases WHERE name = '$db';" 2>/dev/null | tr -d '[:space:]')
    
    if [ "$db_exists" == "$db" ]; then
        echo -e "${YELLOW}Already exists (skipping)${NC}"
    else
        # Create database
        docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C \
            -Q "CREATE DATABASE [$db];" > /dev/null 2>&1
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}✓ Created successfully${NC}"
        else
            echo -e "${RED}✗ Failed to create${NC}"
            exit 1
        fi
    fi
done
echo ""

# Verify databases
echo "Step 2: Verifying Databases"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "SELECT name, create_date FROM sys.databases WHERE name IN ('PMS_Booking', 'PMS_Invoice', 'PMS_Site') ORDER BY name;"
echo ""

# Restart services to trigger migrations
echo "Step 3: Restarting Services to Trigger Migrations"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

SERVICES=("pms-backend_booking-service" "pms-backend_invoice-service" "pms-backend_site-service")

for service in "${SERVICES[@]}"; do
    echo -n "Restarting $service... "
    docker service update --force $service > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Restarted${NC}"
    else
        echo -e "${RED}✗ Failed${NC}"
    fi
done
echo ""

# Wait for services to stabilize
echo "Step 4: Waiting for Services to Start"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Waiting 30 seconds for services to initialize and run migrations..."
for i in {30..1}; do
    echo -ne "  $i seconds remaining...\r"
    sleep 1
done
echo -e "${GREEN}✓ Wait complete${NC}                    "
echo ""

# Check service logs for migration activity
echo "Step 5: Checking Migration Logs"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

for service in "${SERVICES[@]}"; do
    service_name=$(echo $service | sed 's/pms-backend_//')
    echo ""
    echo -e "${BLUE}$service_name:${NC}"
    echo "───────────────────────────────────────"
    
    # Get logs mentioning migration or database
    logs=$(docker service logs $service --tail 20 2>/dev/null | grep -i -E "migration|database|ef core|entity framework" | tail -5 || echo "")
    
    if [ ! -z "$logs" ]; then
        echo "$logs"
    else
        echo -e "${YELLOW}No migration logs found. Checking for errors...${NC}"
        error_logs=$(docker service logs $service --tail 10 2>/dev/null | grep -i "error" | tail -3 || echo "")
        if [ ! -z "$error_logs" ]; then
            echo -e "${RED}Errors found:${NC}"
            echo "$error_logs"
        else
            echo "No errors found in recent logs"
        fi
    fi
done
echo ""

# Check database tables again
echo "Step 6: Verifying Tables Created"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

for db in "${DATABASES[@]}"; do
    echo ""
    echo -e "${BLUE}$db:${NC}"
    
    table_count=$(docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -d "$db" -h -1 -W \
        -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';" 2>/dev/null | tr -d '[:space:]')
    
    if [ "$table_count" -gt "0" ]; then
        echo -e "${GREEN}✓ $table_count tables found${NC}"
        
        # List tables
        docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C -d "$db" -h -1 \
            -Q "SELECT '  - ' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;" 2>/dev/null | grep "^  -"
        
        # Check migration history
        migration_count=$(docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$SA_PASSWORD" -C -d "$db" -h -1 -W \
            -Q "SELECT COUNT(*) FROM __EFMigrationsHistory;" 2>/dev/null | tr -d '[:space:]')
        
        if [ ! -z "$migration_count" ] && [ "$migration_count" -gt "0" ]; then
            echo -e "${GREEN}  ✓ $migration_count migrations applied${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ No tables found - migrations may not have run${NC}"
        echo "  Check service logs for errors"
    fi
done
echo ""

# Summary
echo "========================================"
echo "Summary"
echo "========================================"
echo ""
echo -e "${GREEN}Databases created successfully!${NC}"
echo ""
echo "Next steps:"
echo "  1. Check if migrations ran automatically (see table counts above)"
echo "  2. If no tables exist, check service logs:"
echo "     docker service logs pms-backend_booking-service --tail 50"
echo ""
echo "  3. To manually check a database:"
echo "     docker exec -it $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$SA_PASSWORD' -C -d PMS_Booking"
echo ""
echo "  4. To view service status:"
echo "     docker service ps pms-backend_booking-service"
echo ""

# Check service health
echo "Service Health Check:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
for service in "${SERVICES[@]}"; do
    replicas=$(docker service ls --format "{{.Name}} {{.Replicas}}" | grep $service | awk '{print $2}')
    service_name=$(echo $service | sed 's/pms-backend_//')
    echo "  $service_name: $replicas"
done
echo ""
