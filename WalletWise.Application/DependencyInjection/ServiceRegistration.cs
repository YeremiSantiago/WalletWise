using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Services;

namespace WalletWise.Application.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationLayerIoc(this IServiceCollection services)
        {
            // Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWalletService, WalletService>();

            // Configurations

            services.AddAutoMapper(c => { c.AddMaps(typeof(ServiceRegistration).Assembly); });

            return services;
        }
    }
}
