#!/bin/bash

# Wait for a Docker Swarm service to be healthy
# Usage: wait-for-service.sh <stack_name> <service_name> <max_wait_seconds>

STACK_NAME=$1
SERVICE_NAME=$2
MAX_WAIT=${3:-300}
WAIT_INTERVAL=5

FULL_SERVICE_NAME="${STACK_NAME}_${SERVICE_NAME}"

echo "Waiting for service ${FULL_SERVICE_NAME} to be ready (max ${MAX_WAIT}s)..."

ELAPSED=0
while [ $ELAPSED -lt $MAX_WAIT ]; do
    # Check if service exists and has running tasks
    RUNNING=$(docker service ps ${FULL_SERVICE_NAME} --format "{{.CurrentState}}" 2>/dev/null | grep -c "Running" || echo "0")
    
    if [ "$RUNNING" -gt "0" ]; then
        # Check if service has health check and is healthy
        HEALTHY=$(docker service inspect ${FULL_SERVICE_NAME} --format "{{.UpdateStatus.State}}" 2>/dev/null || echo "unknown")
        
        # For services without explicit health status, check if tasks are running
        if [ "$HEALTHY" = "completed" ] || [ "$HEALTHY" = "rollback_completed" ] || [ "$HEALTHY" = "unknown" ]; then
            # Try to verify the service is actually responding (for services with health checks)
            TASK_ID=$(docker service ps ${FULL_SERVICE_NAME} --filter "desired-state=running" --format "{{.ID}}" | head -1)
            if [ -n "$TASK_ID" ]; then
                CONTAINER_STATE=$(docker inspect $(docker ps -q --filter "label=com.docker.swarm.task.id=${TASK_ID}") --format "{{.State.Health.Status}}" 2>/dev/null || echo "none")
                
                if [ "$CONTAINER_STATE" = "healthy" ] || [ "$CONTAINER_STATE" = "none" ]; then
                    echo "✅ Service ${FULL_SERVICE_NAME} is ready!"
                    exit 0
                fi
            else
                # No health check, just check if running
                if [ "$RUNNING" -gt "0" ]; then
                    echo "✅ Service ${FULL_SERVICE_NAME} is running!"
                    exit 0
                fi
            fi
        fi
    fi
    
    echo "  Still waiting... (${ELAPSED}s/${MAX_WAIT}s)"
    sleep $WAIT_INTERVAL
    ELAPSED=$((ELAPSED + WAIT_INTERVAL))
done

echo "⚠️  Timeout waiting for service ${FULL_SERVICE_NAME}"
exit 1

