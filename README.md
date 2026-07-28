# 💼 WalletWise API

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=for-the-badge&logo=nuget&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI-2088FF?style=for-the-badge&logo=githubactions&logoColor=white)

> API RESTful para la **gestión de finanzas personales**, construida con Clean Architecture sobre .NET 9. Permite a los usuarios registrar transacciones de ingresos y gastos, organizarlas por categorías y billeteras, y obtener reportes financieros detallados con comparaciones entre períodos.

---

## ✨ Características principales

- 🔐 **Autenticación segura** con JWT (JSON Web Tokens) y soporte para Refresh Tokens con rotación automática
- 🔄 **Refresh Token con detección de reutilización**: si un token ya usado es enviado, todos los tokens del usuario son invalidados automáticamente (protección ante robo de tokens)
- 👛 **Gestión de billeteras (Wallets)**: CRUD completo para organizar el dinero por cuentas o fuentes
- 🏷️ **Gestión de categorías**: categorías de tipo `Ingreso` o `Gasto`, con validación de nombre único por usuario
- 💸 **Gestión de transacciones**: registro de ingresos y gastos con filtros, paginación y ordenamiento
- 📊 **Módulo de reportes financieros**:
  - Resumen global (total ingresos, gastos y balance)
  - Resumen mensual por año
  - Reporte por categoría en rango de fechas
  - Top N categorías con más gasto
  - Comparación financiera entre dos períodos
  - Exportación filtrada del historial de transacciones
- 🛡️ **Rate Limiting** con políticas diferenciadas por endpoint (login, registro, API general, reportes)
- ⚠️ **Manejo global de errores** con respuestas estandarizadas (`ApiErrorResponse`) y trazabilidad por `TraceId`
- ✅ **Tests unitarios e integración** con xUnit, Moq y base de datos en memoria (SQLite)
- 🚀 **Pipeline CI/CD** con GitHub Actions (build + unit tests + integration tests)
- 📝 **Documentación automática** con Swagger/OpenAPI y anotaciones descriptivas

---

## 🛠️ Tecnologías utilizadas

| Tecnología | Versión | Justificación |
|---|---|---|
| **.NET / C#** | 9.0 | Plataforma moderna con soporte LTS, mejoras de rendimiento y nullable reference types |
| **ASP.NET Core Web API** | 9.0 | Framework robusto y maduro para construir APIs RESTful |
| **Entity Framework Core** | 9.0 | ORM con soporte para migraciones, LINQ y Code-First |
| **SQL Server** | 2022 | Motor relacional empresarial con soporte completo en EF Core |
| **ASP.NET Core Identity** | 9.0 | Gestión de usuarios y contraseñas sin reinventar la rueda |
| **JWT Bearer** | 9.0 | Autenticación stateless, ideal para APIs consumidas por clientes externos |
| **AutoMapper** | - | Separación limpia entre entidades de dominio y DTOs de presentación |
| **Swashbuckle (Swagger)** | 9.0.6 | Documentación interactiva de la API directamente desde el código |
| **xUnit** | - | Framework de testing estándar en el ecosistema .NET |
| **Moq** | - | Mocking de dependencias en tests unitarios |
| **SQLite (InMemory)** | - | Base de datos en memoria para tests de integración sin dependencias externas |
| **GitHub Actions** | - | Automatización de CI/CD directamente desde el repositorio |

---

## 📁 Estructura del proyecto

El proyecto sigue los principios de **Clean Architecture**, organizando el código en capas con dependencias unidireccionales (de afuera hacia adentro):

