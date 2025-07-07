using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HermesBanking.Core.Application
{
    public static class ServicesRegistration
    {
        //Extension method - Decorator pattern
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            #endregion

            #region Services IOC
            #endregion
        }

    }
}
