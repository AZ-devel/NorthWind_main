using Microsoft.Extensions.DependencyInjection;
using NorthWind.Sales.Frontend.WebApiGateways;
using NorthWind.Sales.Frontend.Views;

namespace NorthWind.Sales.Frontend.IoC
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddNorthWindSalesServices(
this IServiceCollection services,
Action<HttpClient> configureHttpClient)
        {
            services.AddWebApiGateways(configureHttpClient)
            .AddViewsServices()
            .AddValidationService()
            .AddValidators();
            return services;
        }
        }

}