```
WalletWise/
│
├── 📂 WalletWise.Domain/                  # Capa de Dominio (núcleo)
│   ├── Entities/                          # Entidades de negocio (Category, Transaction, Wallet)
│   ├── Common/                            # BaseEntity<T>, AuditEntity, Result<T>, enums, paginación
│   ├── Interfaces/                        # Contratos de repositorios (IGenericRepository, ICategoryRepository...)
│   └── Reports/                           # Modelos de dominio para reportes (ReportSummary, ComparisonReport...)
│
├── 📂 WalletWise.Application/             # Capa de Aplicación (casos de uso)
│   ├── Services/                          # Implementación de casos de uso (CategoryService, TransactionService...)
│   ├── Interfaces/                        # Contratos de servicios de aplicación (ICategoryService, IAuthService...)
│   ├── Dtos/                              # Objetos de transferencia de datos por entidad (Request/Response DTOs)
│   ├── Mappings/EntityToDto/              # Perfiles de AutoMapper (Category, Transaction, Wallet, Reports)
│   ├── Common/                            # BusinessErrorCodes (catálogo de errores de negocio)
│   ├── Exceptions/                        # Excepciones de dominio (NotFoundException, ForbiddenAccessException)
│   └── DependencyInjection/               # Registro de servicios de la capa de aplicación
│
├── 📂 WalletWise.Infrastructure/          # Capa de Infraestructura (implementaciones técnicas)
│   ├── Context/                           # AppDbContext, IdentityAppDbContext
│   ├── Repositories/                      # Implementaciones de repositorios (GenericRepository, TransactionRepository...)
│   ├── Services/                          # AuthService (JWT + Refresh Token), CurrentUserService
│   ├── EntityConfigurations/              # Configuraciones Fluent API de EF Core
│   ├── Migrations/                        # Migraciones de base de datos (AppDbContext e IdentityAppDbContext)
│   ├── Settings/                          # Modelos de configuración (JwtSettings, RefreshToken)
│   ├── Time/                              # IClock / ClockSystem (abstracción del reloj para testabilidad)
│   └── DependencyInjection/               # Registro de servicios de infraestructura
│
├── 📂 WalletWise.WebApi/                  # Capa de Presentación (API)
│   ├── Controllers/                       # Controladores REST (Auth, Categories, Transactions, Wallets, Reports, Users)
│   ├── Handlers/                          # GlobalExceptionHandler (manejo centralizado de errores)
│   ├── Common/                            # ResultExtensions, BusinessErrorHttpMap, ApiErrorResponse
│   ├── Program.cs                         # Configuración de la aplicación (DI, Middleware, Swagger, Rate Limiting, CORS)
│   └── appsettings.json                   # Configuración de cadenas de conexión y JWT
│
├── 📂 WalletWise.UnitTest/                # Tests unitarios con xUnit + Moq
│   └── Services/                          # Tests de CategoryService, TransactionService, WalletService, ReportService
│
├── 📂 WalletWise.Integration.Test/        # Tests de integración con base de datos en memoria
│   ├── Controllers/                       # Tests HTTP end-to-end de todos los controladores
│   └── Repositories/                      # Tests de repositorios contra SQLite InMemory
│
├── 📂 .github/workflows/                  # Pipeline de CI con GitHub Actions
│   └── CI-Dotnet.yml                      # Build → Unit Tests → Integration Tests
│
└── WalletWise.sln                         # Solución de Visual Studio
```

### Patrón arquitectónico

Se implementa **Clean Architecture** con separación estricta de responsabilidades:

- **Dominio**: sin dependencias externas. Define las reglas de negocio y contratos.
- **Aplicación**: orquesta los casos de uso usando los contratos del dominio.
- **Infraestructura**: implementa los contratos (EF Core, Identity, JWT).
- **WebApi**: expone la aplicación al exterior y transforma resultados en respuestas HTTP.

El patrón **Result\<T\>** se usa en toda la capa de aplicación para devolver éxito o fallo sin lanzar excepciones controlables, evitando el uso de excepciones como control de flujo.

---

