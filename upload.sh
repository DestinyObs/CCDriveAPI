#!/bin/bash

# TheDrive API Upload and Deployment Script
# This script uploads the project to your Ubuntu server and runs docker-compose

echo "=========================================="
echo "TheDrive API Deployment Script"
echo "=========================================="

# Server configuration
SERVER_IP="44.203.4.39"
SERVER_USER="ubuntu"
PEM_KEY="C:/Users/Desti/Downloads/ubuntu.pem"
REMOTE_DIR="/home/ubuntu/thedrive-upload"
PROJECT_DIR="C:/Users/Desti/CCDriveAPI"

echo "Server: $SERVER_USER@$SERVER_IP"
echo "Remote directory: $REMOTE_DIR"
echo "Local project: $PROJECT_DIR"
echo ""

# Create remote directory
echo "Creating remote directory..."
ssh -i "$PEM_KEY" -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" "mkdir -p $REMOTE_DIR"

# Upload project files
echo "Uploading project files..."
echo "- Uploading source code and configuration files..."
echo "- Uploading Kubernetes manifests (k8s/)..."
echo "- Uploading Helm chart (helm/)..."

# Upload all necessary files
scp -i "$PEM_KEY" -o StrictHostKeyChecking=no -r \
    "$PROJECT_DIR/Controllers" \
    "$PROJECT_DIR/Data" \
    "$PROJECT_DIR/DTOs" \
    "$PROJECT_DIR/Migrations" \
    "$PROJECT_DIR/Models" \
    "$PROJECT_DIR/Services" \
    "$PROJECT_DIR/Swagger" \
    "$PROJECT_DIR/Program.cs" \
    "$PROJECT_DIR/TheDriveAPI.csproj" \
    "$PROJECT_DIR/appsettings.json" \
    "$PROJECT_DIR/appsettings.Production.json" \
    "$PROJECT_DIR/Dockerfile" \
    "$PROJECT_DIR/docker-compose.yml" \
    "$PROJECT_DIR/.env" \
    "$PROJECT_DIR/init-db.sql" \
    "$PROJECT_DIR/k8s" \
    "$PROJECT_DIR/helm" \
    "$SERVER_USER@$SERVER_IP:$REMOTE_DIR/"

echo "- Upload completed!"
echo ""
echo "=========================================="
echo "Files uploaded successfully!"
echo "=========================================="
echo ""
echo "Available deployment options:"
echo "1. Docker Compose: cd $REMOTE_DIR && docker-compose up -d"
echo "2. Kubernetes: kubectl apply -f k8s/manifests/"
echo "3. Helm: helm install thedrive ./helm/thedrive -n thedrive --create-namespace"
echo ""
echo "To connect to server:"
echo "ssh -i '$PEM_KEY' -o StrictHostKeyChecking=no $SERVER_USER@$SERVER_IP"
echo ""