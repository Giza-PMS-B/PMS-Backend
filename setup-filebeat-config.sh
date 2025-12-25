#!/bin/bash
# Script to create Filebeat Docker config for Swarm deployment

echo "Creating Filebeat Docker config..."

# Check if config already exists
if docker config ls | grep -q "filebeat_config"; then
    echo "⚠️  filebeat_config already exists. Removing old config..."
    docker config rm filebeat_config
fi

# Create config from filebeat.yml
if [ -f "filebeat.yml" ]; then
    docker config create filebeat_config filebeat.yml
    echo "✅ Filebeat config created successfully!"
    echo ""
    echo "Now you can deploy the stack:"
    echo "  docker stack deploy -c docker-compose.swarm.yml pms-backend"
else
    echo "❌ Error: filebeat.yml not found!"
    echo "Make sure filebeat.yml exists in the current directory."
    exit 1
fi

