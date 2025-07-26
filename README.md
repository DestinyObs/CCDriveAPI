# CyberCloud Drive API

This is a .NET 8 Web API backend for CyberCloud Drive, using MSSQL and JWT authentication. Includes Docker support.

## Features
- RESTful endpoints for authentication, users, files, folders, subscriptions, support, etc.
- JWT authentication
- MSSQL database
- Dockerfile for containerization
- CORS support
- Error handling

## Getting Started
1. Install .NET 8 SDK and MSSQL
2. Update connection string in `appsettings.json`
3. Run migrations and seed data
4. Build and run the API

## Docker
To build and run with Docker:
```sh
docker build -t cybercloud-drive-api .
docker run -p 5000:80 cybercloud-drive-api
```
