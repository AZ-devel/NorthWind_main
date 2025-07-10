namespace NorthWind.Sales.Backend.BusinessObjects.Guards
{
    public static class GuardModel
    {
        public static async Task AgainstNotValid<T>(
        IModelValidatorHub<T> modelValidatorHub, T model)
        {
            if (!await modelValidatorHub.Validate(model))
            {
                string Errors = string.Join(" ",
                modelValidatorHub.Errors
                .Select(e => $"{e.PropertyName}: {e.Message}"));
                throw new Exception(Errors);
            }
        }
    }
}
