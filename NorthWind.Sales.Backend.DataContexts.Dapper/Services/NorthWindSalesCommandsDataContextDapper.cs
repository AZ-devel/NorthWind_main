using MySqlConnector;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

namespace NorthWind.Sales.Backend.DataContexts.Dapper.Services
{
    internal class NorthWindSalesCommandsDataContextDapper : INorthWindSalesCommandsDataContext
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public NorthWindSalesCommandsDataContextDapper(IOptions<DBOptions> dbOptions)
        {
            _connection = new MySqlConnection(dbOptions.Value.ConnectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public async Task AddOrderAsync(Order order)
        {
            var sql = @"INSERT INTO Orders 
            (CustomerId, OrderDate, ShipAddress, ShipCity, ShipCountry, ShipPostalCode, ShippingType, DiscountType, Discount)
            VALUES 
            (@CustomerId, @OrderDate, @ShipAddress, @ShipCity, @ShipCountry, @ShipPostalCode, @ShippingType, @DiscountType, @Discount);
            SELECT LAST_INSERT_ID();";

            var id = await _connection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    order.CustomerId,
                    order.OrderDate,
                    order.ShipAddress,
                    order.ShipCity,
                    order.ShipCountry,
                    order.ShipPostalCode,
                    order.ShippingType,
                    order.DiscountType,
                    order.Discount
                },
                _transaction
            );

            order.Id = id;
        }

        public async Task AddOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails)
        {
            var sql = @"INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
                        VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);";

            foreach (var detail in orderDetails)
            {
                await _connection.ExecuteAsync(
                    sql,
                    new
                    {
                        OrderId = detail.Order.Id,
                        detail.ProductId,
                        detail.Quantity,
                        detail.UnitPrice
                    },
                    _transaction
                );
            }
        }


        public async Task<Order> GetOrderAndDetailsAsync(int orderId)
        {
            var sqlOrder = @"SELECT 
                        Id, CustomerId, OrderDate, ShipAddress, ShipCity, ShipCountry, 
                        ShipPostalCode, ShippingType, DiscountType, Discount
                     FROM Orders 
                     WHERE Id = @OrderId;";

            var sqlDetails = @"SELECT 
                          OrderId, ProductId, Quantity, UnitPrice 
                       FROM OrderDetails 
                       WHERE OrderId = @OrderId;";

            // Obtener entidad de base de datos
            var orderEntity = await _connection.QueryFirstOrDefaultAsync<Order>(
                sqlOrder,
                new { OrderId = orderId },
                _transaction
            );

            if (orderEntity is null)
                return null;

            // Obtener detalles de base de datos
            var detailEntities = await _connection.QueryAsync<OrderDetail>(
                sqlDetails,
                new { OrderId = orderId },
                _transaction
            );

            // Mapear a objeto de dominio
            var order = new Order
            {
                Id = orderEntity.Id,
                CustomerId = orderEntity.CustomerId,
                OrderDate = orderEntity.OrderDate,
                ShipAddress = orderEntity.ShipAddress,
                ShipCity = orderEntity.ShipCity,
                ShipCountry = orderEntity.ShipCountry,
                ShipPostalCode = orderEntity.ShipPostalCode,
                ShippingType = orderEntity.ShippingType,
                DiscountType = orderEntity.DiscountType,
                Discount = orderEntity.Discount,
            };

            return order;
        }


        public async Task<IEnumerable<Order>> GetOrdersAsync(int cantidad)
        {
            var orderDictionary = new Dictionary<int, Order>(cantidad);

            var sql = "CALL GetRecentOrdersWithDetails(@Cantidad);";

            var result = await _connection.QueryAsync<Order, OrderDetail, Order>(
                sql,
                (order, detail) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                    {
                        currentOrder = order;
                        currentOrder.OrderDetails = detail != null
                            ? new List<OrderDetail> { detail }
                            : new List<OrderDetail>();
                        orderDictionary[order.Id] = currentOrder;
                    }
                    else if (detail != null)
                    {
                        currentOrder.OrderDetails.Add(detail);
                    }

                    return currentOrder;
                },
                new { Cantidad = cantidad },
                transaction: _transaction,
                splitOn: "OrderId"
            );

            return orderDictionary.Values;
        }

        public Task SaveChangesAsync()
        {
            _transaction.Commit();
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}
