# Full Kubernetes Setup on Ubuntu 24.04 Using K3s

This guide installs:
- ✅ K3s (lightweight Kubernetes)
- ✅ kubectl (standalone CLI)
- ✅ kubeconfig setup for user access

---

## Step 1: Update System & Install Essentials

```bash
sudo apt-get update
sudo apt-get install -y apt-transport-https ca-certificates curl
```

> This updates your package list and installs essential transport tools for secure downloads.

---

## Step 2: Install `kubectl` CLI (Standalone)

```bash
# Download the latest stable kubectl binary
curl -LO "https://dl.k8s.io/release/$(curl -Ls https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"

# Make it executable
chmod +x kubectl

# Move to system PATH
sudo mv kubectl /usr/local/bin/

# Verify installation
kubectl version --client
```

> This installs the latest stable `kubectl` binary and makes it globally accessible.
> You should see output like: `Client Version: v1.33.4`

---

## Step 3: Install K3s (Lightweight Kubernetes)

```bash
curl -sfL https://get.k3s.io | sudo sh -
```

> This downloads and installs K3s, starts the Kubernetes server, and sets up systemd service.
> The installation takes 1-2 minutes and will show progress output.

---

## Step 4: Verify K3s Is Running

```bash
sudo systemctl status k3s
```

> You should see `Active: active (running)` in the output.
> Press `Ctrl+C` to exit the status view.

---

## Step 5: Configure `kubectl` to Use K3s

K3s stores its kubeconfig at `/etc/rancher/k3s/k3s.yaml`, which is root-owned. To use it with your user:

```bash
# Create local kube config directory
mkdir -p ~/.kube

# Copy K3s config to user directory
sudo k3s kubectl config view --raw > ~/.kube/config

# Set proper permissions
chmod 600 ~/.kube/config

# Set environment variable for current session
export KUBECONFIG=~/.kube/config

# Make it permanent
echo 'export KUBECONFIG=~/.kube/config' >> ~/.bashrc
source ~/.bashrc
```

> This copies the K3s configuration to your user directory with proper permissions.

---

## Step 6: Test Your Cluster

```bash
kubectl get nodes
```

> You should see output similar to:
> ```
> NAME               STATUS   ROLES                  AGE   VERSION
> ip-172-31-80-248   Ready    control-plane,master   83s   v1.33.3+k3s1
> ```

---

## What's Next?

You now have a fully functional Kubernetes cluster. You can:
- Deploy apps using `kubectl apply -f <manifest.yaml>`
- Install Helm and deploy charts
- Expose services via LoadBalancer or NodePort
- Monitor with tools like Lens or K9s

---

## Troubleshooting

If you see permission errors with kubectl:
```bash
# Make sure KUBECONFIG is set correctly
echo $KUBECONFIG

# Should output: /home/ubuntu/.kube/config
```

If K3s service fails to start:
```bash
sudo systemctl restart k3s
sudo systemctl status k3s
```

---

**Congratulations! Your K3s cluster is ready for deployments.**
