#!/bin/sh
set -e

echo "Waiting for Kafka (TCP check)..."

until sh -c "</dev/tcp/kafka/9092" 2>/dev/null; do
  echo "Kafka not ready yet..."
  sleep 5
done

echo "Kafka TCP port is reachable."
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

echo "Starting application..."
exec "$@"
