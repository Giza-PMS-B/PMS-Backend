#!/bin/bash
# Script to update Filebeat config in Docker Swarm

set -e

echo "========================================="
echo "Updating Filebeat Configuration"
echo "========================================="
echo ""

# Step 1: Remove old config
echo "1. Removing old Filebeat config..."
if docker config ls --format "{{.Name}}" | grep -q "^filebeat_config$"; then
    docker config rm filebeat_config
    echo "✅ Old config removed"
else
    echo "⚠️  Config 'filebeat_config' not found (may be first time setup)"
fi
echo ""

# Step 2: Create new config
echo "2. Creating new Filebeat config from filebeat.yml..."
if [ ! -f "filebeat.yml" ]; then
    echo "❌ Error: filebeat.yml not found in current directory"
    exit 1
fi

docker config create filebeat_config filebeat.yml
echo "✅ New config created"
echo ""

# Step 3: Redeploy Filebeat
echo "3. Redeploying Filebeat service..."
echo "   (This will restart Filebeat with the new config)"
docker service update --force pms-backend_filebeat
echo "✅ Filebeat service updated"
echo ""

echo "========================================="
echo "Done! Waiting for Filebeat to restart..."
echo "========================================="
echo ""
echo "Wait 1-2 minutes, then check Kibana Discover for docker.* fields"
echo ""
echo "To verify the fix:"
echo "  1. Go to Kibana → Discover"
echo "  2. Search for fields: docker.container.name"
echo "  3. You should see docker.* fields available"
echo ""

