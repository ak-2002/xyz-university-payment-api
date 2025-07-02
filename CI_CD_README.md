# CI/CD Pipeline Documentation

## Overview

This document describes the CI/CD pipeline implementation for the XYZ University Payment API. The pipeline is built using GitHub Actions and provides automated building, testing, security scanning, and deployment capabilities.

## Pipeline Architecture

### Workflows

1. **Main CI/CD Pipeline** (`.github/workflows/ci-cd-pipeline.yml`)
   - Build and test the application
   - Security scanning
   - Docker image building
   - Deployment to environments

2. **Security Scan** (`.github/workflows/security-scan.yml`)
   - Daily security vulnerability scanning
   - Dependency checking
   - SAST analysis with CodeQL

3. **Deploy** (`.github/workflows/deploy.yml`)
   - Environment-specific deployments
   - Health checks
   - Rollback capabilities

## Pipeline Stages

### 1. Build & Test
- **Triggers**: Push to main/develop, Pull Requests
- **Actions**:
  - Restore dependencies
  - Build solution in Release configuration
  - Run unit tests with code coverage
  - Generate test reports
  - Upload artifacts

### 2. Security Scan
- **Triggers**: Daily at 2 AM UTC, Manual, Push to main/develop
- **Actions**:
  - Check for outdated packages
  - Scan for vulnerable dependencies
  - Run SAST analysis with CodeQL
  - Generate security reports

### 3. Docker Build
- **Triggers**: Push to main/develop, Release creation
- **Actions**:
  - Build Docker images
  - Push to GitHub Container Registry
  - Apply proper tagging strategy

### 4. Deployment
- **Development**: Automatic deployment on develop branch
- **Production**: Manual approval required on main branch
- **Actions**:
  - Deploy to target environment
  - Health checks
  - Smoke tests (production only)
  - Rollback on failure

## Environment Configuration

### Development Environment
- **File**: `docker-compose.dev.yml`
- **Services**: API, SQL Server, Redis, RabbitMQ, Identity Server
- **Ports**: 5000 (API), 5001 (HTTPS), 5002 (Identity)

### Production Environment
- **File**: `docker-compose.prod.yml`
- **Services**: API (3 replicas), SQL Server, Redis (2 replicas), RabbitMQ, Identity Server (2 replicas), Nginx
- **Features**: Load balancing, SSL termination, health checks, resource limits

### Kubernetes Deployment
- **File**: `k8s/deployment.yml`
- **Features**: Horizontal Pod Autoscaler, Ingress, TLS, Resource management

## Setup Instructions

### 1. GitHub Repository Configuration

#### Required Secrets
```bash
# Database
DB_USER=your_db_user
DB_PASSWORD=your_secure_password

# JWT Configuration
JWT_SECRET_KEY=your_jwt_secret_key
JWT_ISSUER=xyz-university
JWT_AUDIENCE=xyz-university-api

# Redis
REDIS_PASSWORD=your_redis_password

# RabbitMQ
RABBITMQ_USER=your_rabbitmq_user
RABBITMQ_PASSWORD=your_rabbitmq_password

# Container Registry (if using external registry)
DOCKER_REGISTRY_USERNAME=your_registry_username
DOCKER_REGISTRY_PASSWORD=your_registry_password
```

#### Environment Protection Rules
1. **Development Environment**
   - Required reviewers: 1
   - Restrict pushes: true
   - Allow self-review: false

2. **Production Environment**
   - Required reviewers: 2
   - Restrict pushes: true
   - Allow self-review: false
   - Wait timer: 5 minutes

### 2. Local Development Setup

#### Prerequisites
- Docker Desktop
- .NET 8.0 SDK
- Git

#### Running Locally
```bash
# Clone the repository
git clone https://github.com/your-username/xyz-university-payment-api.git
cd xyz-university-payment-api

# Start development environment
docker-compose -f docker-compose.dev.yml up -d

# Run tests
dotnet test xyz-university-payment-api/Tests/xyz-university-payment-api.Tests.csproj

# Build and run locally
dotnet build xyz-university-payment-api/xyz-university-payment-api.csproj
dotnet run --project xyz-university-payment-api/xyz-university-payment-api.csproj
```

### 3. Deployment Setup

#### Docker Compose Deployment
```bash
# Development
docker-compose -f docker-compose.dev.yml up -d

# Production
docker-compose -f docker-compose.prod.yml up -d
```

#### Kubernetes Deployment
```bash
# Create namespace
kubectl create namespace xyz-university

# Create secrets
kubectl create secret generic xyz-university-secrets \
  --from-literal=connection-string="your_connection_string" \
  --from-literal=jwt-secret="your_jwt_secret" \
  -n xyz-university

# Deploy application
kubectl apply -f k8s/deployment.yml
```

## Pipeline Triggers

### Automatic Triggers
- **Push to develop**: Build, test, security scan, Docker build, deploy to development
- **Push to main**: Build, test, security scan, Docker build
- **Pull Request**: Build, test, security scan
- **Release creation**: Build, test, security scan, Docker build, deploy to production

### Manual Triggers
- **Security scan**: Can be triggered manually from GitHub Actions
- **Deployment**: Production deployment requires manual approval

## Monitoring and Observability

### Health Checks
- **API Health**: `/health` endpoint
- **Readiness**: `/health/ready` endpoint
- **Liveness**: `/health` endpoint

### Logging
- **Application logs**: Structured logging with Serilog
- **Container logs**: Docker/Kubernetes native logging
- **Pipeline logs**: GitHub Actions logs

### Metrics
- **Application metrics**: Prometheus-compatible endpoints
- **Infrastructure metrics**: CPU, memory, disk usage
- **Business metrics**: Payment processing, user activity

## Troubleshooting

### Common Issues

#### Build Failures
1. **Dependency issues**: Check package versions and compatibility
2. **Test failures**: Review test output and fix failing tests
3. **Security vulnerabilities**: Update vulnerable packages

#### Deployment Failures
1. **Environment variables**: Verify all required secrets are set
2. **Resource constraints**: Check CPU/memory limits
3. **Network connectivity**: Verify service dependencies

#### Performance Issues
1. **Resource limits**: Adjust CPU/memory allocations
2. **Scaling**: Review HPA configuration
3. **Database performance**: Check connection pooling and queries

### Debug Commands
```bash
# Check pipeline status
gh run list

# View pipeline logs
gh run view <run-id>

# Check deployment status
kubectl get pods -n xyz-university
kubectl logs -f deployment/xyz-university-api -n xyz-university

# Check service health
curl -f http://localhost/health
```

## Best Practices

### Code Quality
- Write comprehensive unit tests
- Maintain code coverage above 80%
- Use static analysis tools
- Follow coding standards

### Security
- Regular security scans
- Keep dependencies updated
- Use secrets for sensitive data
- Implement proper authentication/authorization

### Deployment
- Use blue-green or rolling deployments
- Implement proper health checks
- Monitor deployment metrics
- Have rollback procedures ready

### Monitoring
- Set up proper alerting
- Monitor application and infrastructure metrics
- Log structured data
- Implement distributed tracing

## Support

For issues with the CI/CD pipeline:
1. Check the GitHub Actions logs
2. Review the troubleshooting section
3. Create an issue in the repository
4. Contact the DevOps team

## Contributing

To contribute to the CI/CD pipeline:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This CI/CD pipeline is part of the XYZ University Payment API project and follows the same license terms. 