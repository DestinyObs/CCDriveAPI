# ArgoCD Installation and Setup Guide

ArgoCD is a declarative, GitOps continuous delivery tool for Kubernetes. It monitors your Git repository for changes and automatically applies them to your Kubernetes cluster, ensuring your deployed applications always match the desired state defined in Git.

## What is GitOps and Why ArgoCD

### Traditional Deployment Problems

In traditional deployments, developers manually run commands like:
```bash
kubectl apply -f deployment.yaml
helm install myapp ./chart
```

This creates several problems:
- **No Single Source of Truth**: What's actually deployed vs what's in Git can differ
- **Manual Errors**: Human mistakes during deployment
- **No Audit Trail**: Hard to track who deployed what when
- **Security Issues**: Developers need cluster access
- **Drift Detection**: No way to know if someone manually changed resources

### GitOps Solution

GitOps inverts this model. Instead of pushing changes to the cluster, you:
1. **Commit changes to Git** (your source of truth)
2. **ArgoCD pulls from Git** (automated sync)
3. **ArgoCD applies to cluster** (declarative state management)
4. **ArgoCD monitors for drift** (continuous reconciliation)

Benefits:
- **Git as Single Source of Truth**: Everything deployed is version controlled
- **Automated Deployment**: No manual kubectl commands
- **Audit Trail**: Git history shows all changes
- **Security**: Developers don't need cluster access
- **Drift Detection**: ArgoCD fixes manual changes automatically

## ArgoCD Architecture

### Core Components

**Application Controller**
- Monitors Git repositories for changes
- Compares desired state (Git) vs actual state (cluster)
- Triggers sync operations when differences detected

**Repository Server**
- Handles Git repository operations
- Generates Kubernetes manifests from Helm charts, Kustomize, etc.
- Caches repository data for performance

**API Server**
- Provides REST API and gRPC interfaces
- Handles authentication and authorization
- Serves the web UI

**Web UI**
- Visual interface for managing applications
- Shows deployment status, sync history, resource health
- Allows manual sync operations and rollbacks

### How ArgoCD Works

1. **Repository Monitoring**: ArgoCD polls your Git repository every 3 minutes
2. **Manifest Generation**: Converts Helm charts/Kustomize to raw Kubernetes YAML
3. **State Comparison**: Compares generated manifests with cluster resources
4. **Sync Decision**: Determines if sync is needed based on differences
5. **Application**: Applies changes to achieve desired state
6. **Health Monitoring**: Continuously monitors resource health

## Preparing Your Application for GitOps

Before installing ArgoCD, you need to prepare your application for GitOps deployment. This involves setting up secure secret management.

### Secret Management Strategy

In GitOps, your GitHub repository should contain only non-sensitive configuration. Secrets must be managed separately. Here's how we handle this:

**Current Helm Setup (Local Development):**
- `values.yaml` + `secrets.yaml` files
- `secrets.yaml` contains passwords and is gitignored
- Works with: `helm install thedrive . -f values.yaml -f secrets.yaml`

**ArgoCD Setup (GitOps Production):**
- `values.yaml` in GitHub (safe, no secrets)
- Kubernetes Secrets created manually (contains actual passwords)
- ArgoCD uses `values.yaml` from GitHub + existing Kubernetes Secrets

### Create Kubernetes Secrets from Your Local Setup

We've provided scripts to help you create Kubernetes Secrets from your existing `secrets.yaml` file:

**Option A: Use PowerShell Script (Windows)**
```powershell
cd argocd
.\create-k8s-secrets.ps1
```

**Option B: Use Bash Script (Linux/macOS/WSL)**
```bash
cd argocd
chmod +x create-k8s-secrets.sh
./create-k8s-secrets.sh
```

**Option C: Manual Creation**
Extract values from your `helm/thedrive/secrets.yaml` and run:
```bash
kubectl create secret generic thedrive-secrets \
  --from-literal=sa-password="your_actual_db_password" \
  --from-literal=jwt-secret="your_actual_jwt_secret" \
  --from-literal=sendgrid-api-key="your_actual_sendgrid_key" \
  --from-literal=s3-access-key="your_actual_s3_access_key" \
  --from-literal=s3-secret-key="your_actual_s3_secret_key" \
  --namespace=thedrive
```

