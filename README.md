# WalletWise

API RESTful para la gestión de finanzas personales, desarrollada bajo los principios de *Clean Architecture*. Permite administrar cuentas, transacciones y presupuestos (con bloqueo estricto por sobregiro).

## Stack Tecnológico
- **Backend:** .NET 9 (C# 13, ASP.NET Core Web API)
- **Base de Datos:** SQL Server 2022, Entity Framework Core 9
- **Seguridad:** ASP.NET Core Identity, JWT, Rate Limiting
- **Pruebas:** xUnit, Moq, Testcontainers
- **Infraestructura:** Docker y Docker Compose

## Ejecución Local

### 1. Configuración de Base de Datos
Crea un archivo `.env` en la raíz del repositorio para definir la contraseña del servidor de SQL:
```env
DB_PASSWORD=TuPasswordSeguro123!
```

### 2. Levantar la Infraestructura
```bash
docker-compose up -d db
```

### 3. Ejecutar la API
```bash
cd WalletWise.WebApi
dotnet run
```
*(Al iniciar, la API aplicará automáticamente las migraciones necesarias a la base de datos).*

## Documentación y Pruebas

- **Swagger (API Docs):** Disponible en `http://localhost:5000/swagger` una vez levantada la API.
- **Testing:** Para correr las pruebas unitarias y de integración (vía Testcontainers):
  ```bash
  dotnet test
  ```
