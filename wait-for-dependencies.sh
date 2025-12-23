#!/usr/bin/env bash
set -e

echo "Waiting for Kafka cluster to be ready..."

until kafka-broker-api-versions \
  --bootstrap-server kafka:9092 \
  >/dev/null 2>&1; do
  echo "Kafka cluster not ready yet..."
  sleep 5
done

echo "Kafka cluster is ready."
echo "Starting application..."

exec "$@"
