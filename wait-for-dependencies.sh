#!/usr/bin/env bash
set -e

echo "Waiting for Kafka (TCP check)..."

until (echo > /dev/tcp/kafka/9092) >/dev/null 2>&1; do
  echo "Kafka not ready yet..."
  sleep 5
done

echo "Kafka TCP port is reachable."
echo "Starting application..."

exec "$@"

# ----------------------------------------
# SQL Server wait (COMMENTED FOR NOW)
# ----------------------------------------
# echo "Waiting for SQL Server..."
# until curl -s sqlserver:1433 >/dev/null 2>&1; do
#   echo "SQL Server not ready yet..."
#   sleep 5
# done
# echo "SQL Server is reachable."
# ----------------------------------------

