#!/bin/bash
# Script to run EF Core migrations for all services (Booking, Invoice, Site)
# Can be run from local machine (if .NET SDK available) or using Docker container
# For Jenkins CI/CD: runs non-interactively using Docker

echo "========================================="
echo "Running EF Core Migrations for All Services"
echo "========================================="
echo ""

DB_PASSWORD="${DB_PASSWORD:-YourStrong@Passw0rd}"
SQL_SERVER="${SQL_SERVER:-sqlserver}"
NETWORK="${NETWORK:-pms-network}"

# Service configurations: (MigrationProject, StartupProject, DatabaseName)
declare -a SERVICES=(
    "Booking.Infrastrcure.Persistent:Booking.API:PMS_Booking"
    "Invoice.Infrastrcure.Persistent:Invoice.API:PMS_Invoice"
    "Site.Infrastrcure.Persistent:Site.API:PMS_Site"
)

# Function to run migrations for a single service
run_migrations_for_service() {
    local migration_project=$1
    local startup_project=$2
    local db_name=$3
    
    echo "----------------------------------------"
    echo "Running migrations for: $startup_project"
    echo "Database: $db_name"
    echo "----------------------------------------"
    
    local connection_string="Server=$SQL_SERVER;Database=$db_name;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
    
    # Restore packages first if needed
    dotnet restore "$startup_project" || true
    
    dotnet ef database update \
        --project "$migration_project" \
        --startup-project "$startup_project" \
        --connection "$connection_string"
    
    if [ $? -eq 0 ]; then
        echo "✅ Migrations completed for $startup_project"
        return 0
    else
        echo "❌ Migrations failed for $startup_project"
        return 1
    fi
}

# Check if running in Docker (for Jenkins) or locally
if [ -n "$JENKINS_HOME" ] || [ ! -t 0 ] || ! command -v dotnet &> /dev/null || ! dotnet --version &> /dev/null; then
    # Running in Jenkins or no local SDK - use Docker container
    echo "Running migrations using Docker SDK container..."
    echo "Network: $NETWORK"
    echo "SQL Server: $SQL_SERVER"
    echo ""
    
    # Check if network exists
    if ! docker network inspect "$NETWORK" &> /dev/null; then
        echo "❌ Network '$NETWORK' does not exist. Please create it first:"
        echo "   docker network create --driver overlay --attachable $NETWORK"
        exit 1
    fi
    
    # Run migrations in Docker container
        docker run --rm \
        --network "$NETWORK" \
        -v "$(pwd):/workspace" \
        -w /workspace \
        mcr.microsoft.com/dotnet/sdk:8.0 \
        bash -c "
            set -e
            echo 'Installing EF Core tools...'
            dotnet tool install --global dotnet-ef --version 8.0.0 || dotnet tool update --global dotnet-ef --version 8.0.0
            export PATH=\"\$PATH:/root/.dotnet/tools\"
            
            echo 'Restoring NuGet packages...'
            dotnet restore PMS.sln || echo 'Warning: Some packages may have failed to restore'
            
            # Run migrations for each service
            failed_services=()
            
            # Booking Service
            echo ''
            echo '========================================='
            echo 'Running migrations for Booking Service'
            echo '========================================='
            if dotnet ef database update \
                --project Booking.Infrastrcure.Persistent \
                --startup-project Booking.API \
                --connection 'Server=$SQL_SERVER;Database=PMS_Booking;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;'; then
                echo '✅ Booking migrations completed'
            else
                echo '❌ Booking migrations failed'
                failed_services+=('Booking')
            fi
            
            # Invoice Service
            echo ''
            echo '========================================='
            echo 'Running migrations for Invoice Service'
            echo '========================================='
            if dotnet ef database update \
                --project Invoice.Infrastrcure.Persistent \
                --startup-project Invoice.API \
                --connection 'Server=$SQL_SERVER;Database=PMS_Invoice;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;'; then
                echo '✅ Invoice migrations completed'
            else
                echo '❌ Invoice migrations failed'
                failed_services+=('Invoice')
            fi
            
            # Site Service
            echo ''
            echo '========================================='
            echo 'Running migrations for Site Service'
            echo '========================================='
            if dotnet ef database update \
                --project Site.Infrastrcure.Persistent \
                --startup-project Site.API \
                --connection 'Server=$SQL_SERVER;Database=PMS_Site;User Id=sa;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;'; then
                echo '✅ Site migrations completed'
            else
                echo '❌ Site migrations failed'
                failed_services+=('Site')
            fi
            
            # Report results
            echo ''
            echo '========================================='
            echo 'Migration Summary'
            echo '========================================='
            if [ \${#failed_services[@]} -eq 0 ]; then
                echo '✅ All migrations completed successfully!'
                exit 0
            else
                echo '❌ Some migrations failed:'
                printf '   - %s\n' \"\${failed_services[@]}\"
                exit 1
            fi
        "
    
    MIGRATION_EXIT_CODE=$?
    
    if [ $MIGRATION_EXIT_CODE -eq 0 ]; then
        echo ""
        echo "✅ All migrations completed successfully!"
    else
        echo ""
        echo "❌ Some migrations failed. Check the output above."
        exit 1
    fi
else
    # Running locally with SDK available
    echo "✓ .NET SDK found locally"
    echo "Running migrations from local machine..."
    echo ""
    
    failed_services=()
    
    for service_config in "${SERVICES[@]}"; do
        IFS=':' read -r migration_project startup_project db_name <<< "$service_config"
        
        if ! run_migrations_for_service "$migration_project" "$startup_project" "$db_name"; then
            failed_services+=("$startup_project")
        fi
        echo ""
    done
    
    echo "========================================="
    echo "Migration Summary"
    echo "========================================="
    if [ ${#failed_services[@]} -eq 0 ]; then
        echo "✅ All migrations completed successfully!"
    else
        echo "❌ Some migrations failed:"
        printf '   - %s\n' "${failed_services[@]}"
        exit 1
    fi
fi

echo ""
echo "========================================="
echo "Verification"
echo "========================================="
echo ""
echo "To verify tables were created, run:"
echo "  docker exec \$(docker ps --filter 'name=sqlserver' --format '{{.ID}}' | head -n1) \\"
echo "    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$DB_PASSWORD' -C \\"
echo "    -d <DATABASE_NAME> -Q \"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';\""
echo ""


