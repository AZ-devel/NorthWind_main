using NorthWind.Sales.Backend.BusinessObjects.Interfaces.Repositories;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace NorthWind.Sales.Backend.Repositories.Repositories
{
    internal class QueriesRepository(INorthWindSalesQueriesDataContext context) : IQueriesRepository
    {
        public async Task<decimal?> GetCustomerCurrentBalance(string customerId)
        {
            var Queryable = context.Customers
                .Where(c => c.Id == customerId)
                .Select(c => new { c.CurrentBalance });

            var Result = await context.FirstOrDefaultAync(Queryable);
            return Result?.CurrentBalance;
        }

        public async Task<Order> GetOrderAndDetailsAsync(int orderId)
        {
            var query = context.Orders
                .Where(o => o.Id == orderId)
                .Select(o=> new Order
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    OrderDate = o.OrderDate,
                    ShipAddress = o.ShipAddress,
                    ShipCity = o.ShipCity,
                    ShipPostalCode = o.ShipPostalCode,
                    ShipCountry = o.ShipCountry,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                    {
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        UnitPrice = od.UnitPrice,
                        Quantity = od.Quantity,
                    }).ToList()
                });

            var order = await context.FirstOrDefaultAync(query);
            return order!;
        }


        public async Task<IEnumerable<ProductUnitsInStock>> GetProductsUnitsInStock(IEnumerable<int> productIds)
        {
            var Queryable = context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new ProductUnitsInStock(p.Id, p.UnitsInStock));

            return await context.ToListAsync(Queryable);
        }

       public async Task<IEnumerable<Order>> GetOrdersAsync(int cantidad)
        {
            var query = context.Orders
                 .OrderByDescending(o => o.OrderDate)
                 .Take(cantidad)
                 .Select(o => new Order
                 {
                     Id = o.Id,
                     CustomerId = o.CustomerId,
                     OrderDate = o.OrderDate,
                     ShipAddress = o.ShipAddress,
                     ShipCity = o.ShipCity,
                     ShipPostalCode = o.ShipPostalCode,
                     ShipCountry = o.ShipCountry,
                     ShippingType = o.ShippingType,
                     DiscountType = o.DiscountType,
                     Discount = o.Discount,
                     OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                     {
                         OrderId = od.OrderId,
                         ProductId = od.ProductId,
                         UnitPrice = od.UnitPrice,
                         Quantity = od.Quantity,
                     }).ToList()
                 });

            return await context.ToListAsync(query); // ✅ CORRECTO
        }
    }
}