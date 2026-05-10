using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Interfaces;
using WalletWise.Domain.Setting;
using WalletWise.Infraestructure.Services;
using WalletWise.Infraestructure.Time;

namespace WalletWise.Infraestructure.DependencyInjection
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddInfraestructureLayerIoc(this IServiceCollection services, IConfiguration configuration)
        {
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
            });

            services.AddScoped<IAuthService, AuthService>();

            return services;

        }
    }
}
