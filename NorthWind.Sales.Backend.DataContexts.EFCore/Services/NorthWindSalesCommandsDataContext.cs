
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Services;

internal class NorthWindSalesCommandsDataContext(IOptions<DBOptions> dbOptions) : NorthWindSalesContext(dbOptions), INorthWindSalesCommandsDataContext
{
    public async Task AddOrderAsync(Order order) => await AddAsync(order);

    public async Task AddOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails) =>
    await AddRangeAsync(orderDetails);

    public async Task<Order> GetOrderAndDetailsAsync(int orderId)=> await GetOrderAndDetailsAsync(orderId);

    public async Task<IEnumerable<Order>> GetOrdersAsync(int cantidad)=> await GetOrdersAsync(cantidad);

    public async Task SaveChangesAsync() => await base.SaveChangesAsync();

}
