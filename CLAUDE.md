# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Pizza API project built with ASP.NET Core 8.0. The API is designed as a monolithic REST service for managing online pizza orders with a MySQL database backend.

## Database Schema

The project uses a relational model with the following entities:
- CUSTOMER (id, first_name, last_name, email, password_hash, order_amount)
- ORDER (id, orderDate, status, total_price)
- PIZZA (id, name, price)
- ORDER_ITEM (quantity, price)
- PIZZA_BASE (id, name)
- PIZZA_INGREDIENT (id, name)

Relationships:
- Customer 1:N Order
- Order 1:N OrderItem
- Pizza 1:N OrderItem
- PizzaBase 1:N Pizza
- PizzaIngredient N:N Pizza

## Development Commands

### Running the Application
```bash
# Run locally
dotnet run --project PizzaAPI

# Run with hot reload
dotnet watch --project PizzaAPI

# Run specific profile
dotnet run --project PizzaAPI --launch-profile https
```

### Building and Testing
```bash
# Build the project
dotnet build

# Build in release mode
dotnet build -c Release

# Restore packages
dotnet restore
```

### Docker Commands
```bash
# Build Docker image
docker build -t pizzaapi .

# Run in container
docker run -p 8080:8080 -p 8081:8081 pizzaapi
```

### Docker Compose Commands
```bash
# Start all services (webapp + MySQL + phpMyAdmin)
docker-compose up -d

# Build and start services
docker-compose up --build -d

# Stop all services
docker-compose down

# Stop and remove volumes (clears database data)
docker-compose down -v

# View logs
docker-compose logs pizzaapi
docker-compose logs mysql

# Follow logs in real-time
docker-compose logs -f pizzaapi

# HTTP-only development (no HTTPS certificates needed)
docker-compose -f docker-compose.http-only.yml up -d
```

### Certificate Setup for HTTPS
```powershell
# Run PowerShell script to setup development certificates
.\setup-dev-cert.ps1

# Or manually create .env file with your certificate password
echo "CERT_PASSWORD=your_password_here" > .env
```

## Project Structure

- `PizzaAPI/` - Main project directory
  - `Controllers/` - API controllers (currently empty)
  - `Program.cs` - Application entry point and configuration
  - `appsettings.json` - Application configuration
  - `Dockerfile` - Docker configuration
  - `PizzaAPI.http` - HTTP test requests
- `docker-compose.yml` - Multi-container orchestration
- `docker-compose.override.yml` - Development-specific overrides
- `docker-compose.http-only.yml` - HTTP-only configuration (no certificates)
- `docker-compose.dcproj` - Visual Studio Docker Compose project
- `init-db.sql` - Database initialization script
- `setup-dev-cert.ps1` - PowerShell script for certificate setup
- `.env.example` - Environment variables template

## Configuration

### Local Development
- **Default HTTP URL**: http://localhost:5059
- **Default HTTPS URL**: https://localhost:7005
- **Swagger**: Available at `/swagger` endpoint in development

### Docker Compose Environment
- **Pizza API HTTP**: http://localhost:8080
- **Pizza API HTTPS**: https://localhost:8081
- **MySQL Database**: localhost:3306
- **phpMyAdmin**: http://localhost:8082
- **Database Credentials**: 
  - Database: PizzaDB
  - User: pizzauser
  - Password: pizzapass
  - Root Password: rootpassword

## Development Environment

- Target Framework: .NET 8.0
- Nullable reference types enabled
- Implicit usings enabled
- Docker support for Linux containers
- Swagger/OpenAPI integration for API documentation

## Launch Configurations

The project includes multiple launch profiles in Visual Studio:

1. **http** - Local development (http://localhost:5059)
2. **https** - Local development with HTTPS (https://localhost:7005)
3. **IIS Express** - IIS Express hosting
4. **Container (Dockerfile)** - Single container deployment
5. **Docker Compose** - Full stack deployment with database

### Visual Studio Launch
- Select "Docker Compose" as startup project to launch with full database stack
- Select "PizzaAPI" project with desired profile for local development
- Docker Compose profile automatically starts MySQL and phpMyAdmin services

## HTTP Testing

Use the `PizzaAPI.http` file for testing API endpoints. Currently configured to test:
- GET /PizzaList/ endpoint on http://localhost:5059

## Troubleshooting

### Certificate Issues
If you encounter certificate password errors:

1. **Use HTTP-only mode**: `docker-compose -f docker-compose.http-only.yml up -d`
2. **Run certificate setup script**: `.\setup-dev-cert.ps1`
3. **Manually set certificate password**: Create `.env` file with `CERT_PASSWORD=your_password`
4. **Check certificate location**: Ensure certificate exists at `~/.aspnet/https/aspnetapp.pfx`

### Common Errors
- **Certificate password incorrect**: Update `CERT_PASSWORD` in `.env` file
- **Certificate not found**: Run `dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p yourpassword`
- **Permission denied**: Run PowerShell as Administrator for certificate setup