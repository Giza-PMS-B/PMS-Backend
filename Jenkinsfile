pipeline {

    agent any

    environment {

        // =========================
        // Swarm
        // =========================
        STACK_NAME = "pms-backend"

        // =========================
        // Docker Hub
        // =========================
        DOCKER_REPO = "wagihh"

        BOOKING_IMAGE = "${DOCKER_REPO}/pms-booking-service"
        INVOICE_IMAGE = "${DOCKER_REPO}/pms-invoice-service"
        SITE_IMAGE    = "${DOCKER_REPO}/pms-site-service"

        IMAGE_TAG = "${BUILD_NUMBER}"
    }

    stages {

        // =========================
        // Checkout
        // =========================
        stage('Checkout Backend Repo') {
            steps {
                git(
                    url: 'https://github.com/Giza-PMS-B/PMS-Backend',
                    branch: 'devops-test',
                    credentialsId: 'swarm-id'
                )
            }
        }

        // =========================
        // Verify Docker Swarm
        // =========================
        stage('Verify Docker Swarm') {
            steps {
                sh '''
                  STATE=$(docker info --format '{{.Swarm.LocalNodeState}}')
                  if [ "$STATE" != "active" ]; then
                    echo "Docker Swarm is not active"
                    exit 1
                  fi
                '''
            }
        }

        // =========================
        // Deploy INFRA Stack
        // =========================
        stage('Deploy Infrastructure') {
            steps {
                sh '''
                  echo "Deploying infrastructure stack..."
                  docker stack deploy \
                    -c docker-compose.infra.yml \
                    ${STACK_NAME}
                '''
            }
        }

        // =========================
        // Wait for INFRA (Kafka-safe)
        // =========================
        stage('Wait for Infrastructure Readiness') {
            steps {
                sh '''
                  echo "Waiting for infrastructure to become healthy..."
                  MAX_WAIT=600   # 10 minutes
                  INTERVAL=20
                  ELAPSED=0

                  while [ $ELAPSED -lt $MAX_WAIT ]; do

                    NOT_READY=$(docker stack services ${STACK_NAME} \
                      --format '{{.Name}} {{.Replicas}}' \
                      | grep -E 'kafka|zookeeper|sqlserver' \
                      | grep '0/' || true)

                    if [ -z "$NOT_READY" ]; then
                      echo "All infra services are running"
                      break
                    fi

                    echo "Infra not ready yet..."
                    echo "$NOT_READY"
                    sleep $INTERVAL
                    ELAPSED=$((ELAPSED + INTERVAL))
                  done

                  if [ $ELAPSED -ge $MAX_WAIT ]; then
                    echo "Infrastructure failed to become ready in time"
                    exit 1
                  fi
                '''
            }
        }

        // =========================
        // Docker Login
        // =========================
        stage('Docker Login') {
            steps {
                withCredentials([
                    usernamePassword(
                        credentialsId: 'Docker-PAT',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )
                ]) {
                    sh '''
                      echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin
                    '''
                }
            }
        }

        // =========================
        // Build Backend Images
        // =========================
        stage('Build Backend Images') {
            steps {
                sh """
                  docker build -t ${BOOKING_IMAGE}:${IMAGE_TAG} -f Booking.API/Dockerfile .
                  docker build -t ${INVOICE_IMAGE}:${IMAGE_TAG} -f Invoice.API/Dockerfile .
                  docker build -t ${SITE_IMAGE}:${IMAGE_TAG} -f Site.API/Dockerfile .

                  docker tag ${BOOKING_IMAGE}:${IMAGE_TAG} ${BOOKING_IMAGE}:latest
                  docker tag ${INVOICE_IMAGE}:${IMAGE_TAG} ${INVOICE_IMAGE}:latest
                  docker tag ${SITE_IMAGE}:${IMAGE_TAG} ${SITE_IMAGE}:latest
                """
            }
        }

        // =========================
        // Push Backend Images
        // =========================
        stage('Push Backend Images') {
            steps {
                sh """
                  docker push ${BOOKING_IMAGE}:${IMAGE_TAG}
                  docker push ${BOOKING_IMAGE}:latest

                  docker push ${INVOICE_IMAGE}:${IMAGE_TAG}
                  docker push ${INVOICE_IMAGE}:latest

                  docker push ${SITE_IMAGE}:${IMAGE_TAG}
                  docker push ${SITE_IMAGE}:latest
                """
            }
        }

        // =========================
        // Deploy APPLICATION Stack
        // =========================
        stage('Deploy Backend Services') {
            steps {
                sh """
                  export IMAGE_TAG=${IMAGE_TAG}
                  docker stack deploy \
                    -c docker-compose.swarm.yml \
                    ${STACK_NAME}
                """
            }
        }

        // =========================
        // App Health Check
        // =========================
        stage('Application Health Check') {
            steps {
                sh '''
                  echo "Waiting for application services..."
                  sleep 40

                  FAILED=$(docker stack services ${STACK_NAME} \
                    --format '{{.Name}} {{.Replicas}}' \
                    | grep -E 'booking|invoice|site' \
                    | grep "0/" || true)

                  if [ -n "$FAILED" ]; then
                    echo "Some application services failed:"
                    echo "$FAILED"
                    exit 1
                  else
                    echo "All application services are running"
                  fi
                '''
            }
        }
    }

    post {

        always {
            sh 'docker logout || true'
        }

        success {
            echo "Build ${BUILD_NUMBER} deployed successfully to ${STACK_NAME}"
        }

        failure {
            echo "Build ${BUILD_NUMBER} failed"
        }
    }
}

