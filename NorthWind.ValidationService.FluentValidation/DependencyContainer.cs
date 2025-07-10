namespace NorthWind.ValidationService.FluentValidation
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddValidationService(
     this IServiceCollection services)
        {
            services.AddScoped(typeof(IValidationService<>),
            typeof(FluentValidationService<>));
            return services;
        }
    }
}