## ⚙️ Requisitos previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| .NET SDK | 9.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| SQL Server | 2019 o superior | [microsoft.com/sql-server](https://www.microsoft.com/sql-server) |
| dotnet-ef (CLI) | 9.0+ | `dotnet tool install --global dotnet-ef` |
| Git | Cualquier versión reciente | [git-scm.com](https://git-scm.com) |

> **Opcional**: SQL Server Management Studio (SSMS) o Azure Data Studio para administrar la base de datos visualmente.

---

## 🚀 Instalación y configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU_USUARIO/WalletWise.git
cd WalletWise
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar las variables de entorno

Edita el archivo `WalletWise.WebApi/appsettings.json` con tus valores reales:

```json
{
  "ConnectionStrings": {
    "WalletWiseConnection": "Server=TU_SERVIDOR;Database=WalletWise;Trusted_Connection=True;TrustServerCertificate=True;",
    "WalletWiseIdentityConnection": "Server=TU_SERVIDOR;Database=WalletWise;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "WalletWiseIdentity",
    "Audience": "WalletWise API",
    "DurationInMinutes": 30
  }
}
```

> ⚠️ Nunca subas el `appsettings.json` con valores reales al repositorio. Usa User Secrets o variables de entorno en producción.

Para usar **User Secrets** en desarrollo:

```bash
cd WalletWise.WebApi
dotnet user-secrets set "JwtSettings:SecretKey" "tu_clave_secreta_aqui"
dotnet user-secrets set "ConnectionStrings:WalletWiseConnection" "tu_cadena_de_conexion"
dotnet user-secrets set "ConnectionStrings:WalletWiseIdentityConnection" "tu_cadena_de_conexion"
```

### 4. Aplicar las migraciones a la base de datos

```bash
# Actualizar la base de datos principal (Categories, Wallets, Transactions)
dotnet ef database update --project WalletWise.Infrastructure/WalletWise.Infrastructure.csproj --startup-project WalletWise.WebApi/WalletWise.WebApi.csproj --context AppDbContext

# Actualizar la base de datos de Identity (Users, Roles, RefreshTokens)
dotnet ef database update --project WalletWise.Infrastructure/WalletWise.Infrastructure.csproj --startup-project WalletWise.WebApi/WalletWise.WebApi.csproj --context IdentityAppDbContext
```

---

## ▶️ Cómo ejecutar el proyecto

### Entorno de desarrollo

```bash
cd WalletWise.WebApi
dotnet run
```

La API estará disponible en `https://localhost:7XXX` (el puerto exacto se muestra en consola).

La documentación Swagger estará disponible en:

```
https://localhost:7XXX/swagger
```

### Ejecutar los tests

```bash
# Tests unitarios
dotnet test WalletWise.UnitTest/WalletWise.Unit.Tests.csproj --configuration Release

# Tests de integración
dotnet test WalletWise.Integration.Test/WalletWise.Integration.Test.csproj --configuration Release

# Todos los tests
dotnet test --configuration Release
```

### Build de producción

```bash
dotnet build --configuration Release
dotnet publish WalletWise.WebApi/WalletWise.WebApi.csproj -c Release -o ./publish
```

---

## 📡 Endpoints disponibles

| Módulo | Método | Ruta | Descripción |
|---|---|---|---|
| 🔐 Auth | POST | `/api/auth/register` | Registro de nuevo usuario |
| 🔐 Auth | POST | `/api/auth/login` | Login y obtención de JWT |
| 🔐 Auth | POST | `/api/auth/refresh-token` | Renovar token de acceso |
| 🔐 Auth | POST | `/api/auth/logout` | Cerrar sesión e invalidar tokens |
| 🔐 Auth | PUT | `/api/auth/me/password` | Cambiar contraseña |
| 👤 Users | GET | `/api/users/me` | Obtener perfil del usuario autenticado |
| 👤 Users | PUT | `/api/users/me` | Actualizar nombre de perfil |
| 👛 Wallets | GET | `/api/wallets` | Listar todas las billeteras |
| 👛 Wallets | POST | `/api/wallets` | Crear billetera |
| 👛 Wallets | GET | `/api/wallets/{id}` | Obtener billetera por ID |
| 👛 Wallets | PUT | `/api/wallets/{id}` | Actualizar billetera |
| 👛 Wallets | DELETE | `/api/wallets/{id}` | Eliminar billetera |
| 🏷️ Categories | GET | `/api/categories` | Listar categorías activas |
| 🏷️ Categories | POST | `/api/categories` | Crear categoría |
| 🏷️ Categories | GET | `/api/categories/{id}` | Obtener categoría por ID |
| 🏷️ Categories | PUT | `/api/categories/{id}` | Actualizar categoría |
| 🏷️ Categories | DELETE | `/api/categories/{id}` | Eliminar categoría (soft delete) |
| 💸 Transactions | GET | `/api/transactions` | Listar transacciones (paginado + filtros) |
| 💸 Transactions | POST | `/api/transactions` | Crear transacción |
| 💸 Transactions | GET | `/api/transactions/summary` | Resumen de transacciones filtradas |
| 💸 Transactions | GET | `/api/transactions/{id}` | Obtener transacción por ID |
| 💸 Transactions | PUT | `/api/transactions/{id}` | Actualizar transacción |
| 💸 Transactions | DELETE | `/api/transactions/{id}` | Eliminar transacción |
| 📊 Reports | GET | `/api/reports/summary` | Resumen global (ingresos, gastos, balance) |
| 📊 Reports | GET | `/api/reports/monthly` | Resumen mensual por año |
| 📊 Reports | GET | `/api/reports/by-category` | Reporte por categoría en rango de fechas |
| 📊 Reports | GET | `/api/reports/top-categories` | Top N categorías con mayor gasto |
| 📊 Reports | GET | `/api/reports/comparison` | Comparación financiera entre dos períodos |
| 📊 Reports | GET | `/api/reports/export` | Exportar historial filtrado en JSON |

> Todos los endpoints excepto `register`, `login` y `refresh-token` requieren el header `Authorization: Bearer {token}`.

---

## 📸 Capturas de pantalla o demos

> 📌 **Sección pendiente de completar**: Agrega aquí capturas del Swagger UI, ejemplos de respuestas JSON, o un GIF del flujo de uso.

```
docs/
├── swagger-overview.png     ← Vista general del Swagger UI
├── auth-flow.gif            ← Flujo de registro y login
└── reports-demo.png         ← Ejemplo de reporte de comparación
```

---

## 🧠 Decisiones técnicas y aprendizajes

### ✅ Patrón Result\<T\> en lugar de excepciones para errores de negocio

Se implementó un `Result<T>` genérico que encapsula éxito o fallo con un código de error y mensaje. Esto evita usar excepciones como control de flujo para errores esperados (ej. "categoría ya existe"), manteniendo el código predecible y fácil de testear.

### ✅ Separación de DbContexts (AppDbContext + IdentityAppDbContext)

Se separaron las tablas de negocio (Categories, Wallets, Transactions) del contexto de Identity (Users, Roles, Tokens) para mantener responsabilidades claras y facilitar la migración independiente de cada esquema.

### ✅ Detección de reutilización de Refresh Tokens

Si un Refresh Token que ya fue usado vuelve a ser enviado, el sistema interpreta que hubo un robo del token y elimina **todos** los Refresh Tokens del usuario, forzándolo a autenticarse nuevamente. Este mecanismo sigue las recomendaciones de seguridad de OAuth 2.0.

### ✅ Rate Limiting diferenciado por contexto

Se configuraron políticas de Rate Limiting específicas para cada tipo de endpoint:
- **Login**: ventana fija de 5 intentos/minuto por IP
- **Registro**: 3 intentos/hora por IP
- **API general**: ventana deslizante de 60 solicitudes/minuto por usuario autenticado
- **Reportes**: limitador de concurrencia (máx. 2 solicitudes simultáneas) para proteger consultas pesadas

### ✅ Manejo global de excepciones con IExceptionHandler

Se implementó un `GlobalExceptionHandler` centralizado que mapea excepciones de dominio (`NotFoundException`, `ForbiddenAccessException`) a respuestas HTTP estándar con `ApiErrorResponse`, asegurando consistencia en toda la API y trazabilidad por `TraceId`.

### ✅ IClock para abstracción del tiempo

Se inyectó una interfaz `IClock` en lugar de usar `DateTime.UtcNow` directamente. Esto hace que los tests unitarios sean deterministas, ya que el reloj puede ser mockeado con cualquier fecha.

### ✅ Pipeline CI con GitHub Actions

Se configuró un pipeline de CI que ejecuta automáticamente en cada push a `dev` y en Pull Requests a `main`/`dev`. Los tests de integración usan SQLite en memoria y el JWT secret se inyecta desde los secretos de GitHub, evitando exponer credenciales en el código.

### 🔧 Reto: dos DbContexts apuntando a la misma base de datos

Durante el desarrollo se detectó que ambos contextos podían entrar en conflicto al intentar crear tablas que ya existían. La solución fue especificar explícitamente el contexto al ejecutar las migraciones con el flag `--context` y gestionar el historial de migraciones de cada contexto de forma independiente.

---

## 🔮 Mejoras futuras

- [ ] **Separar las bases de datos** de Identity y de negocio en instancias distintas para mayor aislamiento
- [ ] **Exportación en múltiples formatos**: CSV y PDF además del actual JSON
- [ ] **Notificaciones**: alertas cuando el gasto supera un límite configurado por el usuario
- [ ] **Presupuestos por categoría**: definir un límite de gasto mensual por categoría y controlar desviaciones
- [ ] **Soporte multimoneda**: registrar transacciones en diferentes divisas con conversión automática
- [ ] **Autenticación con proveedores externos** (Google, Microsoft) usando OAuth 2.0
- [ ] **Caché de reportes**: implementar caché en consultas pesadas con `IMemoryCache` o Redis
- [ ] **Auditoría automática**: completar el llenado de `CreatedBy` / `UpdatedBy` en `AuditEntity` usando `ICurrentUserService`
- [ ] **Versionado de la API** (v1, v2) para gestionar cambios sin romper clientes existentes
- [ ] **Dockerización**: agregar `Dockerfile` y `docker-compose.yml` para facilitar el despliegue

---

## 👤 Autor

**Jeremy Santiago**

[![GitHub](https://img.shields.io/badge/GitHub-YeremiSantiago-181717?style=for-the-badge&logo=github)](https://github.com/YeremiSantiago)

> 💡 Agrega aquí tus enlaces a LinkedIn y portafolio personal.

---

## 📄 Licencia

Este proyecto está distribuido bajo la licencia **MIT**.

```
MIT License - Copyright (c) 2026 Jeremy Santiago
```
