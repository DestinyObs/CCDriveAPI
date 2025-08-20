# TheDrive Kubernetes Project Documentation

This document provides a complete overview of the TheDrive Kubernetes deployment architecture, explaining every component and design decision.

## Project Structure Overview

```
CCDriveAPI/
├── ERRORS.md                          # Complete issue log and solutions
├── helm/thedrive/                     # Professional Helm chart deployment
│   ├── Chart.yaml                     # Chart metadata and dependencies
│   ├── values.yaml                    # Configuration parameters
│   ├── README.md                      # Helm-specific documentation
│   └── templates/                     # Kubernetes resource templates
│       ├── _helpers.tpl              # Helm template helper functions
│       ├── api-deployment.yaml       # API application deployment
│       ├── api-service.yaml          # API service for external access
│       ├── configmap.yaml            # Non-sensitive configuration
│       ├── secret.yaml               # Sensitive data storage
│       ├── mssql-deployment.yaml     # Database deployment
│       ├── mssql-service.yaml        # Database service for internal access
│       ├── mssql-pvc.yaml            # Persistent volume claim for database
│       └── db-init-job.yaml          # Database initialization job
└── k8s/                              # Raw Kubernetes manifests (learning phase)
    ├── README-K8S-SETUP.md           # K3s installation guide
    └── manifests/                     # Individual Kubernetes resources
        ├── 01-namespace.yaml          # Namespace isolation
        ├── 02-secret.yaml             # Sensitive configuration
        ├── 03-configmap.yaml          # Application configuration
        ├── 04-mssql-pvc.yaml          # Database storage
        ├── 05-mssql-deployment.yaml   # Database workload
        ├── 06-mssql-service.yaml      # Database networking
        ├── 07-api-deployment.yaml     # API workload
        └── 08-api-service.yaml        # API networking
```

## Architecture Overview

### Two-Tier Application Design

**Presentation/API Tier**
- **Technology**: ASP.NET Core 6 Web API
- **Container**: destinyobs/thedrive-upload-api:latest
- **Replicas**: 2 pods for high availability and load distribution
- **Port**: Internal 5256, External 30256 (NodePort)
- **Resources**: 256Mi-512Mi memory, 100m-250m CPU

**Data Tier**  
- **Technology**: SQL Server 2022 Express
- **Container**: mcr.microsoft.com/mssql/server:2022-latest
- **Replicas**: 1 (Express edition limitation)
- **Port**: Internal 1433 (ClusterIP only)
- **Storage**: 10Gi persistent volume for data durability
- **Resources**: 2Gi-4Gi memory, 500m-1000m CPU

### Network Architecture

**External Access Flow**
```
Internet → EC2 Instance:30256 → NodePort Service → API Pods:5256
```

**Internal Database Access Flow**
```
API Pods → ClusterIP Service (thedrive-mssql-service:1433) → Database Pod:1433
```

**Service Discovery**
- API discovers database via Kubernetes DNS: `thedrive-mssql-service.thedrive.svc.cluster.local`
- External access via NodePort eliminates need for LoadBalancer on single-node K3s

### Storage Strategy

**Database Persistence**
- **Type**: PersistentVolumeClaim (PVC)
- **Size**: 10Gi
- **Mount**: `/var/opt/mssql` (SQL Server data directory)
- **Provider**: local-path (K3s default)
- **Durability**: Survives pod restarts, recreations, and node reboots

**API Storage**
- **Type**: Stateless (no persistent storage needed)
- **File Storage**: External AWS S3 (configured via environment variables)
- **Temporary Storage**: Container filesystem (ephemeral)

### Security Model

**Secret Management**
- **Database Password**: Kubernetes Secret with base64 encoding
- **Connection String**: Dynamically built in Secret template with embedded password
- **API Keys**: JWT secrets, SendGrid API key, S3 credentials in Secret
- **Access Control**: RBAC via Kubernetes service accounts (default)

**Network Security**
- **Database Isolation**: ClusterIP service (internal only)
- **API Exposure**: NodePort with controlled external access
- **Pod Communication**: Kubernetes network policies (implicit)

**Configuration Security**
- **Sensitive Data**: Stored in Kubernetes Secrets
- **Non-Sensitive Data**: Stored in ConfigMaps
- **Environment Variables**: Injected at runtime, not baked into images

## Deployment Evolution

### Phase 1: Raw Kubernetes Manifests
**Purpose**: Learning Kubernetes fundamentals
**Characteristics**:
- 8 separate YAML files applied in sequence
- Hard-coded values throughout manifests
- Manual secret creation and management
- No templating or reusability
- Direct kubectl commands for all operations

**Learning Outcomes**:
- Understanding of Kubernetes resource types and relationships
- Hands-on experience with pods, services, deployments, and volumes
- Database persistence requirements and storage challenges
- Service discovery and internal networking concepts

### Phase 2: Professional Helm Chart
**Purpose**: Production-ready package management
**Characteristics**:
- Templated resources with configurable values
- Automated secret generation and encoding
- Helper functions for consistent naming and labeling
- Version-controlled deployments with rollback capability
- Single command deployment and upgrades

**Improvements**:
- **Configuration Management**: Single values.yaml for all settings
- **Template Reusability**: Helper functions prevent code duplication
- **Secret Security**: Automated base64 encoding and proper referencing
- **Resource Consistency**: Standardized labels and annotations
- **Deployment Simplicity**: One command vs multiple kubectl applies

