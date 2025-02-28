# ERP Project

Modern Enterprise Resource Planning (ERP) system built with .NET 9.0, implementing Clean Architecture principles and advanced infrastructure features.

## Project Structure

The solution follows Clean Architecture principles with the following layers:

```
Erp/
├── Erp.Domain/           # Core business logic and entities
├── Erp.Application/      # Application services and business rules
├── Erp.Infrastructure/   # Infrastructure concerns and implementations
└── Erp.Api/             # API endpoints and configurations
```
## Infrastructure Features

### 1. Logging Infrastructure
- Structured logging using Serilog
- Multiple logging targets:
  - Console logging (development: Information, production: Warning)
  - JSON structured logs (daily rolling files)
  - Plain text logs for easy reading
  - Machine name and environment enrichment
  - Detailed exception logging
  - Retention policy: 30 days

### 2. Caching System
- In-memory caching implementation
- Distributed caching support
- Cache key management through `CacheKeys` enum
- Cache invalidation strategies

### 3. Database Infrastructure
- Entity Framework Core with PostgreSQL
- In-memory database support for testing
- Code-first migrations
- Repository pattern implementation
- Database seeding capabilities

### 4. Authentication & Authorization
- JWT token-based authentication
- Role-based authorization
- Token validation and refresh mechanisms
- Secure password hashing

### 5. API Infrastructure
- API versioning support
- Global exception handling
- Request/response logging
- Model validation
- Custom response models

### 6. Cross-Cutting Concerns
- Dependency injection setup
- Middleware configurations
- Interceptors for logging and caching
- Custom attributes for authorization

## AOT (Ahead-of-Time) Compilation

The project is configured to support AOT compilation features in .NET 9:

1. **Native AOT Support**:
   - Improved startup time
   - Reduced memory footprint
   - Smaller deployment size
   - No JIT compilation overhead

2. **Trimming Configuration**:
   - Dead code elimination
   - Assembly linking optimization
   - Reflection-based code preservation

3. **Performance Optimizations**:
   - Static compilation benefits
   - Reduced runtime overhead
   - Improved security through reduced attack surface

## Getting Started

1. Prerequisites:
   ```bash
   - .NET 9.0 SDK
   - PostgreSQL 15+
   - Visual Studio 2022+ or VS Code
   ```
## Development Guidelines

1. **Domain Layer**:
   - Keep domain models pure and free from infrastructure concerns
   - Use interfaces for abstractions
   - Implement rich domain models with business logic

2. **Application Layer**:
   - Implement application services
   - Handle business use cases
   - Manage transactions and coordination

3. **Infrastructure Layer**:
   - Implement interfaces defined in domain layer
   - Handle external concerns (database, logging, etc.)
   - Configure system-wide services

4. **API Layer**:
   - Define endpoints and routes
   - Handle HTTP concerns
   - Manage API versioning
