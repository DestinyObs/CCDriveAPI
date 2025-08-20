# TheDrive API

> **Current Status**: Production Ready - Deployed on EC2 with Docker Compose  
> **Live API**: `http://your-server-ip:5256`  
> **Swagger UI**: `http://your-server-ip:5256/swagger`  
> **Docker Hub**: `destinyobs/thedrive-upload-api:latest`

A robust cloud storage backend API built with .NET 9, featuring JWT authentication, file management, and subscription plans.

## Quick Start

### Already Deployed? Check Status
```bash
# Connect to your server
ssh ubuntu@your-server-ip

# Check containers
docker ps

# View logs
docker logs thedrive-api
```

### Fresh Deployment
```bash
# 1. Upload project to server
scp -r . ubuntu@your-server:/home/ubuntu/thedrive-upload/

# 2. Deploy with Docker Compose
cd /home/ubuntu/thedrive-upload
docker-compose up -d

# 3. Access API
curl http://your-server:5256/swagger
```

## Project Structure

```
TheDriveAPI/
├── Controllers/          # API endpoints
│   ├── AuthController    # Login, register, OTP
│   ├── UserController    # Profile, settings
│   ├── FileController    # Upload, download, manage
│   ├── FolderController  # Create, organize folders
│   ├── PricingController # Plans, subscriptions
│   └── SupportController # Help desk, tickets
├── Services/             # Business logic
├── Models/              # Database entities
├── DTOs/               # Request/response objects
├── Data/               # Database context & migrations
└── Migrations/         # EF Core schema changes
```

## Tech Stack

- **Framework**: .NET 9 Web API
- **Database**: SQL Server 2022 (via Docker)
- **Auth**: JWT Bearer tokens + ASP.NET Identity
- **Storage**: AWS S3 + Ceph (configurable)
- **Email**: SendGrid
- **Container**: Docker + Docker Compose
- **Deployment**: EC2 Ubuntu

## Key Features

### Authentication & Security
- JWT-based authentication
- Email OTP verification
- Password reset flow
- Role-based authorization

### File Management
- File upload/download with S3 storage
- Folder organization & hierarchy
- File versioning system
- Share files with permissions

### User Management
- User profiles & preferences
- Storage usage tracking
- Account deletion & data export

### Subscription System
- Multiple pricing tiers (Free, Pro, Business, Enterprise)
- Storage limits enforcement
- Subscription management

### Support System
- Help desk ticketing
- FAQ management
- Contact forms

## Environment Configuration

### Required Environment Variables
```env
# Database
SA_PASSWORD=
DB_NAME=TheDriveAPI
CONNECTION_STRING=Server=mssql,1433;Database=TheDriveAPI;...

# JWT
JWT_SECRET=YourSuperSecretJWTKeyThatShouldBeAtLeast32CharactersLong!
JWT_ISSUER=
JWT_AUDIENCE=

# Email (SendGrid)
SENDGRID_API_KEY=your-api-key
SENDGRID_FROM_EMAIL=noreply@thedrive.com

# File Storage (S3)
S3_ACCESS_KEY=your-access-key
S3_SECRET_KEY=your-secret-key
S3_BUCKET=thedrive-files

# App Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5256
```

## Deployment Process

### Docker Compose (Current Setup)
```bash
# 1. Build and start services
docker-compose up -d

# 2. Check status
docker-compose ps

# 3. View logs
docker-compose logs -f api

# 4. Stop services
docker-compose down
```

### Manual Container Build
```bash
# Build API image
docker build -t thedrive-api .

# Tag for Docker Hub
docker tag thedrive-api destinyobs/thedrive-upload-api:latest

# Push to registry
docker push destinyobs/thedrive-upload-api:latest
```

## API Endpoints

### Authentication (`/api/auth`)
- `POST /register` - Create new account
- `POST /login` - User login
- `POST /verify-otp` - Email verification
- `POST /forgot-password` - Password reset request
- `POST /reset-password` - Complete password reset

### User Management (`/api/user`)
- `GET /profile` - Get user profile
- `PATCH /profile` - Update profile
- `PATCH /password` - Change password
- `GET /usage` - Storage usage stats

### File Operations (`/api/files`)
- `POST /upload` - Upload file
- `GET /{id}/download` - Download file
- `DELETE /{id}` - Delete file
- `PATCH /{id}/rename` - Rename file
- `POST /{id}/share` - Share file

### Folder Management (`/api/folders`)
- `POST /` - Create folder
- `GET /` - List folders
- `DELETE /{id}` - Delete folder
- `PATCH /{id}` - Rename folder

### Pricing & Plans (`/api/pricing`)
- `GET /plans` - List available plans
- `POST /subscribe` - Subscribe to plan
- `POST /cancel` - Cancel subscription

## Database Schema

### Core Models
- **User** - Identity user with profile data
- **File** - File metadata with S3 keys
- **Folder** - Hierarchical folder structure
- **Plan** - Subscription plans with limits
- **Subscription** - User plan subscriptions
- **Activity** - User activity tracking
- **SupportTicket** - Help desk tickets

## Troubleshooting

### Common Issues

**Swagger not loading?**
```bash
# Check if API is running
curl http://localhost:5256/swagger

# Verify Swagger is enabled in Program.cs
# Should have: app.UseSwagger(); app.UseSwaggerUI();
```

**Database connection failed?**
```bash
# Check SQL Server container
docker logs mssql-server

# Verify connection string in .env file
# Test connection manually
docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "password" -C
```

**Build failing?**
```bash
# Check .NET version
dotnet --version  # Should be 9.x

# Clear and restore
dotnet clean
dotnet restore
dotnet build
```

## Development Workflow

### Local Development
```bash
# 1. Start SQL Server
docker run -d --name sqlserver -e ACCEPT_EULA=Y -e SA_PASSWORD=password -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

# 2. Update connection string in appsettings.Development.json

# 3. Run migrations
dotnet ef database update

# 4. Start API
dotnet run
```

### Making Changes
```bash
# 1. Create new migration
dotnet ef migrations add YourChangeName

# 2. Update database
dotnet ef database update

# 3. Test locally
dotnet run

# 4. Build and deploy
docker build -t thedrive-api .
docker-compose up -d
```

## Monitoring & Logs

### Container Logs
```bash
# API logs
docker logs thedrive-api -f

# Database logs
docker logs mssql-server -f

# All services
docker-compose logs -f
```

### Health Checks
```bash
# API health
curl http://your-server-ip:5256/api/health

# Database health
docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "password" -C -Q "SELECT 1"
```

## Next Steps: K8s Migration

Ready to move to Kubernetes? The current Docker setup is K8s-ready:
- All configs are externalized via environment variables
- Health checks implemented
- Persistent volumes configured
- Service separation complete

---

**Need help?** Check the logs first, then verify environment variables. The API is designed to be self-documenting via Swagger UI.
