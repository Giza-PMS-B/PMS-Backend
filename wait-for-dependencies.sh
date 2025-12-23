#!/usr/bin/env bash
set -e

echo "Waiting for Kafka topic 'site-created'..."

until docker run --rm \
  --network pms-backend_pms-network \
  confluentinc/cp-kafka:7.6.0 \
  kafka-topics \
    --bootstrap-server kafka:9092 \
    --describe \
    --topic site-created \
    >/dev/null 2>&1; do
  echo "Kafka topic not ready yet..."
  sleep 5
done

echo "Kafka topic is available."

# -------------------------------------------------
# SQL Server readiness check (COMMENTED ON PURPOSE)
# -------------------------------------------------
# echo "Waiting for SQL Server..."
#
# until docker run --rm \
#   --network pms-backend_pms-network \
#   mcr.microsoft.com/mssql-tools \
#   /opt/mssql-tools/bin/sqlcmd \
#     -S sqlserver \
#     -U sa \
#     -P "$SA_PASSWORD" \
#     -C \
#     -Q "SELECT 1" \
#     >/dev/null 2>&1; do
#   echo "SQL Server not ready yet..."
#   sleep 5
# done
#
# echo "SQL Server is ready."
# -------------------------------------------------

echo "Starting application..."
exec "$@"
