using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletWise.Domain.Interfaces;
using WalletWise.Persistence.Context;
using WalletWise.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;


namespace WalletWise.Persistence.DependencyInjection
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
               ));

            services.AddDbContext<IdentityAppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("IdentityConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.FullName)));

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

            return services;
        }
    }
}
