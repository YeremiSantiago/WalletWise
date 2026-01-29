using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Interfaces;
using WalletWise.Infraestructure.Time;

namespace WalletWise.Infraestructure.DependencyInjection
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddInfraestructureLayerIoc(this IServiceCollection services)
        {
            services.AddSingleton<IClock, ClockSystem>();

            return services;

        }
    }
}
