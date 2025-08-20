# Project Issues and Solutions Documentation

This document chronicles all issues encountered during the project development and their solutions.

## 1. Initial Docker Compose to Kubernetes Migration

### Issue: Namespace Creation and Resource Conflicts
**Problem**: Custom namespace template caused conflicts during Helm deployment
- Error: "namespace already exists" errors
- Failed resource creation due to namespace conflicts

**Root Cause**: Custom namespace template in Helm chart conflicted with standard Helm namespace creation
**Solution**: Removed custom namespace template, used standard Helm `--create-namespace` flag

### Issue: Resource Naming Conflicts
**Problem**: Multiple deployment attempts created conflicting resources
- Pods with different configurations running simultaneously
- Service conflicts preventing proper load balancing

**Root Cause**: Previous deployments not properly cleaned up before new attempts
**Solution**: Implemented proper cleanup procedures and used `helm upgrade` instead of multiple installs

## 2. Infrastructure and Memory Issues

### Issue: Memory Exhaustion on EC2 Instance
**Problem**: Pods crashing with Exit Code 139 (segmentation fault)
- Available memory: 345MB out of 3.8GB total
- API pods failing to start due to insufficient memory
- SQL Server consuming excessive memory

**Root Cause**: T2.medium instance insufficient for SQL Server + API + Kubernetes overhead
**Solution**: Upgraded EC2 instance from t2.medium to t3.large (8GB RAM)

### Issue: Resource Limits Mismatch
**Problem**: Helm chart resource limits exceeded working raw Kubernetes manifest limits
- Helm chart: 1Gi memory limit
- Working manifest: 512Mi memory limit
- Pods failing to schedule due to resource constraints

**Root Cause**: Inconsistent resource specifications between Helm chart and raw manifests
**Solution**: Aligned Helm chart resource limits to match working raw Kubernetes configuration

## 3. Database Connection Issues

### Issue: Connection String Environment Variable Expansion
**Problem**: API failing to connect to database with "Login failed for user 'sa'" error
- Connection string: `Password=$(SA_PASSWORD)` not being expanded
- ConfigMap unable to expand environment variables
- API receiving literal string instead of actual password

**Root Cause**: ConfigMaps cannot expand shell-style environment variables
**Solution**: 
1. Moved connection string from ConfigMap to Secret
2. Built connection string in Secret template using Helm templating
3. Referenced complete connection string from Secret in deployment

### Issue: SA Password Authentication Mismatch
**Problem**: SQL Server accepting password manually but API connection failing
- Manual sqlcmd connection: successful
- API connection: authentication failure
- Password stored in base64 in Secret

**Root Cause**: Connection string construction issue in Helm templates
**Solution**: Created complete connection string in Secret template with proper Helm variable substitution

## 4. Health Check and Readiness Issues

### Issue: Health Check Probe Failures
**Problem**: Readiness and liveness probes failing with HTTP 404 errors
- Probes targeting root path `/` 
- API not exposing root endpoint
- Pods showing Running but not Ready status

**Root Cause**: Health check probes pointing to non-existent endpoint
**Solution**: Changed probe path from `/` to `/swagger` endpoint which exists in the API

### Issue: Pod Readiness Delays
**Problem**: Pods taking long time to become Ready even after successful startup
- API starting successfully but probes still failing
- Kubernetes not marking pods as Ready

**Root Cause**: Incorrect health check endpoint configuration
**Solution**: Updated readiness and liveness probes to use `/swagger` endpoint

## 5. Helm Chart Structure and Templating Issues

### Issue: Template Helper Function Errors
**Problem**: Helm template rendering failing with undefined functions
- `include "thedrive.fullname"` function not found
- Template compilation errors during helm install

**Root Cause**: Missing or incorrect _helpers.tpl template file
**Solution**: Created proper _helpers.tpl with required template functions

### Issue: Values File Structure Inconsistency
**Problem**: Template references to values not matching values.yaml structure
- Template: `{{ .Values.api.env.dbName }}`
- Values file: different structure

**Root Cause**: Inconsistent values.yaml structure and template references
**Solution**: Aligned values.yaml structure with template expectations

## 6. Secret and ConfigMap Management

