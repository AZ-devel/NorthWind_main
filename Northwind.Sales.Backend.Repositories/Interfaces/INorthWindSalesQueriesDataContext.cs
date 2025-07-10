namespace NorthWind.Sales.Backend.Repositories.Interfaces
{
    public interface INorthWindSalesQueriesDataContext
    {
        IQueryable<Customer> Customers { get; }
        IQueryable<Product> Products { get; }
        IQueryable<Order> Orders { get; }
        Task<ReturnType> FirstOrDefaultAync<ReturnType>(
        IQueryable<ReturnType> queryable);
        Task<IEnumerable<ReturnType>> ToListAsync<ReturnType>(IQueryable<ReturnType> queryable);
    }
}
