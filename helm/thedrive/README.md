# TheDrive Helm Chart

This Helm chart represents the evolution from raw Kubernetes manifests to professional package management for the TheDrive File Upload API.

## Journey from Raw K8s to Helm

### Why We Moved to Helm

Initially, we deployed TheDrive using raw Kubernetes manifests - 8 separate YAML files including namespace, secrets, configmaps, persistent volumes, and deployments. While this worked perfectly for learning K8s fundamentals, we quickly realized the limitations:

1. **Manual Management**: Every deployment required applying 8 different files in the correct order
2. **No Version Control**: No easy way to track deployment versions or rollback changes
3. **Configuration Complexity**: Hard-coded values scattered across multiple files
4. **No Templating**: Copying configurations for different environments meant duplicating files
5. **Security Concerns**: Secrets were exposed in plain text in values.yaml initially

### What We Built

This Helm chart consolidates our learning into a professional package that includes:

- **SQL Server 2022 Express** with persistent storage (learned from our PVC experiments)
- **TheDrive API** with 2 replicas for high availability (discovered load balancing benefits)
- **Secrets Management** using Kubernetes secrets (evolved from security concerns)
- **Configuration Templating** via values.yaml (solved the hard-coding problem)
- **Service Discovery** with proper internal networking (mastered during raw K8s phase)
- **External Access** via NodePort on port 30256 (kept what worked from raw deployment)

### Key Decisions and Why

**Namespace Strategy**: We create a dedicated 'thedrive' namespace to isolate our application from system components. This came from learning about resource organization in K8s.

**Two-Tier Architecture**: API and database separation allows independent scaling and maintenance. This design emerged from understanding microservices principles during our raw K8s experiments.

**NodePort Service**: We chose NodePort over LoadBalancer because we're running on a single EC2 instance with K3s. LoadBalancer would require cloud provider integration.

**Persistent Volumes**: SQL Server data persistence was a hard requirement learned from early experiments where database restarts lost all data.

**Resource Limits**: Set conservative CPU and memory limits based on monitoring our raw K8s deployment performance.

## Prerequisites

- Kubernetes cluster (we use K3s on Ubuntu 24.04)
- Helm 3.x installed
- Docker image: destinyobs/thedrive-upload-api:latest available

## Installation Approaches

Based on our deployment experience, we offer three installation methods:

### Basic Installation (Recommended for First Deploy)
```bash
helm install thedrive ./helm/thedrive -n thedrive --create-namespace
```
This uses all default values that we tested and verified work reliably.

### Custom Installation (For Specific Requirements)
```bash
helm install thedrive ./helm/thedrive -n thedrive --create-namespace \
  --set api.replicaCount=3 \
  --set database.storage.size=2Gi \
  --set api.service.nodePort=32000
```
Override specific values without creating a custom file.

### Template-Based Installation (Production Approach)
```bash
# Copy the template we created for security
cp helm/thedrive/values.yaml.template my-values.yaml
# Edit my-values.yaml with your specific settings
helm install thedrive ./helm/thedrive -n thedrive --create-namespace -f my-values.yaml
```
This approach emerged from our security concerns about exposing secrets in Git.

## Architecture Decisions

### Database Configuration
We use SQL Server 2022 Express with specific optimizations learned from troubleshooting:

- **Health Checks**: TCP socket probe on port 1433 (replaced complex sqlcmd probe that failed)
- **Persistent Storage**: 1Gi PVC using default storage class (tested and sufficient for development)
- **Memory Limits**: 2Gi limit prevents OOM kills we experienced during testing
- **Security**: SA password stored in Kubernetes secret, not plain text

### API Configuration
The TheDrive API deployment reflects lessons from our raw K8s experience:

- **Replica Count**: 2 replicas for high availability without resource waste
- **Health Checks**: HTTP probes on /health endpoint with proper timing
- **Resource Limits**: Conservative 512Mi memory and 500m CPU based on observed usage
- **Database Connection**: Uses Kubernetes service discovery instead of hard-coded IPs

## Accessing the Application

Your deployed application will be available at these endpoints:

- **External API**: http://100.27.25.7:30256 (your EC2 instance IP)
- **Swagger Documentation**: http://100.27.25.7:30256/swagger/index.html
- **Internal Service**: http://thedrive-api-service.thedrive.svc.cluster.local (for pod-to-pod communication)

The NodePort 30256 was chosen to avoid conflicts with common services and matches our raw K8s deployment for consistency.

## Configuration Management

### Security Implementation
We learned from initial security warnings about exposed secrets in Git:

**values.yaml.template**: Template file committed to Git with placeholder values
**values.yaml**: Actual file with real secrets, excluded via .gitignore
**.gitignore**: Updated to prevent accidental secret commits

### Key Configuration Options

```yaml
# API Scaling (learned optimal count from load testing)
api:
  replicaCount: 2
  resources:
    limits:
      memory: "512Mi"
      cpu: "500m"

# Database Storage (sized from usage analysis)
database:
  storage:
    size: "1Gi"
  resources:
    limits:
      memory: "2Gi"

# External Access (consistent with raw K8s setup)
api:
  service:
    type: NodePort
    nodePort: 30256
```

