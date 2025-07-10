
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.CreateOrder;

namespace NorthWind.Sales.Backend.DataContexts.EFCore;
public static class DependencyContainer
{
    public static IServiceCollection AddDataContexts(this IServiceCollection services, Action<DBOptions> configureDBOptions)
    {
        services.Configure(configureDBOptions);
        services.AddScoped<INorthWindSalesCommandsDataContext, NorthWindSalesCommandsDataContext>();

        services.AddScoped<INorthWindSalesQueriesDataContext,NorthWindSalesQueriesDataContext>();

        return services;
    }
}

