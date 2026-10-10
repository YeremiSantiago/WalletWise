# WalletWise

## Descripción
WalletWise es una API RESTful de finanzas personales desarrollada en .NET 9. Proporciona la infraestructura backend necesaria para registrar gastos, administrar presupuestos estrictos, analizar hábitos financieros mediante reportes detallados y gestionar múltiples billeteras de forma simultánea.

## Funcionalidades principales
- **Autenticación segura:** Gestión de identidad y autenticación basada en JWT con rotación de tokens (refresh tokens), protegida mediante limitación de peticiones (rate limiting) por IP y usuario.
- **Gestión multicuentas y categorías:** Soporte para administrar fondos separados a través de múltiples billeteras y clasificar las transacciones como ingresos o gastos.
- **Control estricto de presupuestos:** Configuración de presupuestos por categoría y periodo. Su "Modo Estricto" intercepta y rechaza automáticamente aquellas transacciones que superen los límites definidos.
- **Analítica financiera:** Endpoints diseñados para generar resúmenes globales, tendencias mensuales, mayores gastos, comparativas entre periodos y exportación de datos en formato JSON.

## Tecnologías
- **Framework:** .NET 9.0 (ASP.NET Core Web API), C# 13
- **Base de Datos y ORM:** Microsoft SQL Server 2022, Entity Framework Core 9.0
- **Seguridad:** ASP.NET Core Identity, JWT Bearer
- **Pruebas:** xUnit, Moq, Testcontainers
- **Infraestructura:** Docker y Docker Compose

## Arquitectura
La solución sigue los principios de *Clean Architecture*, garantizando que el núcleo de la aplicación sea independiente de frameworks externos:
- **`WalletWise.Domain`:** Entidades de dominio, enumeraciones y excepciones personalizadas.
- **`WalletWise.Application`:** Lógica de negocio, servicios e interfaces (utilizando AutoMapper para DTOs).
- **`WalletWise.Infrastructure`:** Acceso a datos, contextos de EF Core (`AppDbContext`, `IdentityAppDbContext`), repositorios y configuración de Identity.
- **`WalletWise.WebApi`:** Capa de presentación (controladores), limitación de peticiones (rate limiting) y manejo global de excepciones.

## Instalación y ejecución

### Requisitos
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (para levantar el servidor de base de datos)

### Pasos de ejecución
1. Crea un archivo `.env` en el directorio raíz (donde se encuentra `docker-compose.yml`) y define la contraseña de SQL Server:
   ```env
   DB_PASSWORD=TuContrasenaSegura123!
   ```
2. Inicia el contenedor de la base de datos:
   ```bash
   docker-compose up -d db
   ```
3. Ejecuta la API desde el directorio del proyecto WebApi:
   ```bash
   cd WalletWise.WebApi
   dotnet run
   ```
   *Nota: Al ejecutarse bajo el entorno `Development`, la aplicación aplica automáticamente las migraciones pendientes de EF Core al iniciar.*

### Documentación de la API
Una vez que la aplicación esté en ejecución, puedes consultar la documentación interactiva en Swagger navegando a `http://localhost:5000/swagger` (o el puerto que asigne tu entorno). Utiliza el endpoint `/api/auth/login` para obtener un token JWT y probar las rutas protegidas.

## Pruebas
El proyecto incluye pruebas unitarias para aislar la lógica de negocio y pruebas de integración apoyadas en Testcontainers, lo cual permite validar contra una base de datos efímera real. Para ejecutar toda la suite de pruebas desde la raíz del repositorio:
```bash
dotnet test
```