## Lessons Learned and Best Practices

### From Raw K8s to Helm Evolution

1. **Template Everything**: Hard-coded values in raw manifests became template variables
2. **Centralize Configuration**: Scattered settings across 8 files consolidated into values.yaml
3. **Version Control**: Helm releases provide deployment history that raw kubectl lacked
4. **Dependency Management**: _helpers.tpl provides reusable template functions
5. **Security First**: Template approach prevents secret exposure in version control

### Production Readiness Features

- **Health Checks**: Proper liveness and readiness probes based on real-world testing
- **Resource Limits**: Conservative limits prevent resource starvation
- **Persistent Data**: Database survives pod restarts and deployments
- **Service Discovery**: No hard-coded IPs or endpoints
- **Namespace Isolation**: Clean separation from system components

## Operational Commands

### Day-to-Day Management
```bash
# Check deployment status
helm list -n thedrive
kubectl get pods -n thedrive

# View application logs
kubectl logs -n thedrive deployment/thedrive-api-deployment -f

# Check database connectivity
kubectl logs -n thedrive deployment/thedrive-mssql-deployment
```

### Updates and Rollbacks
```bash
# Update with new configuration
helm upgrade thedrive ./helm/thedrive -n thedrive

# Test changes before applying (dry run)
helm upgrade thedrive ./helm/thedrive -n thedrive --dry-run

# View deployment history
helm history thedrive -n thedrive

# Rollback to previous version
helm rollback thedrive -n thedrive
```

### Complete Removal
```bash
# Remove Helm release
helm uninstall thedrive -n thedrive

# Clean up namespace (if needed)
kubectl delete namespace thedrive
```

## Troubleshooting Guide

### Common Issues from Real Experience

**Pods Stuck in Pending State**
Usually indicates resource constraints or storage issues.
```bash
kubectl describe nodes
kubectl get pvc -n thedrive
kubectl describe pod <pod-name> -n thedrive
```

**Database Connection Failures**
Often related to SQL Server startup timing or password issues.
```bash
kubectl logs -n thedrive deployment/thedrive-mssql-deployment
kubectl get secrets -n thedrive
```

**API Not Accessible Externally**
Service or port configuration problems.
```bash
kubectl get services -n thedrive
kubectl describe service thedrive-api-service -n thedrive
```

**Image Pull Errors**
Docker image availability or registry access issues.
```bash
docker pull destinyobs/thedrive-upload-api:latest
kubectl describe pod <pod-name> -n thedrive
```

## Migration Notes

### From Raw K8s Deployment
If migrating from our previous raw Kubernetes setup:

1. **Remove existing deployment**: kubectl delete namespace thedrive
2. **Clear any persistent volumes**: kubectl get pv (if needed)
3. **Deploy via Helm**: Follow installation instructions above
4. **Verify functionality**: Test API endpoints and database connectivity

### Chart Development Evolution
This chart structure evolved through several iterations:
- Initial conversion from raw manifests
- Security improvements for secret management
- Resource optimization based on monitoring
- Template abstraction for reusability

## Technical Implementation Details

### Helm Chart Structure
```
thedrive/
├── Chart.yaml              # Chart metadata and version info
├── values.yaml              # Default configuration values
├── values.yaml.template     # Git-safe template with placeholders
└── templates/
    ├── _helpers.tpl         # Reusable template functions
    ├── namespace.yaml       # Namespace creation
    ├── secret.yaml          # Kubernetes secrets for passwords
    ├── configmap.yaml       # Configuration data
    ├── mssql-pvc.yaml       # Persistent volume claim for database
    ├── mssql-deployment.yaml # SQL Server deployment
    ├── mssql-service.yaml   # Database service definition
    ├── api-deployment.yaml  # TheDrive API deployment
    └── api-service.yaml     # API service with NodePort
```

### Template Functions
The _helpers.tpl file provides reusable functions that eliminate duplication:
- Chart name standardization
- Label generation
- Selector templates
- Resource naming conventions

These templates ensure consistency across all Kubernetes resources and follow Helm best practices.

## Security Considerations

### Secrets Management
- Database passwords stored in Kubernetes secrets, not ConfigMaps
- Values template approach prevents accidental Git commits of sensitive data
- Default passwords included for development, should be changed for production

### Network Security
- Pod-to-pod communication uses internal service discovery
- External access limited to API service only
- Database not directly accessible from outside cluster

### Production Recommendations
- Use external secret management systems (HashiCorp Vault, AWS Secrets Manager)
- Implement proper RBAC for namespace access
- Consider network policies for additional pod isolation
- Regular security scanning of container images

## Next Steps

This Helm chart provides the foundation for advanced Kubernetes operations:

1. **CI/CD Integration**: Automate deployments with GitHub Actions or Jenkins
2. **GitOps with ArgoCD**: Implement declarative deployment management
3. **Monitoring**: Add Prometheus metrics and Grafana dashboards
4. **Scaling**: Implement Horizontal Pod Autoscaler for API pods
5. **Ingress**: Replace NodePort with proper ingress controller for production

The chart is designed to evolve with your infrastructure maturity while maintaining the core functionality proven in our raw Kubernetes experiments.