### Issue: Secret Data Encoding
**Problem**: Secrets not properly base64 encoded in templates
- Raw password values in Secret manifests
- Kubernetes rejecting improperly encoded secrets

**Root Cause**: Missing base64 encoding in Helm templates
**Solution**: Used Helm `b64enc` function for proper secret encoding

### Issue: Environment Variable Loading Order
**Problem**: Environment variables from ConfigMap and Secret not properly loaded
- API not receiving configuration values
- Missing environment variables in container

**Root Cause**: Incorrect envFrom configuration in deployment template
**Solution**: Properly configured envFrom to load from both ConfigMap and Secret

## 7. Service and Networking Issues

### Issue: NodePort Service Configuration
**Problem**: External access to API not working through NodePort
- Service created but not accessible externally
- Port configuration mismatch

**Root Cause**: Service port mapping configuration issues
**Solution**: Verified and corrected NodePort service configuration with proper port mapping

### Issue: Init Container Network Dependencies
**Problem**: API pods starting before database was ready
- Connection failures during startup
- Pods crashing due to database unavailability

**Root Cause**: No dependency management between API and database
**Solution**: Added init container to wait for database readiness before starting API

## 8. Entity Framework and Database Migration Issues

### Issue: Database Migration Execution
**Problem**: Entity Framework migrations not running automatically
- Database not initialized
- Missing tables and data

**Root Cause**: Migration execution not properly configured in startup
**Solution**: Ensured migrations run during application startup in Program.cs

### Issue: Database Initialization Job
**Problem**: Manual database creation required before API startup
- TheDriveAPI database not existing
- API failing to connect to non-existent database

**Root Cause**: No automated database creation process
**Solution**: Created db-init-job.yaml to automatically create database

## 9. Docker and Container Issues

### Issue: Container Image Architecture
**Problem**: Docker image not running properly on ARM-based instances
- Image architecture mismatch
- Container startup failures

**Root Cause**: Image built for different architecture than deployment target
**Solution**: Used x86_64 EC2 instances compatible with existing Docker image

### Issue: Container Resource Limits
**Problem**: Containers exceeding memory limits and being killed
- OOMKilled status on pods
- Insufficient memory allocation

**Root Cause**: Conservative memory limits vs actual application requirements
**Solution**: Adjusted memory limits based on actual usage patterns

## 10. Development Workflow Issues

### Issue: File Upload and Synchronization
**Problem**: Manual file copying to EC2 instance for each change
- Time-consuming deployment process
- Error-prone manual copying

**Root Cause**: No automated deployment pipeline
**Solution**: Created PowerShell upload script with SCP for efficient file transfer

### Issue: Configuration Management
**Problem**: Multiple configuration files getting out of sync
- Raw Kubernetes manifests vs Helm templates
- Different resource specifications

**Root Cause**: No single source of truth for configuration
**Solution**: Standardized on Helm charts as primary configuration method

## Key Lessons Learned

1. **Infrastructure Sizing**: Properly size infrastructure for workload requirements
2. **Environment Variables**: ConfigMaps cannot expand shell variables, use Secrets for dynamic values
3. **Health Checks**: Ensure health check endpoints exist and are accessible
4. **Resource Limits**: Align resource specifications across all deployment methods
5. **Dependencies**: Use init containers for service dependencies
6. **Template Structure**: Maintain consistent values.yaml structure with template references
7. **Secret Management**: Properly encode and reference secrets in Helm templates
8. **Cleanup Procedures**: Always clean up previous deployments before new attempts
9. **Documentation**: Keep configuration and troubleshooting steps well documented
10. **Testing**: Test each component individually before full integration

## Commands for Common Issues

### Clean up failed deployments
```bash
helm uninstall thedrive -n thedrive
kubectl delete namespace thedrive
```

### Check pod logs for troubleshooting
```bash
kubectl logs -f deployment/thedrive-api-deployment -n thedrive
kubectl describe pod <pod-name> -n thedrive
```

### Verify secret contents
```bash
kubectl get secret thedrive-secrets -n thedrive -o yaml
echo "<base64-value>" | base64 -d
```

### Test database connectivity
```bash
kubectl exec -it deployment/thedrive-mssql-deployment -n thedrive -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<password>" -C
```

### Monitor resource usage
```bash
kubectl top pods -n thedrive
kubectl describe nodes
```
