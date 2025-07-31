# SimpleLMS Deployment Guide

This guide provides step-by-step instructions for deploying the SimpleLMS application in various environments.

## Prerequisites

- Docker and Docker Compose installed
- Git (for version control)
- Basic knowledge of Docker and containerization

## Quick Deployment

### 1. Development Environment

```bash
# Clone the repository (if not already done)
git clone <repository-url>
cd SimpleLMS

# Start development environment
./build.sh dev

# Access the application
open http://localhost:5000
```

### 2. Production Environment

```bash
# Start production environment with nginx
./build.sh prod

# Access the application
open http://localhost:80
```

## Manual Deployment Steps

### Step 1: Build the Docker Image

```bash
# Build production image
docker build -t simplelms:latest .

# Verify the image was created
docker images | grep simplelms
```

### Step 2: Run the Container

```bash
# Run with port mapping
docker run -d -p 8080:8080 --name simplelms-app simplelms:latest

# Check if container is running
docker ps | grep simplelms
```

### Step 3: Verify Deployment

```bash
# Check container logs
docker logs simplelms-app

# Test health endpoint
curl http://localhost:8080/health
```

## Environment-Specific Configurations

### Development Environment

```bash
# Use development compose file
docker-compose --profile dev up -d

# View logs
docker-compose logs -f simplelms-dev
```

### Production Environment

```bash
# Use production compose file
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f
```

## Database Management

### SQLite Database (Default)

The application uses SQLite by default, which is stored in the container. For persistence:

```bash
# Create data directory
mkdir -p ./data

# Run with volume mount
docker run -d -p 8080:8080 \
  -v $(pwd)/data:/app/data \
  --name simplelms-app simplelms:latest
```

### External Database (Recommended for Production)

For production, consider using PostgreSQL or SQL Server:

1. **Update connection string in appsettings.Production.json:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=db-server;Database=SimpleLMS;User Id=user;Password=password;"
     }
   }
   ```

2. **Add database service to docker-compose.prod.yml:**
   ```yaml
   postgres:
     image: postgres:15
     environment:
       POSTGRES_DB: SimpleLMS
       POSTGRES_USER: user
       POSTGRES_PASSWORD: password
     volumes:
       - postgres_data:/var/lib/postgresql/data
   ```

## SSL/HTTPS Setup

### Using Let's Encrypt with nginx

1. **Create SSL directory:**
   ```bash
   mkdir -p ./ssl
   ```

2. **Generate SSL certificate:**
   ```bash
   # For development (self-signed)
   openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
     -keyout ./ssl/nginx.key -out ./ssl/nginx.crt
   ```

3. **Update nginx.conf for HTTPS:**
   ```nginx
   server {
       listen 443 ssl;
       ssl_certificate /etc/nginx/ssl/nginx.crt;
       ssl_certificate_key /etc/nginx/ssl/nginx.key;
       # ... rest of configuration
   }
   ```

## Monitoring and Logging

### Health Checks

The application includes a health check endpoint:

```bash
# Test health endpoint
curl http://localhost:8080/health

# Expected response
{"status":"healthy","timestamp":"2024-01-01T00:00:00.000Z"}
```

### Log Management

```bash
# View application logs
docker logs simplelms-app

# Follow logs in real-time
docker logs -f simplelms-app

# Export logs to file
docker logs simplelms-app > app.log
```

### Resource Monitoring

```bash
# Check container resource usage
docker stats simplelms-app

# Check disk usage
docker system df
```

## Backup and Recovery

### Database Backup

```bash
# Backup SQLite database
docker exec simplelms-app sqlite3 /app/app.db ".backup '/app/backup.db'"

# Copy backup from container
docker cp simplelms-app:/app/backup.db ./backup.db
```

### Application Backup

```bash
# Create backup of uploads
tar -czf uploads-backup.tar.gz ./wwwroot/uploads/

# Create backup of entire application
docker commit simplelms-app simplelms:backup-$(date +%Y%m%d)
```

## Troubleshooting

### Common Issues

1. **Port already in use:**
   ```bash
   # Find process using port
   lsof -i :8080
   
   # Kill process or change port
   docker run -p 8081:8080 simplelms:latest
   ```

2. **Permission denied:**
   ```bash
   # Fix uploads directory permissions
   sudo chown -R $USER:$USER ./wwwroot/uploads/
   ```

3. **Container won't start:**
   ```bash
   # Check container logs
   docker logs simplelms-app
   
   # Check if port is available
   netstat -tulpn | grep :8080
   ```

### Debug Mode

```bash
# Run container in interactive mode
docker run -it --rm -p 8080:8080 simplelms:latest /bin/bash

# Access running container
docker exec -it simplelms-app /bin/bash
```

## Performance Optimization

### Resource Limits

```yaml
# Add to docker-compose.yml
deploy:
  resources:
    limits:
      memory: 512M
      cpus: '0.5'
    reservations:
      memory: 256M
      cpus: '0.25'
```

### Caching

The nginx configuration includes caching for static files. For additional caching:

```nginx
# Add to nginx.conf
location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

## Security Considerations

### Environment Variables

Never commit sensitive data. Use environment variables:

```bash
# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-connection-string"

# Run with environment variables
docker run -e ASPNETCORE_ENVIRONMENT=Production simplelms:latest
```

### Network Security

```bash
# Use custom network
docker network create simplelms-network

# Run with custom network
docker run --network simplelms-network simplelms:latest
```

### Regular Updates

```bash
# Update base images
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
docker pull mcr.microsoft.com/dotnet/sdk:8.0

# Rebuild application
docker build --no-cache -t simplelms:latest .
```

## Scaling

### Horizontal Scaling

```bash
# Scale application
docker-compose up --scale simplelms=3

# Use load balancer
docker run -d --name nginx-lb \
  -p 80:80 \
  -v $(pwd)/nginx-lb.conf:/etc/nginx/nginx.conf \
  nginx:alpine
```

### Load Balancer Configuration

```nginx
# nginx-lb.conf
upstream simplelms {
    server simplelms:8080;
    server simplelms:8081;
    server simplelms:8082;
}
```

## Cleanup

```bash
# Stop and remove containers
docker-compose down

# Remove images
docker rmi simplelms:latest

# Clean up unused resources
docker system prune -a

# Remove volumes (WARNING: This will delete data)
docker volume prune
```

## Support

For issues and questions:

1. Check the logs: `docker logs <container-name>`
2. Verify configuration files
3. Test connectivity: `curl http://localhost:8080/health`
4. Check resource usage: `docker stats`

## Next Steps

After successful deployment:

1. Set up monitoring (Prometheus, Grafana)
2. Configure automated backups
3. Set up CI/CD pipeline
4. Implement logging aggregation
5. Configure alerting 