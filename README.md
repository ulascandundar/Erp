# ERP Project

Modern Enterprise Resource Planning (ERP) system built with .NET 9.0, implementing Clean Architecture principles and advanced infrastructure features.

## Technologies & Features

- **Framework**: .NET 9.0 with AOT compilation support
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT-based authentication and role-based authorization
- **API**: RESTful API with versioning and comprehensive documentation
- **Caching**: In-memory and distributed caching support
- **Logging**: Structured logging with Serilog
- **Testing**: Unit and integration testing with xUnit
- **Localization**: Multi-language support with resource-based localization

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

## Project Structure

The solution follows Clean Architecture principles with the following layers:

```
Erp/
├── Erp.Domain/           # Core business logic and entities
├── Erp.Application/      # Application services and business rules
├── Erp.Infrastructure/   # Infrastructure concerns and implementations
└── Erp.Api/              # API endpoints and configurations
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

# API Documentation

## Table of Contents
1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Language Support](#language-support)
4. [API Endpoints](#api-endpoints)
   - [Auth API](#auth-api)
   - [User API](#user-api)
   - [Supplier API](#supplier-api)
   - [Product API](#product-api)
   - [Unit API](#unit-api)
   - [Category API](#category-api)
   - [Raw Material API](#raw-material-api)
   - [Product Formula API](#product-formula-api)
   - [Order API](#order-api)
5. [Error Handling](#error-handling)
6. [Pagination and Filtering](#pagination-and-filtering)

## Overview

The Erp API is a backend API for an Enterprise Resource Planning (ERP) system. The API follows RESTful principles and exchanges data in JSON format. All API endpoints are structured in the format `/api/v1/[controller]` and API versioning is done through headers.

All API responses are returned in the following format:

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": { ... }
}
```

In case of an error:

```json
{
  "isSuccess": false,
  "message": "Error message",
  "data": null
}
```

## Authentication

The API uses JWT (JSON Web Token) based authentication. The authentication process follows these steps:

1. Send user credentials to the `/api/v1/Auth/login` endpoint
2. Receive a JWT token upon successful login
3. Include this token in the `Authorization` header in the format `Bearer [token]` for all subsequent requests

### Example Login Request

```http
POST /api/v1/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Example Response

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2023-03-15T12:00:00Z"
  }
}
```

## Language Support

The API provides multi-language support. Clients can specify their preferred language in requests. Three methods are supported for language selection:

1. **HTTP Header Method (Recommended)**: Specify language with the `X-Culture-Info` header
   ```
   X-Culture-Info: tr-TR
   ```

2. **Cookie Method**: Store language preference with the `.AspNetCore.Culture` cookie
   ```
   c=tr-TR|uic=tr-TR
   ```

3. **Accept-Language Header**: Use the browser's language settings
   ```
   Accept-Language: tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7
   ```

Supported languages: `tr-TR` (Turkish), `en-US` (English)

## API Endpoints

### Auth API

Used for authentication and user management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| POST | `/api/v1/Auth/login` | User login | No authorization required |
| POST | `/api/v1/Auth/forgotPassword/send` | Send password reset code | No authorization required |
| PUT | `/api/v1/Auth/forgotPassword/resetPassword` | Reset password | No authorization required |
| PUT | `/api/v1/Auth/changePassword` | Change password | User |

#### Example: Password Reset Request

```http
POST /api/v1/Auth/forgotPassword/send
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Example: Change Password Request

```http
PUT /api/v1/Auth/changePassword
Authorization: Bearer [token]
Content-Type: application/json

{
  "currentPassword": "oldPassword123",
  "newPassword": "newPassword123",
  "confirmPassword": "newPassword123"
}
```

### User API

Used for user management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/User` | List all users | CompanyAdmin |
| GET | `/api/v1/User/{id}` | Get a specific user | CompanyAdmin |
| POST | `/api/v1/User` | Create a new user | CompanyAdmin |
| PUT | `/api/v1/User/{id}` | Update user information | CompanyAdmin |
| DELETE | `/api/v1/User/{id}` | Delete a user | CompanyAdmin |
| GET | `/api/v1/User/paged` | Paginated user list | CompanyAdmin |

#### Example: Create User Request

