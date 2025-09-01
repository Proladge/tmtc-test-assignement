# Task Management API

A simple REST API for managing Users and UserTasks with automatic task assignment and intelligent task rotation.

> **Note**: This is a proof-of-concept developed for an assignment. See [Assumptions & Scope](ASSUMPTIONS.md) for details about production-readiness considerations that were intentionally scoped out.

## Features

- ✅ **User Management** - CRUD operations with unique name constraints
- ✅ **Task Management** - CRUD operations with automatic assignment
- ✅ **Task Rotation** - Automatic reassignment every 2 minutes with intelligent rules
- ✅ **Fair Distribution** - Ensures all users work on all tasks eventually  
- ✅ **Capacity Management** - Maximum 3 active tasks per user
- ✅ **Assignment History** - Complete audit trail of all task assignments
- ✅ **Auto-Completion** - Tasks complete when assigned to all users

## Quick Start

### Prerequisites
- .NET 9.0 SDK

### Run the Application
```bash
cd TaskManagementApi
dotnet run
```

The API will be available at `https://localhost:7076` with Swagger UI at `/swagger`.


## Testing the API

### Option 1: Swagger UI
Navigate to `https://localhost:7076/swagger` for an interactive API interface.

### Option 2: Postman Collection
Import the included Postman collection for comprehensive testing:
1. Import `TaskManagementApi.postman_collection.json`
2. Import `TaskManagementApi.postman_environment.json`
3. Select "Task Management API - Development" environment

## Core Business Rules

- **Unique Constraints**: User names and task titles must be unique
- **Task Limits**: Users can have maximum 3 active tasks
- **Auto-Assignment**: New tasks automatically assigned to available users
- **Smart Reassignment**: Every 2 minutes, tasks rotate to different users following intelligent rules:
  - Cannot go to current or previous user
  - Prioritizes users who haven't worked on the task yet
  - Respects capacity limits
  - Tasks complete when assigned to all users

## Documentation

For detailed technical specifications, API schemas, and implementation details, see:
- **[Technical Specification](TECHNICAL-SPECIFICATION.md)** - Complete technical documentation
- **[Original Requirements](original-requirements.md)** - Original assignment requirements
- **[Assumptions & Scope](ASSUMPTIONS.md)** - Project assumptions and scope decisions

## Technology

Built with .NET 9.0, ASP.NET Core Web API, and in-memory storage for easy deployment and testing.
