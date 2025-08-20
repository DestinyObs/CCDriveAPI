# TheDrive API Smart Upload and Deployment Script (PowerShell)
# This script syncs only new/modified files to your Ubuntu server using rsync

Write-Host "==========================================" -ForegroundColor Green
Write-Host "TheDrive API Smart Sync Script" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Server configuration
$SERVER_IP = "100.27.25.7"
$SERVER_USER = "ubuntu"
$PEM_KEY = "C:\Users\Desti\Downloads\ubuntu.pem"
$REMOTE_DIR = "/home/ubuntu/thedrive-upload"
$PROJECT_DIR = "C:\Users\Desti\CCDriveAPI"

Write-Host "Server: $SERVER_USER@$SERVER_IP" -ForegroundColor Cyan
Write-Host "Remote directory: $REMOTE_DIR" -ForegroundColor Cyan
Write-Host "Local project: $PROJECT_DIR" -ForegroundColor Cyan
Write-Host ""

# Create remote directory
Write-Host "Creating remote directory..." -ForegroundColor Yellow
& ssh -i "$PEM_KEY" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "mkdir -p $REMOTE_DIR"

# Check if rsync is available
Write-Host "Checking for rsync..." -ForegroundColor Yellow
$rsyncExists = Get-Command rsync -ErrorAction SilentlyContinue

if ($rsyncExists) {
    Write-Host "Using rsync for smart sync (only uploads new/modified files)..." -ForegroundColor Green
    Write-Host ""
    
    # Use rsync to sync only changed files
    & rsync -avz --progress --delete `
        -e "ssh -i `"$PEM_KEY`" -o StrictHostKeyChecking=no" `
        --exclude='.git' `
        --exclude='bin' `
        --exclude='obj' `
        --exclude='*.tmp' `
        --exclude='*.log' `
        --exclude='.vs' `
        --exclude='node_modules' `
        "$PROJECT_DIR/" `
        "$SERVER_USER@$SERVER_IP`:$REMOTE_DIR/"
        e
    Write-Host ""
    Write-Host "Smart sync completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Uploading updated Helm chart to server..." -ForegroundColor Yellow
    
    # Upload the updated helm chart specifically
    & scp -i "$PEM_KEY" -o StrictHostKeyChecking=no -r "$PROJECT_DIR\helm" "$SERVER_USER@$SERVER_IP`:$REMOTE_DIR/"

} else {
    Write-Host "rsync not found, using scp for full upload..." -ForegroundColor Yellow
    Write-Host ""
    
    # Fallback to scp for specific files
    Write-Host "- Uploading source code and configuration files..." -ForegroundColor White
    Write-Host "- Uploading Kubernetes manifests (k8s/)..." -ForegroundColor White
    Write-Host "- Uploading Helm chart (helm/)..." -ForegroundColor White

    # Upload all necessary files
    & scp -i "$PEM_KEY" -o StrictHostKeyChecking=no -r `
        "$PROJECT_DIR\Controllers" `
        "$PROJECT_DIR\Data" `
        "$PROJECT_DIR\DTOs" `
        "$PROJECT_DIR\Migrations" `
        "$PROJECT_DIR\Models" `
        "$PROJECT_DIR\Services" `
        "$PROJECT_DIR\Swagger" `
        "$PROJECT_DIR\Program.cs" `
        "$PROJECT_DIR\TheDriveAPI.csproj" `
        "$PROJECT_DIR\appsettings.json" `
        "$PROJECT_DIR\appsettings.Production.json" `
        "$PROJECT_DIR\Dockerfile" `
        "$PROJECT_DIR\docker-compose.yml" `
        "$PROJECT_DIR\.env" `
        "$PROJECT_DIR\init-db.sql" `
        "$PROJECT_DIR\k8s" `
        "$PROJECT_DIR\helm" `
        "$SERVER_USER@$SERVER_IP`:$REMOTE_DIR/"
        
    Write-Host "Upload completed!" -ForegroundColor Green
}
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Files uploaded successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Available deployment options:" -ForegroundColor Cyan
Write-Host "1. Docker Compose: cd $REMOTE_DIR && docker-compose up -d" -ForegroundColor White
Write-Host "2. Kubernetes: kubectl apply -f k8s/manifests/" -ForegroundColor White
Write-Host "3. Helm: helm install thedrive ./helm/thedrive -n thedrive --create-namespace" -ForegroundColor White
Write-Host ""
Write-Host "To connect to server:" -ForegroundColor Cyan
Write-Host "ssh -i '$PEM_KEY' -o StrictHostKeyChecking=no $SERVER_USER@$SERVER_IP" -ForegroundColor Yellow
Write-Host ""

# Optional: Ask if user wants to connect to server
$connect = Read-Host "Do you want to connect to the server now? (y/n)"
if ($connect -eq "y" -or $connect -eq "Y") {
    Write-Host "Connecting to server..." -ForegroundColor Green
    & ssh -i "$PEM_KEY" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP"
}