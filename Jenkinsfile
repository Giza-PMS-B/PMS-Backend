pipeline {

    agent any

    triggers {
        pollSCM('H/2 * * * *')
    }

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
                    branch: 'DevOps',
                    credentialsId: 'github-pat-wagih'
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

                  docker rmi ${BOOKING_IMAGE}:latest || true
                  docker tag ${BOOKING_IMAGE}:${IMAGE_TAG} ${BOOKING_IMAGE}:latest

                  docker rmi ${INVOICE_IMAGE}:latest || true
                  docker tag ${INVOICE_IMAGE}:${IMAGE_TAG} ${INVOICE_IMAGE}:latest

                  docker rmi ${SITE_IMAGE}:latest || true
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
        // Deploy Swarm Stack
        // =========================
        stage('Deploy Backend Stack') {
            steps {
                sh """
                  export IMAGE_TAG=${IMAGE_TAG}
                  docker stack deploy -c docker-compose.swarm.yml ${STACK_NAME}
                """
            }
        }

        // =========================
        // Swarm Health Check
        // =========================
        stage('Health Check') {
            steps {
                sh '''
                  echo "Waiting for services to stabilize..."
                  sleep 40

                  FAILED=$(docker stack services ${STACK_NAME} \
                    --format '{{.Name}} {{.Replicas}}' | grep "0/" || true)

                  if [ -n "$FAILED" ]; then
                    echo "Some services are not running:"
                    echo "$FAILED"
                    exit 1
                  else
                    echo "All services are running"
                  fi
                '''
            }
        }
    }

    post {

        always {
            sh '''
              docker logout || true
            '''
        }

        success {
            echo "Build ${BUILD_NUMBER} deployed successfully to ${STACK_NAME}"
        }

        failure {
            echo "Build ${BUILD_NUMBER} failed"
        }
    }
}
