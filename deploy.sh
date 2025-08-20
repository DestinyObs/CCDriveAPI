#!/bin/bash

echo "==========================================
Starting TheDrive API Deployment
=========================================="

cd /home/ubuntu/thedrive-upload || exit 1

echo "Current directory: $(pwd)"
echo "Files in directory:"
ls -la

echo
echo "Checking Docker and Docker Compose..."
docker --version
docker-compose --version

echo
echo "Stopping any existing containers..."
sudo docker-compose down --remove-orphans 2>/dev/null || true

echo
echo "Removing any existing images..."
sudo docker system prune -f

echo
echo "Building and starting TheDrive API..."
sudo docker-compose up --build -d

echo
echo "Waiting for services to start..."
sleep 30

echo
echo "Checking container status..."
sudo docker-compose ps

echo
echo "Checking API container logs..."
sudo docker-compose logs api

echo
echo "==========================================
Deployment Complete!
=========================================="