**Verify Secret Creation:**
```bash
kubectl get secrets -n thedrive
kubectl describe secret thedrive-secrets -n thedrive
```

### Understanding the Security Model

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   GitHub Repo   │    │   ArgoCD        │    │   Kubernetes    │
│                 │    │                 │    │                 │
│ ✓ values.yaml   │───▶│ Monitors repo   │───▶│ ✓ values.yaml   │
│ ✗ secrets.yaml  │    │ Deploys changes │    │ ✓ K8s Secrets   │
│   (gitignored)  │    │                 │    │   (manual)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

**Why This Approach:**
- **Security:** No secrets in GitHub, compliance with security best practices
- **Automation:** ArgoCD can automatically deploy configuration changes
- **Auditability:** All configuration changes are tracked in Git
- **Separation:** Secrets managed separately from application configuration

## Installing ArgoCD on K3s

### Step 1: Create ArgoCD Namespace

```bash
# Create dedicated namespace for ArgoCD
kubectl create namespace argocd
```

Namespaces in Kubernetes provide resource isolation. ArgoCD needs its own namespace to:
- Isolate its components from your applications
- Apply specific RBAC policies
- Manage resources independently

### Step 2: Install ArgoCD Core Components

```bash
# Install ArgoCD using official manifests
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

This command installs:
- **argocd-application-controller**: Main GitOps engine
- **argocd-repo-server**: Git repository management
- **argocd-server**: API server and web UI
- **argocd-dex-server**: Authentication provider
- **argocd-redis**: Caching layer for performance

### Step 3: Verify Installation

```bash
# Check all pods are running
kubectl get pods -n argocd

# Should see output like:
# NAME                                  READY   STATUS    RESTARTS   AGE
# argocd-application-controller-...     1/1     Running   0          2m
# argocd-dex-server-...                 1/1     Running   0          2m
# argocd-redis-...                      1/1     Running   0          2m
# argocd-repo-server-...                1/1     Running   0          2m
# argocd-server-...                     1/1     Running   0          2m
```

### Step 4: Access ArgoCD Web UI

By default, ArgoCD server runs as ClusterIP service (internal only). For external access:

```bash
# Change service type to NodePort for external access
kubectl patch svc argocd-server -n argocd -p '{"spec":{"type":"NodePort"}}'

# Get the assigned NodePort
kubectl get svc argocd-server -n argocd
```

Alternative access methods:
```bash
# Port forwarding (temporary access)
kubectl port-forward svc/argocd-server -n argocd 8080:443

# LoadBalancer (if you have external load balancer)
kubectl patch svc argocd-server -n argocd -p '{"spec":{"type":"LoadBalancer"}}'
```

### Step 5: Get Initial Admin Password

```bash
# ArgoCD generates a random admin password on first install
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

# Example output: xR7pK9mNzC8wQ4t2
```

Security Note: This is the initial password. In production, you should:
- Change the admin password immediately
- Set up proper authentication (LDAP, OIDC, etc.)
- Create role-based access controls

### Step 6: Login to ArgoCD

Access ArgoCD web UI:
```
URL: https://YOUR-SERVER-IP:NODEPORT
Username: admin
Password: (from step 5)
```

## ArgoCD CLI Installation

The ArgoCD CLI provides command-line access to ArgoCD functions:

```bash
# Download ArgoCD CLI
curl -sSL -o argocd-linux-amd64 https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64

# Make executable and move to PATH
sudo install -m 555 argocd-linux-amd64 /usr/local/bin/argocd

# Verify installation
argocd version
```

Login via CLI:
```bash
# Login to ArgoCD server
argocd login YOUR-SERVER-IP:NODEPORT

# Use the same admin credentials from web UI
```

## Deploying TheDrive Application with ArgoCD

Now that ArgoCD is installed and you've created the necessary Kubernetes Secrets, you can deploy your TheDrive application using GitOps.

### Step 1: Verify Prerequisites

Before deploying, ensure you have completed the secret management setup:

```bash
# Verify that your secrets exist
kubectl get secret thedrive-secrets -n thedrive

