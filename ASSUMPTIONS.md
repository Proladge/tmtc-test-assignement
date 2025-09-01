# Project Assumptions and Scope Decisions

## Context

This project was developed as an **assignment/demonstration** rather than a production-ready application. The following assumptions and decisions were made to focus on core functionality and rapid delivery of a working proof-of-concept.

## Core Assumptions

### 1. **No Security Requirements**
- **No Authentication**: No user login/logout, session management, or user identity verification
- **No Authorization**: No role-based access control, permissions, or resource protection
- **No API Security**: No API keys, JWT tokens, OAuth, or any security headers
- **No Input Sanitization**: Basic validation only, no protection against injection attacks
- **No Rate Limiting**: No protection against abuse or excessive API calls

**Rationale**: Security implementation would significantly increase complexity and development time without demonstrating the core business logic requirements.

### 2. **No Persistent Data Storage**
- **In-Memory Only**: All data stored in `ConcurrentDictionary` collections
- **No Database**: No SQL Server, PostgreSQL, MongoDB, or any persistent storage
- **No Data Migration**: No schema versioning or data evolution strategies
- **Data Loss on Restart**: All data is lost when application stops

**Rationale**: Database setup, configuration, and ORM integration would add deployment complexity and infrastructure dependencies not required for demonstrating the assignment logic.

### 3. **Limited Non-Functional Requirements**

#### **Performance & Scalability**
- No load balancing or horizontal scaling considerations
- No caching strategies beyond in-memory collections  
- No performance benchmarking or optimization beyond basic concurrent programming
- No connection pooling or resource management optimizations

#### **Reliability & Availability**
- No error recovery or retry mechanisms
- No circuit breakers or fallback strategies
- No health checks or monitoring endpoints
- No graceful shutdown handling
- No backup or disaster recovery

#### **Observability & Monitoring**
- No structured logging beyond basic console output
- No metrics collection (Prometheus, Application Insights, etc.)
- No distributed tracing
- No application performance monitoring (APM)
- No alerting or notification systems

#### **Configuration Management**
- No environment-specific configurations
- No external configuration sources (Azure Key Vault, etc.)
- No feature flags or configuration hot-reloading

#### **Deployment & DevOps**
- No containerization (Docker)
- No orchestration (Kubernetes)
- No CI/CD pipelines
- No infrastructure as code (Terraform, ARM templates)
- No environment provisioning automation

### 4. **Testing Strategy**

#### **No Automated Testing**
- **No Unit Tests**: No xUnit, NUnit, or MSTest test projects
- **No Integration Tests**: No automated API endpoint testing
- **No Performance Tests**: No load testing or stress testing
- **No End-to-End Tests**: No automated user journey testing

#### **Manual Testing Only**
- **Postman Collection**: Comprehensive collection for manual verification
- **Swagger UI**: Interactive API documentation for manual testing
- **Manual Verification**: Reliance on manual testing of business rules and edge cases

**Rationale**: Automated testing, while important for production applications, would significantly increase development time. The Postman collection provides sufficient verification capabilities for assignment evaluation.

### 5. **Simplified Business Logic**

#### **Concurrency Approach**
- **Best-Effort Constraints**: Business rules (unique titles, 3-task limit) enforced with "best effort" rather than guaranteed consistency
- **Acceptable Race Conditions**: Brief constraint violations acceptable for performance benefits

#### **Error Handling**
- **Basic Exception Handling**: Simple try-catch with standard HTTP response codes
- **No Error Tracking**: No centralized error logging or tracking systems
- **Limited Validation**: Basic input validation without comprehensive edge case handling