```http
POST /api/v1/User
Authorization: Bearer [token]
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "password123",
  "roles": ["CompanyAdmin"]
}
```

### Supplier API

Used for supplier management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/Supplier` | List suppliers (with pagination) | CompanyAdmin |
| GET | `/api/v1/Supplier/{id}` | Get a specific supplier | CompanyAdmin |
| POST | `/api/v1/Supplier` | Create a new supplier | CompanyAdmin |
| PUT | `/api/v1/Supplier/{id}` | Update supplier information | CompanyAdmin |
| DELETE | `/api/v1/Supplier/{id}` | Delete a supplier | CompanyAdmin |

#### Example: Create Supplier Request

```http
POST /api/v1/Supplier
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "ABC Suppliers",
  "contactPerson": "Jane Smith",
  "email": "contact@abcsuppliers.com",
  "phoneNumber": "1234567890",
  "address": "123 Main St, City",
  "taxNumber": "TX123456",
  "website": "https://abcsuppliers.com",
  "description": "Office supplies provider"
}
```

#### Example: List Suppliers Request (Pagination and Search)

```http
GET /api/v1/Supplier?pageNumber=1&pageSize=10&search=ABC&orderBy=Name
Authorization: Bearer [token]
```

### Product API

Used for product management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/Product` | List all products | CompanyAdmin |
| GET | `/api/v1/Product/{id}` | Get a specific product | CompanyAdmin |
| POST | `/api/v1/Product` | Create a new product | CompanyAdmin |
| PUT | `/api/v1/Product/{id}` | Update product information | CompanyAdmin |
| DELETE | `/api/v1/Product/{id}` | Delete a product | CompanyAdmin |
| GET | `/api/v1/Product/paged` | Paginated product list | CompanyAdmin |

#### Example: Create Product Request

```http
POST /api/v1/Product
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Chair",
  "sku": "OFC-001",
  "barcode": "1234567890123",
  "description": "Ergonomic office chair",
  "price": 199.99,
  "categoryIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"]
}
```

### Unit API

Used for unit management operations (weight, length, volume units, etc.).

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/Unit` | List units (with pagination) | CompanyAdmin |
| GET | `/api/v1/Unit/{id}` | Get a specific unit | CompanyAdmin |
| POST | `/api/v1/Unit` | Create a new unit | CompanyAdmin |
| PUT | `/api/v1/Unit/{id}` | Update unit information | CompanyAdmin |
| DELETE | `/api/v1/Unit/{id}` | Delete a unit | CompanyAdmin |
| GET | `/api/v1/Unit/relationUnits/{unitId}` | List related units | CompanyAdmin |
| GET | `/api/v1/Unit/convertUnit/{unitId}/{rawMaterialId}` | Calculate unit conversion rate | CompanyAdmin |
| GET | `/api/v1/Unit/findRateToRoot/{unitId}` | Calculate conversion rate to root unit | CompanyAdmin |

#### Example: Create Unit Request

```http
POST /api/v1/Unit
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Kilogram",
  "shortCode": "kg",
  "description": "Metric weight unit",
  "conversionRate": 1,
  "rootUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Example: Calculate Unit Conversion Rate

```http
GET /api/v1/Unit/convertUnit/3fa85f64-5717-4562-b3fc-2c963f66afa6/4fa85f64-5717-4562-b3fc-2c963f66afa7
Authorization: Bearer [token]
```

### Category API

Used for category management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/Category` | List categories (with pagination) | CompanyAdmin |
| GET | `/api/v1/Category/{id}` | Get a specific category | CompanyAdmin |
| POST | `/api/v1/Category` | Create a new category | CompanyAdmin |
| PUT | `/api/v1/Category/{id}` | Update category information | CompanyAdmin |
| DELETE | `/api/v1/Category/{id}` | Delete a category | CompanyAdmin |

#### Example: Create Category Request

```http
POST /api/v1/Category
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Furniture",
  "description": "Desks, chairs, and other office furniture"
}
```

### Raw Material API

Used for raw material management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/RawMaterial` | List raw materials (with pagination) | CompanyAdmin |
| GET | `/api/v1/RawMaterial/{id}` | Get a specific raw material | CompanyAdmin |
| POST | `/api/v1/RawMaterial` | Create a new raw material | CompanyAdmin |
| PUT | `/api/v1/RawMaterial/{id}` | Update raw material information | CompanyAdmin |
| DELETE | `/api/v1/RawMaterial/{id}` | Delete a raw material | CompanyAdmin |

