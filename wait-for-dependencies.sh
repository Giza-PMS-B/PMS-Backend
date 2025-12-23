#!/usr/bin/env bash
set -e

KAFKA_BOOTSTRAP="kafka:9092"
TOPIC_NAME="site-created"
PARTITIONS=1
REPLICATION_FACTOR=1

echo "Waiting for Kafka cluster to become ready..."

# 1️⃣ Wait for Kafka CLUSTER readiness (not TCP)
until kafka-broker-api-versions \
  --bootstrap-server "$KAFKA_BOOTSTRAP" \
  >/dev/null 2>&1; do
  echo "Kafka cluster not ready yet..."
  sleep 5
done

echo "Kafka cluster is ready."

echo "Checking if Kafka topic '$TOPIC_NAME' exists..."

# 2️⃣ Check topic existence
if kafka-topics \
  --bootstrap-server "$KAFKA_BOOTSTRAP" \
  --describe \
  --topic "$TOPIC_NAME" \
  >/dev/null 2>&1; then

  echo "Kafka topic '$TOPIC_NAME' already exists."

else
  echo "Kafka topic '$TOPIC_NAME' does not exist. Creating..."

  # 3️⃣ Create topic if missing
  kafka-topics \
    --bootstrap-server "$KAFKA_BOOTSTRAP" \
    --create \
    --if-not-exists \
    --topic "$TOPIC_NAME" \
    --partitions "$PARTITIONS" \
    --replication-factor "$REPLICATION_FACTOR"

  echo "Kafka topic '$TOPIC_NAME' created successfully."
fi

# -------------------------------------------------
# SQL Server readiness check (COMMENTED ON PURPOSE)
# -------------------------------------------------
# echo "Waiting for SQL Server..."
#
# until /opt/mssql-tools/bin/sqlcmd \
#   -S sqlserver \
#   -U sa \
#   -P "$SA_PASSWORD" \
#   -C \
#   -Q "SELECT 1" \
#   >/dev/null 2>&1; do
#   echo "SQL Server not ready yet..."
#   sleep 5
# done
#
# echo "SQL Server is ready."
# -------------------------------------------------

echo "All dependencies are ready. Starting application..."
exec "$@"
