pipeline {

    agent any

    environment {
        // =============================
        // MODE SWITCH
        // =============================
        // false = local testing (no push)
        // true  = production (push enabled)
        PUSH_IMAGES = "false"

        // =============================
        // Swarm
        // =============================
        STACK_NAME = "pms-backend"

        // =============================
        // Docker Hub
        // =============================
        DOCKER_REPO = "wagihh"

        BOOKING_IMAGE = "${DOCKER_REPO}/pms-booking-service"
        INVOICE_IMAGE = "${DOCKER_REPO}/pms-invoice-service"
        SITE_IMAGE    = "${DOCKER_REPO}/pms-site-service"

        IMAGE_TAG = "${BUILD_NUMBER}"
    }

    stages {

        stage('Checkout Backend Repo') {
            steps {
                git(
                    url: 'https://github.com/Giza-PMS-B/PMS-Backend',
                    branch: 'deployment',
                    credentialsId: 'github-pat-wagih'
                )
            }
        }

        stage('Verify Docker Swarm') {
            steps {
                sh '''
                  [ "$(docker info --format '{{.Swarm.LocalNodeState}}')" = "active" ]
                '''
            }
        }

        // =============================
        // Docker Login (ONLY if pushing)
        // =============================
        stage('Docker Login') {
            when {
                expression { env.PUSH_IMAGES == "true" }
            }
            steps {
                withCredentials([
                    usernamePassword(
                        credentialsId: 'Docker-PAT',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )
                ]) {
                    sh 'echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin'
                }
            }
        }

        stage('Build Images') {
            steps {
                sh """
                  docker build -t ${BOOKING_IMAGE}:${IMAGE_TAG} -f Booking.API/Dockerfile .
                  docker build -t ${INVOICE_IMAGE}:${IMAGE_TAG} -f Invoice.API/Dockerfile .
                  docker build -t ${SITE_IMAGE}:${IMAGE_TAG} -f Site.API/Dockerfile .
                  
                  # Tag as latest for local deployment
                  docker tag ${BOOKING_IMAGE}:${IMAGE_TAG} ${BOOKING_IMAGE}:latest
                  docker tag ${INVOICE_IMAGE}:${IMAGE_TAG} ${INVOICE_IMAGE}:latest
                  docker tag ${SITE_IMAGE}:${IMAGE_TAG} ${SITE_IMAGE}:latest
                """
            }
        }

        // =============================
        // Push Images (DISABLED FOR NOW)
        // =============================
        stage('Push Images') {
            when {
                expression { env.PUSH_IMAGES == "true" }
            }
            steps {
                sh """
                  docker push ${BOOKING_IMAGE}:${IMAGE_TAG}
                  docker push ${INVOICE_IMAGE}:${IMAGE_TAG}
                  docker push ${SITE_IMAGE}:${IMAGE_TAG}
                  docker push ${BOOKING_IMAGE}:latest
                  docker push ${INVOICE_IMAGE}:latest
                  docker push ${SITE_IMAGE}:latest
                """
            }
        }

        // =============================
        // PHASE 1 – Deploy Infrastructure
        // Deploy full stack but scale application services to 0
        // =============================
        stage('Deploy Infrastructure') {
            steps {
                sh """
                  # Deploy stack with all services (apps will be scaled to 0)
                  export BOOKING_IMAGE=${BOOKING_IMAGE}:latest
                  export INVOICE_IMAGE=${INVOICE_IMAGE}:latest
                  export SITE_IMAGE=${SITE_IMAGE}:latest
                  
                  docker stack deploy \
                    -c docker-compose.swarm.yml \
                    ${STACK_NAME}
                  
                  # Scale application services to 0 initially
                  docker service scale ${STACK_NAME}_booking-service=0 2>/dev/null || true
                  docker service scale ${STACK_NAME}_invoice-service=0 2>/dev/null || true
                  docker service scale ${STACK_NAME}_site-service=0 2>/dev/null || true
                """
            }
        }

        // =============================
        // Wait for Zookeeper (base dependency)
        // =============================
        stage('Wait for Zookeeper') {
            steps {
                sh '''
                  echo "Waiting for Zookeeper to be ready..."
                  until docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    confluentinc/cp-zookeeper:7.6.0 \
                    bash -c "nc -z zookeeper 2181" \
                    >/dev/null 2>&1; do
                    echo "Zookeeper not ready yet..."
                    sleep 5
                  done
                  echo "✅ Zookeeper is ready"
                '''
            }
        }

        // =============================
        // Wait for Kafka (depends on Zookeeper)
        // =============================
        stage('Wait for Kafka & Create Topics') {
            steps {
                sh '''
                  echo "Waiting for Kafka to be ready..."
                  until docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    confluentinc/cp-kafka:7.6.0 \
                    kafka-broker-api-versions \
                      --bootstrap-server kafka:9092 \
                      >/dev/null 2>&1; do
                    echo "Kafka not ready yet..."
                    sleep 5
                  done
                  echo "✅ Kafka is ready"
                  
                  # Create Kafka topics
                  echo "Creating Kafka topics..."
                  docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    confluentinc/cp-kafka:7.6.0 \
                    kafka-topics \
                      --bootstrap-server kafka:9092 \
                      --create --if-not-exists \
                      --topic site-created \
                      --partitions 1 \
                      --replication-factor 1 || echo "Topic creation skipped or already exists"
                '''
            }
        }

        // =============================
        // Wait for SQL Server (depends on Kafka phase completion)
        // =============================
        stage('Wait for SQL Server') {
            steps {
                sh '''
                  echo "Waiting for SQL Server to be ready..."
                  MAX_WAIT=180
                  ELAPSED=0
                  
                  until docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    mcr.microsoft.com/mssql/server:2022-latest \
                    /opt/mssql-tools18/bin/sqlcmd \
                      -S sqlserver \
                      -U sa \
                      -P "${SA_PASSWORD:-YourStrong@Passw0rd}" \
                      -C \
                      -Q "SELECT 1" \
                      >/dev/null 2>&1; do
                    
                    if [ $ELAPSED -ge $MAX_WAIT ]; then
                      echo "⚠️  Timeout waiting for SQL Server"
                      exit 1
                    fi
                    
                    echo "SQL Server not ready yet... (${ELAPSED}s)"
                    sleep 5
                    ELAPSED=$((ELAPSED + 5))
                  done
                  echo "✅ SQL Server is ready"
                '''
            }
        }

        // =============================
        // PHASE 2 – Deploy Application Services
        // Scale application services to 1 (they're already defined in the stack)
        // =============================
        stage('Deploy Application Services') {
            steps {
                sh """
                  echo "Scaling application services to 1..."
                  
                  # Scale up application services
                  docker service scale ${STACK_NAME}_booking-service=1
                  docker service scale ${STACK_NAME}_invoice-service=1
                  docker service scale ${STACK_NAME}_site-service=1
                  
                  echo "✅ Application services scaled up"
                """
            }
        }

        // =============================
        // Health Check
        // =============================
        stage('Health Check') {
            steps {
                sh '''
                  echo "Waiting for all services to stabilize..."
                  sleep 30
                  
                  echo ""
                  echo "📊 Stack Status:"
                  docker stack services ${STACK_NAME}
                  
                  echo ""
                  echo "📋 Service Tasks:"
                  docker stack ps ${STACK_NAME} --no-trunc
                  
                  # Check if all services have running replicas
                  FAILED_SERVICES=$(docker service ls --filter "label=com.docker.stack.namespace=${STACK_NAME}" --format "{{.Name}}: {{.Replicas}}" | grep -v "1/1" | grep -v "0/0" || true)
                  
                  if [ -n "$FAILED_SERVICES" ]; then
                    echo "⚠️  Warning: Some services may not be running correctly:"
                    echo "$FAILED_SERVICES"
                  else
                    echo "✅ All services appear to be running"
                  fi
                '''
            }
        }
    }

    post {
        always {
            echo "Pipeline completed"
        }
        failure {
            echo "Pipeline failed - check logs above"
        }
        success {
            echo "✅ Deployment successful!"
        }
    }
}