#### Example: Create Raw Material Request

```http
POST /api/v1/RawMaterial
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Wood",
  "description": "Oak wood for furniture",
  "price": 50.00,
  "barcode": "RM-001",
  "stock": 100,
  "unitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "supplierIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa7"]
}
```

### Product Formula API

Used for product formula management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| GET | `/api/v1/ProductFormula` | List product formulas (with pagination) | CompanyAdmin |
| GET | `/api/v1/ProductFormula/{id}` | Get a specific product formula | CompanyAdmin |
| POST | `/api/v1/ProductFormula` | Create a new product formula | CompanyAdmin |
| PUT | `/api/v1/ProductFormula/{id}` | Update product formula information | CompanyAdmin |
| DELETE | `/api/v1/ProductFormula/{id}` | Delete a product formula | CompanyAdmin |

#### Example: Create Product Formula Request

```http
POST /api/v1/ProductFormula
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Chair Formula",
  "description": "Formula for standard office chair",
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "rawMaterialId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "quantity": 2.5,
      "unitId": "3fa85f64-5717-4562-b3fc-2c963f66afa8"
    }
  ]
}
```

### Order API

Used for order management operations.

#### Endpoints

| Method | Endpoint | Description | Authorization |
|-------|----------|----------|-------|
| POST | `/api/v1/Order/place` | Create a new order | CompanyAdmin |
| GET | `/api/v1/OrderReport/daily` | Get daily order report | CompanyAdmin |
| GET | `/api/v1/OrderReport/monthly` | Get monthly order report | CompanyAdmin |
| GET | `/api/v1/OrderReport/product/{productId}` | Get order report by product | CompanyAdmin |

#### Example: Create Order Request

```http
POST /api/v1/Order/place
Authorization: Bearer [token]
Content-Type: application/json

{
  "customerName": "Acme Corp",
  "customerEmail": "orders@acmecorp.com",
  "customerPhone": "1234567890",
  "customerAddress": "123 Business St, City",
  "orderItems": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 5,
      "unitPrice": 199.99
    }
  ],
  "payments": [
    {
      "amount": 999.95,
      "paymentMethod": "CreditCard",
      "paymentDetails": "XXXX-XXXX-XXXX-1234"
    }
  ]
}
```

## Error Handling

The API returns various error conditions in a standard format. Error responses always come with HTTP 400 (Bad Request) or 500 (Internal Server Error) status codes and are in the following format:

```json
{
  "isSuccess": false,
  "message": "Error message appears here",
  "data": null
}
```

Common error types:

- `NullValueException`: When a required value is null
- `UserAuthException`: Authentication error
- `SupplierNameAlreadyExistsException`: Supplier with the same name already exists
- `SupplierTaxNumberAlreadyExistsException`: Supplier with the same tax number already exists
- `SupplierHasRawMaterialsException`: A supplier with raw materials cannot be deleted
- `UnitNameAlreadyExistsException`: Unit with the same name already exists
- `UnitShortCodeAlreadyExistsException`: Unit with the same short code already exists
- `UnitHasProductRawMaterialException`: A unit with raw materials cannot be deleted
- `UnitTypeMismatchException`: Unit types do not match
- `ProductNameAlreadyExistsException`: Product with the same name already exists
- `ProductBarcodeAlreadyExistsException`: Product with the same barcode already exists

## Pagination and Filtering

The API provides pagination and filtering support for endpoints that return collections. Pagination and filtering parameters are sent via query string.

### Pagination Parameters

- `pageNumber`: Page number (starts from 1)
- `pageSize`: Number of items per page
- `orderBy`: Sorting field
- `isDesc`: `true` for descending sort, `false` for ascending sort
- `search`: Search term

### Example Pagination Request

```http
GET /api/v1/Supplier?pageNumber=1&pageSize=10&orderBy=Name&isDesc=false&search=ABC
Authorization: Bearer [token]
```

### Pagination Response

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": {
    "items": [ ... ],
    "totalCount": 100,
    "totalPages": 10,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```
