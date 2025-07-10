

namespace NorthWind.Sales.Backend.DataContexts.Dapper
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddDataContexts(this IServiceCollection services, Action<DBOptions> configureDBOptions)
        {
            services.Configure(configureDBOptions);
            services.AddScoped<INorthWindSalesCommandsDataContext, NorthWindSalesCommandsDataContextDapper>();

            return services;
        }
    }
}
