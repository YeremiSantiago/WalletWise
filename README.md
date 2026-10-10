# WalletWise

## What is WalletWise?
WalletWise is a personal finance management RESTful API built with .NET 9. It provides the backend infrastructure to track expenses, manage strict budgets, analyze financial habits through detailed reports, and control multiple wallets simultaneously.

## Key Features
- **Secure Authentication:** JWT-based authentication with refresh tokens and identity management, protected by IP and user rate limiting.
- **Multi-Wallet & Category Management:** Support for segregating funds across multiple wallets and categorizing transactions as income or expense.
- **Strict Budget Control:** Configurable budgets per category and period. A strict mode intercepts and rejects transactions that exceed defined limits.
- **Financial Analytics:** Endpoints to generate global summaries, monthly trends, top expenses, period comparisons, and JSON data exports.

## Tech Stack
- **Framework:** .NET 9.0 (ASP.NET Core Web API), C# 13
- **Database & ORM:** Microsoft SQL Server 2022, Entity Framework Core 9.0
- **Security:** ASP.NET Core Identity, JWT Bearer
- **Testing:** xUnit, Moq, Testcontainers
- **Infrastructure:** Docker & Docker Compose

## Architecture
The solution follows Clean Architecture principles, ensuring the core domain is independent of external frameworks:
- **`WalletWise.Domain`:** Entities, enums, and exceptions.
- **`WalletWise.Application`:** Business logic, services, and DTOs (using AutoMapper).
- **`WalletWise.Infrastructure`:** Data access, EF Core contexts (`AppDbContext`, `IdentityAppDbContext`), repositories, and Identity configurations.
- **`WalletWise.WebApi`:** Presentation layer containing controllers, rate limiting, and global exception handling.

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (for the database)

### Setup & Execution
1. Create a `.env` file in the root directory to define the SQL Server password:
   ```env
   DB_PASSWORD=YourStrongPassword123!
   ```
2. Start the database container:
   ```bash
   docker-compose up -d db
   ```
3. Run the API from the WebApi project directory:
   ```bash
   cd WalletWise.WebApi
   dotnet run
   ```
   *Note: Running in the `Development` environment automatically applies EF Core migrations on startup.*

### API Documentation
Interactive documentation is available via Swagger. Once the API is running, navigate to `http://localhost:5000/swagger` (or the port assigned by your environment). Use the `/api/auth/login` endpoint to obtain a JWT to authorize against secured routes.

## Testing
The project includes unit tests for business logic and integration tests utilizing Testcontainers for ephemeral database validation. Run all tests from the root directory:
```bash
dotnet test
```
