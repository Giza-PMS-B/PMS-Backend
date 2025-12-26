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
        DOCKER_REPO = "omareldamaty"
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
                    branch: 'main',
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
        // Push Backend Images with Retry
        // =========================
       // stage('Push Backend Images') {
       //     steps {
       //         script {
       //             def images = [
       //                 ["${BOOKING_IMAGE}", "Booking"],
       //                 ["${INVOICE_IMAGE}", "Invoice"],
       //                 ["${SITE_IMAGE}", "Site"]
       //             ]
       //             
       //             for (img in images) {
       //                 def imageName = img[0]
       //                 def serviceName = img[1]
       //                 
       //                 retry(3) {
       //                     echo "Pushing ${serviceName} service images (${imageName})..."
       //                     sh """
       //                       docker push ${imageName}:${IMAGE_TAG}
       //                       docker push ${imageName}:latest
       //                     """
       //                 }
       //             }
       //         }
       //     }
       // }
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
                script {
                    echo "Waiting for services to stabilize..."
                    sleep 240
                    
                    def maxRetries = 5
                    def retryDelay = 10
                    def allHealthy = false
                    
                    for (int i = 0; i < maxRetries; i++) {
                        def failed = sh(
                            script: """
                              docker stack services ${STACK_NAME} \
                                --format '{{.Name}} {{.Replicas}}' | grep '0/' || true
                            """,
                            returnStdout: true
                        ).trim()
                        
                        if (failed == '') {
                            echo "All services are running"
                            allHealthy = true
                            break
                        } else {
                            echo "Attempt ${i + 1}/${maxRetries}: Some services not ready:"
                            echo failed
                            if (i < maxRetries - 1) {
                                echo "Waiting ${retryDelay} seconds before retry..."
                                sleep retryDelay
                            }
                        }
                    }
                    
                    if (!allHealthy) {
                        error("Services failed to become healthy after ${maxRetries} attempts")
                    }
                }
            }
        }
        // =========================
        // Run Database Migrations
        // =========================
        stage('Run Database Migrations') {
            steps {
                script {
                    echo "Waiting for SQL Server to be ready..."
                    sleep 20
                    
                    // Check if SQL Server is accessible
                    def sqlContainer = sh(
                        script: """
                          docker ps --filter 'name=${STACK_NAME}_sqlserver' --format '{{.ID}}' | head -n1
                        """,
                        returnStdout: true
                    ).trim()
                    
                    if (!sqlContainer) {
                        echo "⚠ Warning: SQL Server container not found. Migrations may fail."
                    }
                    
                    echo "Running EF Core migrations for all services..."
                    sh '''
                        chmod +x run-migrations.sh
                        export DB_PASSWORD="YourStrong@Passw0rd"
                        export SQL_SERVER="sqlserver"
                        export NETWORK="pms-network"
                        ./run-migrations.sh
                    '''
                }
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
            sh '''
              echo "=== Service Status ==="
              docker stack services ${STACK_NAME} || true
              echo ""
              echo "=== Service Logs (last 50 lines) ==="
              for service in $(docker stack services ${STACK_NAME} --format '{{.Name}}' 2>/dev/null || true); do
                echo "--- Logs for $service ---"
                docker service logs --tail 50 $service || true
              done
            '''
        }
    }
}
