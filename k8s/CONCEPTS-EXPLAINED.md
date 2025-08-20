# Kubernetes Concepts Explained Simply

## What We Just Created

Think of the old Docker Compose setup vs this new Kubernetes setup:

### Docker Compose 
```
docker-compose.yml
├── mssql container
├── db-init container  
└── api container
```

### Kubernetes
```
Namespace (thedrive)
├── Secret (passwords)
├── ConfigMap (settings)
├── PVC (storage request)
├── SQL Server
│   ├── Deployment (manages pods)
│   └── Service (networking)
└── API
    ├── Deployment (manages pods)
    └── Service (networking)
```

## Key Differences

### 1. **Separation of Concerns**
- **Docker Compose**: Everything in one file
- **Kubernetes**: Each concern gets its own file

### 2. **Configuration Management**
- **Docker Compose**: `.env` file
- **Kubernetes**: ConfigMap (public) + Secret (private)

### 3. **Networking**
- **Docker Compose**: Services talk to each other by name
- **Kubernetes**: Services provide stable DNS names for pods

### 4. **Storage**
- **Docker Compose**: Named volumes
- **Kubernetes**: PersistentVolumeClaims (more powerful)

### 5. **Scaling**
- **Docker Compose**: One container per service
- **Kubernetes**: Multiple replicas per deployment

## The Magic Explained

### Why So Many Files?
Each file has a specific job:

1. **Namespace**: "Create a folder called 'thedrive'"
2. **Secret**: "Store passwords safely"
3. **ConfigMap**: "Store settings that aren't secret"
4. **PVC**: "Reserve 1GB of storage for the database"
5. **SQL Deployment**: "Run SQL Server with these specs"
6. **SQL Service**: "Give SQL Server a stable network name"
7. **API Deployment**: "Run 2 copies of our API"
8. **API Service**: "Let people access our API from outside"

### Why Use Services?
In Docker Compose, if your database container restarts, it keeps the same name.
In Kubernetes, if a pod restarts, it gets a new IP address.
Services solve this by providing a stable name that always works.

### Example:
- **Docker Compose**: `mssql:1433`
- **Kubernetes**: `mssql-service:1433`

## What Happens When We Deploy

1. **Namespace**: Creates isolated environment
2. **Secret & ConfigMap**: Stores our configuration
3. **PVC**: Kubernetes finds storage and reserves it
4. **SQL Deployment**: 
   - Creates 1 SQL Server pod
   - Mounts the storage
   - Waits for SQL Server to be ready
5. **SQL Service**: Creates network route to SQL Server
6. **API Deployment**:
   - Creates 2 API pods
   - Each connects to `mssql-service`
   - Waits for API to be healthy
7. **API Service**: Exposes API on port 30256

## Benefits Over Docker Compose

1. **High Availability**: If a pod crashes, K8s restarts it
2. **Load Balancing**: Traffic distributed across API pods
3. **Rolling Updates**: Update without downtime
4. **Resource Limits**: Prevents one container from hogging resources
5. **Health Checks**: Automatic monitoring and healing
6. **Scaling**: Easy to add more replicas

## Next Steps

Once you understand these concepts, we'll:
1. Deploy this to your K8s cluster
2. Create Helm charts (packages these files nicely)
3. Set up CI/CD (automatic deployments)
4. Add monitoring (see how everything is performing)
