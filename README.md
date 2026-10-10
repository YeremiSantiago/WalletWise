# WalletWise

WalletWise es una API RESTful desarrollada en .NET 9 que centraliza la gestión de finanzas personales. Su objetivo es brindar una infraestructura backend robusta para que los usuarios puedan registrar sus gastos, definir presupuestos estrictos por categoría, controlar los saldos de múltiples billeteras simultáneas y obtener análisis detallados de sus hábitos de consumo.

## Funcionalidades principales

* **Gestión de identidad y autenticación:** Registro e inicio de sesión seguros mediante tokens JWT con capacidad de rotación (refresh tokens), protegidos por límites de peticiones por IP.
* **Control de múltiples billeteras y categorías:** Permite separar los fondos en distintas cuentas y clasificar las transacciones como ingresos o gastos.
* **Manejo de presupuestos con modo estricto:** Facilita la creación de límites de gasto temporales por categoría, interceptando y rechazando transacciones que excedan el presupuesto definido si el modo estricto está activo.
* **Generación de reportes analíticos:** Provee consultas agregadas para obtener resúmenes mensuales, el top de mayores gastos, comparativas entre periodos y exportación del historial a formato JSON.
* **Protección de endpoints:** Implementa políticas de *rate limiting* y un manejo de excepciones global y unificado que retorna respuestas estandarizadas.

## Tecnologías utilizadas

* **Backend:** .NET 9.0 (ASP.NET Core Web API), C# 13.
* **Persistencia:** Entity Framework Core 9.0, Microsoft SQL Server 2022.
* **Seguridad:** ASP.NET Core Identity, Autenticación JWT Bearer.
* **Pruebas:** xUnit, Moq, Testcontainers.
* **Infraestructura:** Docker, Docker Compose.

## Arquitectura

La solución está construida siguiendo los principios de la Arquitectura Limpia (*Clean Architecture*), organizada en los siguientes proyectos:

* **`WalletWise.Domain`:** Define el núcleo del negocio. Contiene las entidades, enumeraciones, constantes de error y excepciones personalizadas, sin depender de ningún otro proyecto o framework externo.
* **`WalletWise.Application`:** Orquesta los casos de uso. Contiene los servicios, interfaces, DTOs y perfiles de mapeo mediante AutoMapper.
* **`WalletWise.Infrastructure`:** Resuelve las preocupaciones técnicas externas. Contiene el acceso a datos mediante dos contextos separados (`AppDbContext` e `IdentityAppDbContext`), los repositorios, las migraciones de EF Core y la configuración de Identity.
* **`WalletWise.WebApi`:** Actúa como la capa de presentación. Expone los controladores REST, configura la inyección de dependencias y los middlewares (como CORS y el manejo de excepciones global).

## Instalación y ejecución

### Requisitos previos
* SDK de .NET 9.0.
* Docker y Docker Compose (para ejecutar la base de datos localmente).

### Configuración e inicio

1. Crea un archivo `.env` en la raíz del repositorio y define la contraseña de SQL Server:
   ```env
   DB_PASSWORD=TuPasswordSeguro123!
   ```
2. Levanta el servidor de base de datos utilizando Docker Compose:
   ```bash
   docker-compose up -d db
   ```
3. Inicia la aplicación desde la terminal, dentro del proyecto principal:
   ```bash
   cd WalletWise.WebApi
   dotnet run
   ```

### Migraciones de la base de datos
Al ejecutar el proyecto con la variable de entorno `ASPNETCORE_ENVIRONMENT` configurada como `Development` (comportamiento por defecto con `dotnet run`), la aplicación aplicará automáticamente las migraciones pendientes en ambos contextos (`AppDbContext` e `IdentityAppDbContext`) durante el arranque.

Si necesitas aplicar las migraciones manualmente mediante la herramienta de línea de comandos, ejecuta:
```bash
dotnet ef database update --context IdentityAppDbContext --project WalletWise.Infrastructure --startup-project WalletWise.WebApi
dotnet ef database update --context AppDbContext --project WalletWise.Infrastructure --startup-project WalletWise.WebApi
```

## Pruebas

La solución contiene proyectos de pruebas unitarias y de integración. Las pruebas de integración se apoyan en la biblioteca Testcontainers, la cual instancia automáticamente un servidor SQL efímero en Docker para garantizar que las pruebas contra la base de datos estén aisladas y sean fiables.

Para ejecutar toda la suite de pruebas, sitúate en la raíz del repositorio y ejecuta:
```bash
dotnet test
```

## Documentación de la API

La API cuenta con documentación interactiva provista por Swagger. Una vez que la aplicación esté corriendo, ingresa en tu navegador a:
`http://localhost:5000/swagger`

*Nota: Utiliza el endpoint `/api/auth/login` dentro de la interfaz para obtener un token válido y poder autorizar las peticiones hacia el resto de rutas protegidas.*
