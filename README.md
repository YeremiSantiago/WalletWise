# WalletWise

WalletWise is a comprehensive personal finance management RESTful API built with **.NET 9**. It provides users with the tools to track their expenses, manage budgets, analyze their spending habits through detailed reports, and maintain control over multiple wallets. It's designed following **Clean Architecture** principles and implements robust security features including JWT authentication and rate limiting.

## 🚀 Features

- **Authentication & Authorization:** Secure user registration, login, token refresh, and password management using ASP.NET Core Identity and JWT.
- **Wallet Management:** Create and manage multiple wallets to segregate funds (e.g., Cash, Bank Account, Savings).
- **Category Management:** Define custom categories for transactions, classified as Income or Expense.
- **Transaction Tracking:** Record income and expenses, linking them to specific wallets and categories. Includes filtering and pagination.
- **Budget Control:** Set up budgets per wallet and category for specific periods. Supports a "Strict Mode" to prevent transactions that exceed the defined budget.
- **Financial Reports:** Generate comprehensive financial insights including global summaries, monthly trends, spending by category, top expenses, and period comparisons. Export data to JSON.
- **Security & Reliability:** Features global exception handling, model validation, CORS, and endpoint rate limiting to prevent abuse.

## 🛠️ Technologies & Stack

- **Framework:** .NET 9.0 (ASP.NET Core Web API)
- **Language:** C# 13
- **Database:** Microsoft SQL Server 2022
- **ORM:** Entity Framework Core 9.0
- **Security:** ASP.NET Core Identity, JWT Bearer Authentication
- **Documentation:** Swagger / OpenAPI
- **Containerization:** Docker & Docker Compose
- **Testing:** xUnit, Moq, Testcontainers (Integration Testing)
- **Other:** AutoMapper

## 🏗️ Architecture

The solution follows **Clean Architecture**, ensuring a clear separation of concerns:

- **`WalletWise.Domain`:** The core of the application. Contains domain entities (`Wallet`, `Transaction`, `Budget`, `Category`), enums, custom exceptions, and domain interfaces. Has no external dependencies.
- **`WalletWise.Application`:** Contains the business logic. Implements interfaces, services (`ITransactionService`, `IAuthService`, etc.), Data Transfer Objects (DTOs), and mapping profiles (AutoMapper).
- **`WalletWise.Infrastructure`:** Implementation of data access and external concerns. Contains EF Core `DbContext`s (`AppDbContext`, `IdentityAppDbContext`), repositories, entity configurations, migrations, and identity configurations.
- **`WalletWise.WebApi`:** The presentation layer. Exposes REST endpoints (Controllers), configures Dependency Injection, middleware (Rate Limiting, CORS, Global Exception Handling), and Swagger.
- **`WalletWise.UnitTest` & `WalletWise.Integration.Test`:** Automated test projects ensuring application reliability.

## ⚙️ Prerequisites & Configuration

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker & Docker Compose](https://www.docker.com/) (Optional, for database)
- SQL Server (if not using Docker)

### Environment Variables (.env)
If you plan to use Docker for the database, create a `.env` file in the root directory (where `docker-compose.yml` is located):
```env
DB_PASSWORD=YourStrongPassword123!
```

### Application Settings
Update the connection strings and JWT settings in `WalletWise.WebApi/appsettings.json` or `appsettings.Development.json`. By default, the API expects two connection strings:
- `WalletWiseConnection`: For application data.
- `WalletWiseIdentityConnection`: For user identity data.

*Note: Never commit real passwords or secret keys to source control.*

## 🏃 Local Execution

### 1. Run the Database (Docker)
Start the SQL Server instance using Docker Compose:
```bash
docker-compose up -d db
```
This will start a SQL Server 2022 container on port `1435`. 

### 2. Run the Application
Navigate to the Web API project and run it:
```bash
cd WalletWise.WebApi
dotnet run
```
*(By default, ASP.NET Core listens on http://localhost:5000 or similar).*

### Using Docker for the entire stack
You can run both the API and the Database together using:
```bash
docker-compose up -d
```
The API will be accessible on port `5050` (`http://localhost:5050`) based on the provided `docker-compose.yml`.

## 🗄️ Database Migrations

The project uses two EF Core contexts: `AppDbContext` (Core data) and `IdentityAppDbContext` (User data). 

**Automatic Migrations:**
When running the application with the environment variable `ASPNETCORE_ENVIRONMENT=Development`, the API automatically applies pending migrations for both contexts on startup.

**Manual Migrations:**
To apply migrations manually using the EF Core CLI, run the following commands from the solution root:
```bash
dotnet ef database update --context IdentityAppDbContext --project WalletWise.Infrastructure --startup-project WalletWise.WebApi
dotnet ef database update --context AppDbContext --project WalletWise.Infrastructure --startup-project WalletWise.WebApi
```

## 📡 API Documentation (Swagger)

When running in the Development environment, interactive API documentation is available via Swagger UI.

1. Start the API.
2. Navigate to: `http://localhost:<PORT>/swagger` (e.g., `http://localhost:5050/swagger` if running via docker-compose)
3. Click "Authorize" and provide a valid JWT token to test secured endpoints.

**Available Modules:**
- `Auth`: `/api/auth` (Register, Login, Refresh, Logout, Change Password)
- `Users`: `/api/users` (Profile management)
- `Wallets`: `/api/wallets` (CRUD for wallets)
- `Categories`: `/api/categories` (CRUD for categories)
- `Transactions`: `/api/transactions` (Record operations, get summary)
- `Budgets`: `/api/budgets` (Setup limits and monitor spending)
- `Reports`: `/api/reports` (Analytics, comparisons, JSON exports)

## 🧪 Testing

The solution includes Unit Tests and Integration Tests. Integration tests utilize **Testcontainers** to spin up an ephemeral SQL Server instance for reliable and isolated database testing.

To run all tests, execute the following command from the root directory:
```bash
dotnet test
```
