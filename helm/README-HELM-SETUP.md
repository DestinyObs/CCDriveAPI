# 🚀 Helm Setup & Deployment Guide for Ubuntu 24.04

This guide covers:
- Helm 3.x installation on Ubuntu
- Helm repository management
- TheDrive chart deployment
- Chart management & troubleshooting

---

## Step 1: Install Helm on Ubuntu (Official Method)

### Download and Install Helm

```bash
# Download the official installer script
curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3

# Make the script executable
chmod 700 get_helm.sh

# Run the installer
./get_helm.sh
```

> This downloads the latest Helm 3.x release and installs it to `/usr/local/bin/helm`

---

## Step 2: Verify Installation

```bash
# Check Helm version
helm version
```

> You should see output like:
> ```
> version.BuildInfo{Version:"v3.17.4", GitCommit:"...", GitTreeState:"clean", GoVersion:"go1.22.8"}
> ```

---

## Step 3: Add Common Helm Repositories (Optional)

```bash
# Add stable chart repository
helm repo add stable https://charts.helm.sh/stable

# Add bitnami repository (popular charts)
helm repo add bitnami https://charts.bitnami.com/bitnami

# Update repository index
helm repo update

# List available repositories
helm repo list
```

> This gives you access to thousands of pre-built charts for common applications.

---

## Step 4: Deploy TheDrive Application

### Prerequisites Check
```bash
# Ensure K3s is running
kubectl get nodes

# Verify you have the chart files
ls -la helm/thedrive/
```

### Basic Deployment
```bash
# Deploy TheDrive with default settings
helm install thedrive ./helm/thedrive -n thedrive --create-namespace
```

> This creates the `thedrive` namespace and deploys all components.

### Custom Deployment
```bash
# Deploy with custom settings
helm install thedrive ./helm/thedrive -n thedrive --create-namespace \
  --set api.replicaCount=3 \
  --set database.storage.size=2Gi \
  --set api.service.nodePort=32000
```

### Deployment with Custom Values File
```bash
# Copy and customize values
cp helm/thedrive/values.yaml custom-values.yaml
# Edit values.yaml.template with your preferences

# Deploy with custom values file
helm install thedrive ./helm/thedrive -n thedrive --create-namespace -f custom-values.yaml
```

---

## 🔍 Step 5: Verify Deployment

```bash
# Check Helm release status
helm status thedrive -n thedrive

# List all Helm releases
helm list -A

# Check Kubernetes resources
kubectl get all -n thedrive

# Check pods are running
kubectl get pods -n thedrive -w
```

> Wait for all pods to show `Running` status. This may take 2-3 minutes.

---

## Step 6: Access Your Application

After successful deployment:

```bash
# Get your server's external IP
curl -s ifconfig.me

# Your API endpoints:
# - API: http://YOUR_SERVER_IP:30256
# - Swagger: http://YOUR_SERVER_IP:30256/swagger/index.html
```

---

## 🔧 Chart Management Commands

### Upgrade Deployment
```bash
# Upgrade with new values
helm upgrade thedrive ./helm/thedrive -n thedrive

# Upgrade with new values file
helm upgrade thedrive ./helm/thedrive -n thedrive -f my-custom-values.yaml
```

### Rollback Deployment
```bash
# View release history
helm history thedrive -n thedrive

# Rollback to previous version
helm rollback thedrive -n thedrive

# Rollback to specific revision
helm rollback thedrive 1 -n thedrive
```

### Uninstall Deployment
```bash
# Remove the release (keeps namespace)
helm uninstall thedrive -n thedrive

# Remove release and namespace
helm uninstall thedrive -n thedrive
kubectl delete namespace thedrive
```

---

## 🛠️ Troubleshooting

### Check Release Status
```bash
# Detailed release information
helm status thedrive -n thedrive

# Get release values
helm get values thedrive -n thedrive
```

### Debug Failed Deployment
```bash
# Check pod logs
kubectl logs -n thedrive deployment/thedrive-api

# Check SQL Server logs
kubectl logs -n thedrive deployment/thedrive-mssql

# Describe problematic pods
kubectl describe pod -n thedrive <pod-name>
```

### Common Issues & Solutions

**Issue**: Pods stuck in `Pending` state
```bash
# Check node resources
kubectl describe nodes

# Check persistent volume claims
kubectl get pvc -n thedrive
```

**Issue**: Image pull errors
```bash
# Check if image exists
docker pull destinyobs/thedrive-upload-api:latest

# Verify image name in values.yaml
helm get values thedrive -n thedrive
```

**Issue**: Service not accessible
```bash
# Check service status
kubectl get svc -n thedrive

# Verify NodePort is correct
kubectl describe svc thedrive-api-service -n thedrive
```

---

## 📊 Monitoring Your Deployment

```bash
# Watch pod status in real-time
kubectl get pods -n thedrive -w

# Check resource usage
kubectl top pods -n thedrive

# View recent events
kubectl get events -n thedrive --sort-by=.metadata.creationTimestamp
```

---

##  Success! Your Helm Deployment is Ready
