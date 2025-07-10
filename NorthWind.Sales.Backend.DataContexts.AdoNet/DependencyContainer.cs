
namespace NorthWind.Sales.Backend.DataContexts.AdoNet
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddDataContexts(this IServiceCollection services, Action<DBOptions> configureDBOptions)
        {
            services.Configure(configureDBOptions);
            services.AddScoped<INorthWindSalesCommandsDataContext, NorthWindSalesCommandsDataContextAdoNet>();

            return services;
        }
    }
}