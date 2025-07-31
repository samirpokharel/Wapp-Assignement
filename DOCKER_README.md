# SimpleLMS Docker Setup

This document provides instructions for running the SimpleLMS application using Docker.

## Prerequisites

- Docker installed on your system
- Docker Compose (usually comes with Docker Desktop)

## Quick Start

### Production Build

1. **Build and run the production container:**
   ```bash
   docker-compose up --build
   ```

2. **Access the application:**
   - Open your browser and go to `http://localhost:8080`

### Development Build (with Hot Reload)

1. **Build and run the development container:**
   ```bash
   docker-compose --profile dev up --build
   ```

2. **Access the application:**
   - Open your browser and go to `http://localhost:5000`

## Docker Commands

### Build the Image
```bash
# Production build
docker build -t simplelms:latest .

# Development build
docker build -f Dockerfile.dev -t simplelms:dev .
```

### Run the Container
```bash
# Production
docker run -p 8080:8080 simplelms:latest

# Development
docker run -p 5000:5000 -v $(pwd):/app simplelms:dev
```

### Using Docker Compose

```bash
# Start production services
docker-compose up

# Start development services
docker-compose --profile dev up

# Run in background
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

## Environment Variables

The following environment variables can be configured:

- `ASPNETCORE_ENVIRONMENT`: Set to `Production` or `Development`
- `ASPNETCORE_URLS`: The URL the application will listen on
- `ConnectionStrings__DefaultConnection`: Database connection string (optional)

## Volumes

The following directories are mounted as volumes:

- `./wwwroot/uploads:/app/wwwroot/uploads` - Uploaded files
- `./data:/app/data` - Database files (optional)

## Health Check

The application includes a health check endpoint at `/health` that returns:

```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.000Z"
}
```

## Troubleshooting

### Common Issues

1. **Port already in use:**
   ```bash
   # Check what's using the port
   lsof -i :8080
   
   # Kill the process or change the port in docker-compose.yml
   ```

2. **Permission denied:**
   ```bash
   # Fix permissions for uploads directory
   sudo chown -R $USER:$USER ./wwwroot/uploads
   ```

3. **Database issues:**
   ```bash
   # Remove existing database
   rm -f app.db
   
   # Rebuild container
   docker-compose down
   docker-compose up --build
   ```

### Viewing Logs

```bash
# View application logs
docker-compose logs simplelms

# Follow logs in real-time
docker-compose logs -f simplelms

# View logs for specific service
docker logs <container_name>
```

### Accessing the Container

```bash
# Access running container
docker exec -it <container_name> /bin/bash

# Access container with specific shell
docker exec -it <container_name> /bin/sh
```

## Production Deployment

For production deployment, consider:

1. **Using a reverse proxy (nginx):**
   ```yaml
   # Add to docker-compose.yml
   nginx:
     image: nginx:alpine
     ports:
       - "80:80"
       - "443:443"
     volumes:
       - ./nginx.conf:/etc/nginx/nginx.conf
     depends_on:
       - simplelms
   ```

2. **Using environment-specific configuration:**
   ```bash
   # Create production environment file
   cp appsettings.json appsettings.Production.json
   
   # Set production environment
   export ASPNETCORE_ENVIRONMENT=Production
   ```

3. **Database considerations:**
   - Use external database (PostgreSQL, SQL Server)
   - Configure connection strings
   - Set up database migrations

## Security Considerations

1. **Never commit sensitive data:**
   - Use environment variables for secrets
   - Add `.env` files to `.gitignore`

2. **Update base images regularly:**
   ```bash
   # Update base images
   docker pull mcr.microsoft.com/dotnet/aspnet:8.0
   docker pull mcr.microsoft.com/dotnet/sdk:8.0
   ```

3. **Use non-root user (optional):**
   ```dockerfile
   # Add to Dockerfile
   RUN adduser --disabled-password --gecos '' appuser
   USER appuser
   ```

## Performance Optimization

1. **Multi-stage builds** (already implemented)
2. **Layer caching** (already optimized)
3. **Resource limits:**
   ```yaml
   # Add to docker-compose.yml
   deploy:
     resources:
       limits:
         memory: 512M
         cpus: '0.5'
   ```

## Monitoring

1. **Health checks** (already configured)
2. **Logging** (structured logging with Serilog recommended)
3. **Metrics** (consider adding Prometheus metrics)

## Backup and Restore

```bash
# Backup database
docker exec <container_name> sqlite3 /app/app.db ".backup '/app/backup.db'"

# Restore database
docker exec <container_name> sqlite3 /app/app.db ".restore '/app/backup.db'"
```

## Cleanup

```bash
# Remove all containers and images
docker-compose down --rmi all --volumes --remove-orphans

# Clean up unused Docker resources
docker system prune -a
``` 