# Check secret contents (keys only, not values)
kubectl describe secret thedrive-secrets -n thedrive
```

You should see output confirming the secret exists with the required keys:
- `sa-password`
- `jwt-secret` 
- `sendgrid-api-key`
- `s3-access-key`
- `s3-secret-key`

### Step 2: Deploy TheDrive Application

Deploy the TheDrive application using the prepared ArgoCD Application manifest:

```bash
# Deploy the application
kubectl apply -f argocd/thedrive-application.yaml

# Verify the application was created
kubectl get application -n argocd
```

### Step 3: Monitor Deployment in ArgoCD UI

1. Open ArgoCD Web UI in your browser
2. You should see the `thedrive-app` application appear
3. Click on the application to view its details
4. ArgoCD will automatically sync the application (may take a few minutes)

### Step 4: Monitor Deployment Progress

```bash
# Watch application sync status
kubectl get application thedrive-app -n argocd -w

# Monitor pod deployment
kubectl get pods -n thedrive -w

# Check application logs once pods are running
kubectl logs -f deployment/thedrive-api -n thedrive
```

### Step 5: Verify Successful Deployment

Once deployment completes, verify your application is working:

```bash
# Check all resources in thedrive namespace
kubectl get all -n thedrive

# Test API endpoint
curl http://YOUR-SERVER-IP:30256/swagger

# Check database connectivity
kubectl logs deployment/thedrive-api -n thedrive | grep -i database
```

### Step 6: Understanding GitOps Workflow

Now that your application is deployed via ArgoCD, here's how the GitOps workflow operates:

1. **Make Changes**: Modify configuration in `helm/thedrive/values.yaml` 
2. **Commit to GitHub**: Push changes to your main branch
3. **ArgoCD Detection**: ArgoCD detects changes within 3 minutes
4. **Automatic Sync**: ArgoCD automatically applies changes to cluster
5. **Monitor Results**: View deployment status in ArgoCD UI

**Important Notes:**
- Only modify `values.yaml` (safe for GitHub)
- Never modify `secrets.yaml` and commit it
- Secret changes require manual Kubernetes Secret updates
- ArgoCD manages application lifecycle automatically

### Troubleshooting ArgoCD Deployment

**Application Shows "OutOfSync":**
```bash
# Force sync if needed
kubectl patch application thedrive-app -n argocd --type merge -p '{"operation":{"sync":{"prune":true}}}'
```

**Application Won't Sync:**
```bash
# Check application events
kubectl describe application thedrive-app -n argocd

# Check ArgoCD logs
kubectl logs -f deployment/argocd-application-controller -n argocd
```

**Secret Issues:**
```bash
# Verify secret exists and has correct keys
kubectl get secret thedrive-secrets -n thedrive -o yaml

# Recreate secret if needed
kubectl delete secret thedrive-secrets -n thedrive
# Then run create-k8s-secrets.ps1 again
```

## Understanding ArgoCD Applications

### Application Definition

An ArgoCD Application is a custom Kubernetes resource that defines:
- **Source**: Where to get the manifests (Git repo, path, branch)
- **Destination**: Where to deploy (cluster, namespace)
- **Sync Policy**: How to handle deployments (manual vs automatic)

### Basic Application Structure

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: my-app
  namespace: argocd
spec:
  # Source configuration
  source:
    repoURL: https://github.com/user/repo.git
    targetRevision: main
    path: helm/chart
  
  # Destination configuration
  destination:
    server: https://kubernetes.default.svc
    namespace: my-app
  
  # Sync policy
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

### Key Application Components

**Source Configuration**
- `repoURL`: Git repository URL
- `targetRevision`: Branch, tag, or commit to monitor
- `path`: Directory containing Kubernetes manifests or Helm charts
- `helm`: Helm-specific configuration (values files, parameters)

**Destination Configuration**
- `server`: Kubernetes cluster API server URL
- `namespace`: Target namespace for deployment

**Sync Policy**
- `automated`: Enable automatic sync on Git changes
- `prune`: Remove resources not defined in Git
- `selfHeal`: Fix drift by reapplying desired state

## Security Considerations

### RBAC (Role-Based Access Control)

ArgoCD uses Kubernetes RBAC for authorization:

```bash
# View ArgoCD service account permissions
kubectl describe clusterrole argocd-application-controller

