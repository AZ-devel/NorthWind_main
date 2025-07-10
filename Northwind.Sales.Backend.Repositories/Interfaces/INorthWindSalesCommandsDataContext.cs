
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

namespace NorthWind.Sales.Backend.Repositories.Interfaces;

public interface INorthWindSalesCommandsDataContext
{
    Task AddOrderAsync(Order order);
    Task AddOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails);
    Task<Order> GetOrderAndDetailsAsync(int orderId);   
    Task<IEnumerable<Order>> GetOrdersAsync(int cantidad);
    Task SaveChangesAsync();
}
