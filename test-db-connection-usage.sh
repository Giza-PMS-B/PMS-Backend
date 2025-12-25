#!/bin/bash
# Test to verify the application is actually using the connection string
# (not just that it's configured)

echo "========================================="
echo "Testing Database Connection Usage"
echo "========================================="
echo ""

SERVICE_NAME="pms-backend_invoice-service"
DB_NAME="PMS_Invoice"
DB_PASSWORD="YourStrong@Passw0rd"

# Test 1: Check if tables exist (proves app connected and created schema)
echo "✓ Test 1: Check if tables exist in database"
echo "----------------------------------------"
TABLES=$(docker exec $(docker ps --filter "name=sqlserver" --format "{{.ID}}" | head -n1) \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -C \
  -d "$DB_NAME" \
  -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';" \
  -h -1 2>/dev/null | grep -v "^$" | grep -v "^---" | grep -v "rows affected" | tail -n +2)

if [ -n "$TABLES" ] && [ "$TABLES" != "TABLE_NAME" ]; then
    echo "  ✓ Tables exist (app has connected and created schema):"
    echo "$TABLES" | while read table; do
        if [ -n "$table" ]; then
            echo "    - $table"
        fi
    done
    echo ""
    echo "  ✅ CONFIRMED: Application IS using the connection string!"
else
    echo "  ⚠ No tables found. This means:"
    echo "    - Either migrations haven't been run"
    echo "    - Or the app hasn't performed any database operations yet"
    echo ""
fi
echo ""

# Test 2: Check if __EFMigrationsHistory table exists (proves migrations ran)
echo "✓ Test 2: Check if migrations have been applied"
echo "----------------------------------------"
MIGRATIONS=$(docker exec $(docker ps --filter "name=sqlserver" --format "{{.ID}}" | head -n1) \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -C \
  -d "$DB_NAME" \
  -Q "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;" \
  -h -1 2>/dev/null | grep -v "^$" | grep -v "^---" | grep -v "rows affected" | tail -n +2)

if [ -n "$MIGRATIONS" ] && [ "$MIGRATIONS" != "MigrationId" ]; then
    echo "  ✓ Migrations have been applied:"
    echo "$MIGRATIONS" | while read migration; do
        if [ -n "$migration" ]; then
            echo "    - $migration"
        fi
    done
    echo ""
    echo "  ✅ CONFIRMED: Migrations ran using the connection string!"
else
    echo "  ⚠ No migrations found in __EFMigrationsHistory table"
    echo "  This could mean migrations haven't been run yet"
    echo ""
fi
echo ""

# Test 3: Check recent logs for database connection attempts
echo "✓ Test 3: Check logs for database activity"
echo "----------------------------------------"
DB_LOGS=$(docker service logs $SERVICE_NAME --tail 200 2>&1 | grep -iE "database|sql|mssql|connection|migration|entity framework|ef core" | grep -v "Kafka" | head -10)

if [ -n "$DB_LOGS" ]; then
    echo "  Found database-related log entries:"
    echo "$DB_LOGS" | head -5 | sed 's/^/    /'
    echo ""
else
    echo "  ⚠ No database-related log entries found"
    echo "  (This is normal if EF Core connects lazily on first use)"
    echo ""
fi
echo ""

# Test 4: Verify connection string format matches what .NET expects
echo "✓ Test 4: Verify connection string format"
echo "----------------------------------------"
CONN_STRING=$(docker service inspect $SERVICE_NAME --format '{{range .Spec.TaskTemplate.ContainerSpec.Env}}{{println .}}{{end}}' | grep ConnectionStrings__DefaultConnection)

if echo "$CONN_STRING" | grep -q "Server=sqlserver,1433"; then
    echo "  ✓ Server format is correct: sqlserver,1433"
else
    echo "  ✗ Server format might be incorrect"
fi

if echo "$CONN_STRING" | grep -q "Database=$DB_NAME"; then
    echo "  ✓ Database name matches: $DB_NAME"
else
    echo "  ✗ Database name mismatch"
fi

if echo "$CONN_STRING" | grep -q "TrustServerCertificate=True"; then
    echo "  ✓ TrustServerCertificate is set (required for Docker SQL Server)"
else
    echo "  ⚠ TrustServerCertificate might be missing"
fi
echo ""

# Summary
echo "========================================="
echo "Summary & Next Steps"
echo "========================================="
echo ""
echo "To definitively verify the connection string is being used:"
echo ""
echo "1. Check if tables exist (Test 1 above)"
echo "   If tables exist → App HAS used the connection string ✅"
echo ""
echo "2. Run migrations manually to create tables:"
echo "   docker exec -it \$(docker ps --filter 'name=invoice-service' --format '{{.ID}}' | head -n1) \\"
echo "     dotnet ef database update --project Invoice.Infrastrcure.Persistent --startup-project Invoice.API"
echo ""
echo "3. Or trigger a database operation via API:"
echo "   - Call an endpoint that uses InvoiceService.CreateInvoiceAsync()"
echo "   - This will force EF Core to connect and create tables if they don't exist"
echo ""
echo "4. Monitor logs during database operation:"
echo "   docker service logs -f $SERVICE_NAME"
echo ""