# ArgoCD needs permissions to:
# - Read/write all Kubernetes resources
# - Manage custom resources
# - Access secrets and configmaps
```

### Secret Management

ArgoCD needs access to:
- **Git repositories**: SSH keys or tokens for private repos
- **Helm repositories**: Credentials for private Helm repos
- **Container registries**: Pull secrets for private images

Best practices:
- Use SSH keys instead of passwords for Git access
- Rotate credentials regularly
- Limit ArgoCD permissions to necessary namespaces only

### Network Security

- **Git Access**: ArgoCD needs outbound access to Git repositories
- **Webhook Access**: For immediate sync, configure Git webhooks to ArgoCD
- **UI Access**: Secure web UI with TLS and proper authentication

## Monitoring and Troubleshooting

### Application Health

ArgoCD monitors application health by checking:
- **Resource Status**: Are pods running, services accessible?
- **Sync Status**: Does cluster match Git state?
- **Operation Status**: Did the last sync succeed?

### Common Issues

**Sync Failures**
```bash
# Check application events
kubectl describe application my-app -n argocd

# Check controller logs
kubectl logs -n argocd deployment/argocd-application-controller

# Common causes:
# - Invalid YAML syntax
# - Missing Kubernetes permissions
# - Resource conflicts
```

**Git Access Issues**
```bash
# Check repository server logs
kubectl logs -n argocd deployment/argocd-repo-server

# Common causes:
# - Invalid Git credentials
# - Network connectivity issues
# - Wrong repository URL
```

**Resource Conflicts**
```bash
# Check for existing resources
kubectl get all -n target-namespace

# Common causes:
# - Manual changes to cluster resources
# - Resource ownership conflicts
# - Namespace permission issues
```

## GitOps Workflow Best Practices

### Repository Structure

Organize your Git repository for GitOps:
```
repo/
├── applications/           # ArgoCD application definitions
├── helm/                  # Helm charts
│   └── myapp/
│       ├── Chart.yaml
│       ├── values.yaml    # Environment-specific values
│       └── templates/
└── environments/          # Per-environment configurations
    ├── dev/
    ├── staging/
    └── production/
```

### Environment Management

**Separate Repositories Approach**
- One repo per environment
- Clear separation of concerns
- Different access controls per environment

**Single Repository Approach**
- Different branches or directories per environment
- Simpler management
- Shared code between environments

### Change Management

**Development Workflow**
1. Developer commits code changes
2. CI builds and pushes new image
3. Developer updates Helm values with new image tag
4. ArgoCD detects change and deploys automatically

**Rollback Process**
1. Identify problematic commit in Git
2. Revert commit or update to previous version
3. ArgoCD automatically rolls back deployment

## Advanced ArgoCD Features

### Application Sets

Manage multiple applications with templates:
```yaml
apiVersion: argoproj.io/v1alpha1
kind: ApplicationSet
metadata:
  name: myapp-environments
spec:
  generators:
  - list:
      elements:
      - cluster: dev
        namespace: myapp-dev
      - cluster: prod
        namespace: myapp-prod
  template:
    metadata:
      name: 'myapp-{{cluster}}'
    spec:
      source:
        repoURL: https://github.com/user/repo.git
        path: helm/myapp
        helm:
          valueFiles:
          - values-{{cluster}}.yaml
      destination:
        namespace: '{{namespace}}'
```

### Sync Waves

Control deployment order with annotations:
```yaml
metadata:
  annotations:
    argocd.argoproj.io/sync-wave: "1"
```

Lower numbers deploy first, allowing you to sequence:
1. Namespaces and RBAC (wave 0)
2. Secrets and ConfigMaps (wave 1)
3. Database deployments (wave 2)
4. Application deployments (wave 3)

### Resource Health Checks

Custom health checks for CRDs:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-cm
  namespace: argocd
data:
  resource.customizations.health.myapp_v1_MyResource: |
    hs = {}
    if obj.status ~= nil then
      if obj.status.phase == "Running" then
        hs.status = "Healthy"
      else
        hs.status = "Progressing"
      end
    end
    return hs
```

This guide provides a complete foundation for understanding and implementing ArgoCD in your Kubernetes environment. The next step is creating your first application to deploy the TheDrive Helm chart we built.
