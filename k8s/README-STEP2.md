# Step 2: Deploy TheDrive API to Kubernetes

## What We're Building

We'll convert your Docker Compose setup to Kubernetes manifests:

1. **Namespace** - Logical separation for our app
2. **ConfigMap** - Store configuration data  
3. **Secret** - Store sensitive data (passwords, keys)
4. **SQL Server Deployment** - Database container
5. **SQL Server Service** - Network access to database
6. **API Deployment** - Your TheDrive API container
7. **API Service** - Network access to API

## File Structure

```
k8s/
├── manifests/
│   ├── 01-namespace.yaml      # Logical separation
│   ├── 02-secret.yaml         # Sensitive data
│   ├── 03-configmap.yaml      # Configuration
│   ├── 04-mssql-pvc.yaml      # Database storage
│   ├── 05-mssql-deployment.yaml # Database pods
│   ├── 06-mssql-service.yaml  # Database networking
│   ├── 07-api-deployment.yaml # API pods  
│   └── 08-api-service.yaml    # API networking
```

## Key Kubernetes Concepts

- **Pod**: Your container + networking (ephemeral)
- **Deployment**: Manages pods, handles updates, scaling
- **Service**: Load balancer + DNS name for pods
- **PersistentVolume**: Permanent storage that survives pod restarts
- **ConfigMap**: Non-sensitive configuration
- **Secret**: Sensitive data (base64 encoded)

Let's create these files step by step!