### Phase 3: GitOps Ready (Future)
**Purpose**: Continuous deployment automation
**Planned Characteristics**:
- ArgoCD integration for Git-based deployments
- Automatic synchronization on repository changes
- Visual deployment status and health monitoring
- Automated rollbacks on failure detection
- Multi-environment promotion workflows

## Resource Requirements and Sizing

### Development Environment (Current)
**Infrastructure**: EC2 t3.large (8GB RAM, 2 vCPU)
**Database Resources**:
- Requests: 2Gi memory, 500m CPU
- Limits: 4Gi memory, 1000m CPU
- Storage: 10Gi persistent volume

**API Resources**:
- Requests: 256Mi memory, 100m CPU per pod
- Limits: 512Mi memory, 250m CPU per pod
- Replicas: 2 pods

**Total Consumption**: ~5-6Gi memory, ~1.2-1.5 CPU cores

### Production Recommendations
**Infrastructure**: Multi-node cluster with resource redundancy
**Database**: External managed service (Azure SQL, AWS RDS) for scalability
**API**: Horizontal pod autoscaling based on CPU/memory metrics
**Storage**: Network-attached storage for multi-node persistence
**Monitoring**: Prometheus/Grafana for resource usage tracking

## Operational Procedures

### Deployment Commands
```bash
# Initial deployment
helm install thedrive ./helm/thedrive -n thedrive --create-namespace

# Configuration updates
helm upgrade thedrive ./helm/thedrive -n thedrive

# Status monitoring
kubectl get pods -n thedrive
kubectl logs -f deployment/thedrive-api-deployment -n thedrive

# Resource monitoring
kubectl top pods -n thedrive
kubectl describe pods -n thedrive
```

### Troubleshooting Guidelines

**Pod Startup Issues**
1. Check resource availability: `kubectl describe nodes`
2. Verify image availability: `kubectl describe pod <pod-name> -n thedrive`
3. Check configuration: `kubectl get configmap,secret -n thedrive`

**Database Connection Issues**
1. Verify service discovery: `kubectl get svc -n thedrive`
2. Test database connectivity: `kubectl exec -it deployment/thedrive-mssql-deployment -n thedrive -- sqlcmd`
3. Check connection string: Decode secret and verify format

**Performance Issues**
1. Monitor resource usage: `kubectl top pods -n thedrive`
2. Check resource limits: `kubectl describe pod <pod-name> -n thedrive`
3. Scale API replicas: `helm upgrade thedrive ./helm/thedrive --set api.replicaCount=3`

### Health Monitoring

**API Health Checks**
- **Readiness Probe**: HTTP GET /swagger (30s delay, 10s interval)
- **Liveness Probe**: HTTP GET /swagger (60s delay, 30s interval)
- **Endpoint**: http://server-ip:30256/swagger/index.html

**Database Health Checks**
- **Readiness Probe**: TCP socket 1433 (30s delay, 10s interval)
- **Liveness Probe**: TCP socket 1433 (60s delay, 30s interval)
- **Internal Access**: thedrive-mssql-service:1433

## Best Practices Implemented

### Kubernetes Best Practices
1. **Resource Limits**: Prevent resource exhaustion with appropriate limits
2. **Health Checks**: Implement readiness and liveness probes for all services
3. **Labels and Annotations**: Consistent labeling for resource management
4. **Namespace Isolation**: Separate application from system resources
5. **Secret Management**: Never store sensitive data in plain text

### Helm Best Practices
1. **Template Functions**: Use helper templates for consistency
2. **Values Documentation**: Comprehensive comments in values.yaml
3. **Resource Naming**: Consistent naming convention across resources
4. **Chart Versioning**: Semantic versioning for chart releases
5. **Default Values**: Sensible defaults for all configurable parameters

### Security Best Practices
1. **Least Privilege**: Services only expose necessary ports
2. **Secret Rotation**: Support for updating secrets without downtime
3. **Image Security**: Use official base images and specific tags
4. **Network Segmentation**: Database not accessible from outside cluster
5. **Configuration Security**: Separate sensitive from non-sensitive config

## Future Enhancements

### Short Term (Next Sprint)
- ArgoCD integration for GitOps workflow
- Ingress controller for custom domain access
- TLS/SSL certificate management
- Resource monitoring and alerting

### Medium Term (Next Quarter)
- Horizontal Pod Autoscaling (HPA)
- External database migration (managed service)
- Multi-environment deployment (dev/staging/prod)
- Backup and disaster recovery procedures

### Long Term (Next Year)
- Service mesh implementation (Istio/Linkerd)
- Advanced security policies (NetworkPolicies, PodSecurityPolicies)
- Multi-region deployment for high availability
- Performance optimization and cost management

## Conclusion

This project demonstrates the complete journey from Docker Compose through raw Kubernetes to professional Helm chart deployment. Each phase built upon previous learning while addressing real-world operational challenges. The final Helm chart represents production-ready deployment patterns suitable for enterprise environments while maintaining simplicity for development workflows.

The comprehensive documentation, error tracking, and best practices implementation ensure that any developer can understand, deploy, and maintain this Kubernetes application successfully.
