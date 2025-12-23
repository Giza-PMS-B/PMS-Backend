pipeline {

    agent any

    environment {
        STACK_NAME = "pms-backend"
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

        stage('Docker Login') {
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
                """
            }
        }

        stage('Push Images') {
            steps {
                sh """
                  docker push ${BOOKING_IMAGE}:${IMAGE_TAG}
                  docker push ${INVOICE_IMAGE}:${IMAGE_TAG}
                  docker push ${SITE_IMAGE}:${IMAGE_TAG}
                """
            }
        }

        // =============================
        // PHASE 1 – Deploy Infrastructure
        // =============================
        stage('Deploy Infrastructure') {
            steps {
                sh """
                  docker stack deploy \
                    -c docker-compose.infra.yml \
                    ${STACK_NAME}
                """
            }
        }

        // =============================
        // WAIT FOR KAFKA (CONTROL PLANE)
        // =============================
        stage('Wait for Kafka & Create Topics') {
            steps {
                sh '''
                  until docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    confluentinc/cp-kafka:7.6.0 \
                    kafka-broker-api-versions \
                      --bootstrap-server kafka:9092 \
                      >/dev/null 2>&1; do
                    echo "Kafka not ready yet..."
                    sleep 5
                  done

                  docker run --rm \
                    --network ${STACK_NAME}_pms-network \
                    confluentinc/cp-kafka:7.6.0 \
                    kafka-topics \
                      --bootstrap-server kafka:9092 \
                      --create --if-not-exists \
                      --topic site-created \
                      --partitions 1 \
                      --replication-factor 1
                '''
            }
        }

        // =============================
        // PHASE 2 – Deploy Applications
        // =============================
        stage('Deploy Application Services') {
            steps {
                sh """
                  export IMAGE_TAG=${IMAGE_TAG}
                  docker stack deploy \
                    -c docker-compose.apps.yml \
                    ${STACK_NAME}
                """
            }
        }

        stage('Health Check') {
            steps {
                sh '''
                  sleep 40
                  docker stack services ${STACK_NAME}
                '''
            }
        }
    }
}
