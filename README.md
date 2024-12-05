# Street Service Application

## Overview

This project implements a REST API for managing street data using ASP.NET Core, Entity Framework Core, PostgreSQL with PostGIS, and Docker. It includes features for creating, modifying, and querying street geometries.

## Project Structure

```
StreetService/
├── Infrastructure/
│   ├── docker-compose.yml
│   └── kubernetes.yaml
├── StreetService/
│   ├── Controllers/
│   │   └── StreetsController.cs
│   ├── Data/
│   │   └── StreetContext.cs
│   ├── Models/
│   │   ├── CreateStreetDto.cs
│   │   ├── ModifyStreetRequest.cs
│   │   └── Street.cs
│   ├── Program.cs
│   └── Startup.cs
└── Dockerfile
```

## How to Run Locally

1. Clone the repository:
   ```
   git clone https://github.com/your-username/street-service.git
   cd street-service
   ```

2. Build the Docker image:
   ```
   docker build -t street-service .
   ```

3. Start the application using Docker Compose:
   ```
   docker-compose up --scale street-service=3
   ```

4. Access the API:
   - Base URL: http://localhost:5001
   - API endpoints:
     - POST /api/streets
     - GET /api/streets
     - PUT /api/streets/{name}/modify

5. Access pgAdmin at: http://localhost:8081

## Kubernetes Deployment

1. Apply the Kubernetes manifest:
   ```
   kubectl apply -f Infrastructure/kubernetes.yaml
   ```

2. Verify deployment:
   ```
   kubectl get deployments
   kubectl get pods
   ```

## Features Implemented

1. REST API endpoints for creating and querying streets
2. Street modification with point addition
3. Race condition handling using optimistic concurrency
4. Feature flag for database-level vs algorithmic operations

## Technology Stack

- ASP.NET Core (Controller-based approach)
- Entity Framework Core
- PostgreSQL with PostGIS
- Docker
- Kubernetes

## Key Components

1. **Controllers**: Handles HTTP requests and defines endpoints (StreetsController.cs)
2. **Program.cs**: Entry point for the application
3. **StreetAPI.csproj**: Project file managing dependencies and configuration
4. **StreetContext.cs**: Database context coordinating Entity Framework functionality
5. **CreateStreetDto.cs**: DTO for creating streets
6. **ModifyStreetRequest.cs**: Request model for modifying streets

## Database Setup

1. PostgreSQL image: `postgis/postgis`
2. Connection string format:
   ```
   "DockerConnection": "Host=postgres;Port=5432;Database=streetsdb;Username=postgres;Password=mysecretpassword;"
   ```

## Docker Configuration

- Uses separate DTOs for data transfer control
- Implements health checks for database readiness
- Utilizes PostGIS for spatial data handling
