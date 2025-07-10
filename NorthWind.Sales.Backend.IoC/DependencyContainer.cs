namespace NorthWind.Sales.Backend.IoC;
public static class DependencyContainer
{
    public static IServiceCollection AddNorthWindSalesServices(
this IServiceCollection services,
Action<DBOptions> configureDBOptions)
    {
        services.AddUseCasesServices()
        .AddRepositories()
        .AddDataContexts(configureDBOptions)
        .AddPresenters()
        .AddValidationService()
        .AddValidators();
        return services;
    }
}
