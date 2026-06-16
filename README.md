# ProductApi — RESTful Backend API

A production-grade RESTful API built with **.NET 8 / ASP.NET Core**, following **Clean Architecture** principles.

## Table of Contents
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [API Endpoints](#api-endpoints)
- [Authentication Flow](#authentication-flow)
- [Environment Setup](#environment-setup)
- [Running with Docker](#running-with-docker)
- [Running Locally](#running-locally)

---

## Architecture

The solution follows Clean Architecture with strict layer dependencies:

```
API → Application → Domain
Infrastructure → Application → Domain
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities (`Product`, `Item`, `User`), Enums, Custom Exceptions — zero dependencies |
| **Application** | Services, DTOs, Interfaces, Validators, AutoMapper profiles |
| **Infrastructure** | EF Core, MySQL Repositories, UoW, JWT Authentication, Serilog |
| **API** | Controllers, Middleware, Health Checks, Swagger UI, `Program.cs` |

---

## Tech Stack

| Concern | Technology |
|---------|------------|
| Framework | .NET 8 / ASP.NET Core Web API |
| Database | **MySQL** + EF Core 8 (Code-First) |
| Authentication | JWT Bearer Tokens |
| Authorization | Role-based (`Admin`, `User`) |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Logging | Serilog (Console + Rolling File) |
| Documentation | Swagger / OpenAPI (Swashbuckle) |
| Versioning | `Asp.Versioning.Http` |
| Containerization | Docker + Docker Compose |

---

## API Endpoints

### Authentication (`/api/v1/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/register` | Public | Register new user (Default role: `User`) |
| POST | `/api/v1/auth/login` | Public | Login → Returns Access Token & Refresh Token |
| POST | `/api/v1/auth/refresh` | Public | Rotate refresh token |
| POST | `/api/v1/auth/revoke` | Bearer | Revoke refresh token (logout) |

### Products (`/api/v1/products`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/products` | Bearer | Paginated product list |
| GET | `/api/v1/products/{id}` | Bearer | Get product by ID |
| GET | `/api/v1/products/{id}/items` | Bearer | Get product with all its items |
| POST | `/api/v1/products` | Bearer | Create a new product |
| PUT | `/api/v1/products/{id}` | Bearer | Update product details |
| DELETE | `/api/v1/products/{id}` | Bearer | Delete product |

### Items (`/api/v1/items`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/items/{id}` | Bearer | Get item by ID |
| POST | `/api/v1/items` | Bearer | Create a new item mapped to a product |
| PUT | `/api/v1/items/{id}` | Bearer | Update item quantity |
| DELETE | `/api/v1/items/{id}` | Bearer | Delete item |

---

## Authentication Flow

**Default seeded admin credentials:**
- Username: `admin`
- Password: `Admin@123`

1. **Login:** Call `POST /api/v1/auth/login`. Copy the `accessToken` from the response.
2. **Authorize Swagger:** Click the green **Authorize** button at the top of Swagger. **Paste your token directly** (Do not type "Bearer" in front of it, Swagger does this automatically).
3. **Call Protected Endpoints:** You can now freely test `/api/v1/products` and `/api/v1/items`.

*(Note: During development, token expiration is set to 24 hours to make testing easier.)*

---

## Environment Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL** Server (Running locally or via Docker)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Optional, for containerized setup)

### Configuration

Ensure your `appsettings.Development.json` points to your MySQL instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=Assignment;Uid=root;Pwd=YourPassword;"
  },
  "Jwt": {
    "Secret": "<at-least-32-character-secret>",
    "Issuer": "ProductApi",
    "Audience": "ProductApiClients",
    "AccessTokenMinutes": "1440"
  }
}
```

---

## Running Locally

```bash
# 1. Start your local MySQL server (Ensure a database named 'Assignment' exists or can be created)

# 2. Run the API (This will automatically create tables and seed dummy data):
cd src/API
dotnet run --environment Development

# 3. Open Swagger UI in your browser:
# http://localhost:5000/swagger
```

---

## Running with Docker

The API includes a `Dockerfile` with built-in health checks.

```bash
# Build the image
docker build -t productapi .

# Run the container (Ensure your database connection string in appsettings.Docker.json is configured for Docker networking)
docker run -p 8080:8080 productapi
```

Health Check Endpoint: `http://localhost:5000/health`
