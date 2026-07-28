using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Threading.RateLimiting;
using WalletWise.Application.DependencyInjection;
using WalletWise.Infrastructure.DependencyInjection;
using WalletWise.Infrastructure.DependencyInjection;
using WalletWise.WebApi.Handlers;
using WalletWise.Application.Common;
using WalletWise.WebApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace WalletWise.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationLayerIoc().
                AddInfrastructureLayerIoc(builder.Configuration);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => { options.InvalidModelStateResponseFactory = context => { var apiError = new ApiErrorResponse { Status = StatusCodes.Status400BadRequest, Error = BusinessErrorCodes.ERR_VALIDATION, Message = "Errores de validación encontrados.", TraceId = context.HttpContext.TraceIdentifier, Timestamp = DateTime.UtcNow }; return new BadRequestObjectResult(apiError); }; });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddOpenApi();

            #region Rate Limiting 
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


                // Login Por Ip
                options.AddPolicy("AuthLoginByIp", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                // Registro Por Ip
                options.AddPolicy("AuthRegisterByIp", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter($"register:{ip}", _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                // Operaciones por usuario authenticado 
                options.AddPolicy("AuthenticatedUserApi", context =>
                {
                    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    var partitionKey = userId is not null
                    ? $"user:{userId}"
                    : $"anon:{context.Connection.RemoteIpAddress}";

                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                // Para operaciones concurrentes en los endpoints de reportes
                options.AddPolicy("ReportsEndpoint", context =>
                {
                    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";

                    return RateLimitPartition.GetConcurrencyLimiter($"reports:{userId}", _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 2,
                        QueueLimit = 1,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
                });

                // Respuesta personalizada con Headers Informativos
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";
                    context.HttpContext.Response.ContentType = "application/json";

                    var response = new
                    {
                        type = "https://tools.ietf.org/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = 429,
                        detail = "Has superado el l�mite de solicitudes. Intenta nuevamente m�s tarde."

                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
                };
            });
            #endregion

            #region CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Allow everything", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            #endregion

            #region Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WalletWise API",
                    Version = "v1",
                    Description = "API para gestion de finanzas personales. Permite administrar wallets, transacciones y categorias."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseCors("Allow everything");

            if (!app.Environment.IsEnvironment("Testing")) { app.UseRateLimiter(); }

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}



