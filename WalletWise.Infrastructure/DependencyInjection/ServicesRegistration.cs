using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Settings;
using WalletWise.Infrastructure.Services;
using WalletWise.Infrastructure.Time;
using WalletWise.Infrastructure.Context;
using WalletWise.Infrastructure.Repositories;

namespace WalletWise.Infrastructure.DependencyInjection
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddInfrastructureLayerIoc(this IServiceCollection services, IConfiguration configuration)
        {
            // PERSISTENCE 
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            ));

            services.AddDbContext<IdentityAppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("IdentityConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.FullName)
            ));

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityAppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            // INFRASTRUCTURE & AUTH 
            services.AddSingleton<IClock, ClockSystem>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // HttpContextAccesor
            services.AddHttpContextAccessor();

            var jwtSettings = configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>()!;

            services.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var tokenStamp = context.Principal?.FindFirst("security_stamp")?.Value;

                        if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tokenStamp))
                        {
                            context.Fail("Token Invalido");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);

                        if(user is null)
                        {
                            context.Fail("Usuario no encontrado");
                            return;
                        }

                        var currentStamp = await userManager.GetSecurityStampAsync(user);
                            
                        if(!string.Equals(currentStamp, tokenStamp, StringComparison.Ordinal))
                        {
                            context.Fail("Token revocado");
                        }
                    }
                };
            });

            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
