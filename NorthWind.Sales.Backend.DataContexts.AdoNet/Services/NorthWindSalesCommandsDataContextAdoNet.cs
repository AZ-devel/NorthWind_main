using Microsoft.Extensions.Options;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;
using NorthWind.Sales.Backend.DataContexts.AdoNet.Options;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

namespace NorthWind.Sales.Backend.DataContexts.AdoNet.Services
{
    internal class NorthWindSalesCommandsDataContextAdoNet : INorthWindSalesCommandsDataContext
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public NorthWindSalesCommandsDataContextAdoNet(IOptions<DBOptions> dbOptions)
        {
            _connection = new SqlConnection(dbOptions.Value.ConnectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public async Task AddOrderAsync(Order order)
        {
            var sql = @"INSERT INTO Orders 
                        (CustomerId, OrderDate, ShipAddress, ShipCity, ShipCountry, ShipPostalCode, ShippingType, DiscountType, Discount)
                        VALUES 
                        (@CustomerId, @OrderDate, @ShipAddress, @ShipCity, @ShipCountry, @ShipPostalCode, @ShippingType, @DiscountType, @Discount);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, _connection, _transaction);
            command.Parameters.AddWithValue("@CustomerId", order.CustomerId);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@ShipAddress", order.ShipAddress);
            command.Parameters.AddWithValue("@ShipCity", order.ShipCity);
            command.Parameters.AddWithValue("@ShipCountry", order.ShipCountry);
            command.Parameters.AddWithValue("@ShipPostalCode", order.ShipPostalCode);
            command.Parameters.AddWithValue("@ShippingType", order.ShippingType);
            command.Parameters.AddWithValue("@DiscountType", order.DiscountType);
            command.Parameters.AddWithValue("@Discount", order.Discount);

            var result = await command.ExecuteScalarAsync();
            order.Id = Convert.ToInt32(result);
        }

        public async Task AddOrderDetailsAsync(IEnumerable<OrderDetail> orderDetails)
        {
            var sql = @"INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
                        VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);";

            foreach (var detail in orderDetails)
            {
                using var command = new SqlCommand(sql, _connection, _transaction);
                command.Parameters.AddWithValue("@OrderId", detail.Order.Id);
                command.Parameters.AddWithValue("@ProductId", detail.ProductId);
                command.Parameters.AddWithValue("@Quantity", detail.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<BusinessObjects.POCOEntities.Order> GetOrderAndDetailsAsync(int orderId)
        {
            BusinessObjects.POCOEntities.Order order = null;
            var orderDetails = new List<BusinessObjects.ValueObjects.OrderDetail>();

            // 1. Obtener la orden
            var sqlOrder = @"SELECT Id, CustomerId, OrderDate, ShipAddress, ShipCity, ShipCountry, ShipPostalCode,
                            ShippingType, DiscountType, Discount
                     FROM Orders
                     WHERE Id = @OrderId;";

            using (var cmd = new SqlCommand(sqlOrder, _connection, _transaction))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    order = new BusinessObjects.POCOEntities.Order
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        CustomerId = reader.GetString(reader.GetOrdinal("CustomerId")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        ShipAddress = reader.GetString(reader.GetOrdinal("ShipAddress")),
                        ShipCity = reader.GetString(reader.GetOrdinal("ShipCity")),
                        ShipCountry = reader.GetString(reader.GetOrdinal("ShipCountry")),
                        ShipPostalCode = reader.GetString(reader.GetOrdinal("ShipPostalCode")),
                    };
                }
                else
                {
                    return null;
                }
            }

            // 2. Obtener los detalles
            var sqlDetails = @"SELECT ProductId, Quantity, UnitPrice
                       FROM OrderDetails
                       WHERE OrderId = @OrderId;";

            using (var cmd = new SqlCommand(sqlDetails, _connection, _transaction))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var detail = new BusinessObjects.ValueObjects.OrderDetail
                    {
                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        Quantity = (short)reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
                    };
                    orderDetails.Add(detail);
                }
            }

            return order;
        }


        public async Task<IEnumerable<BusinessObjects.POCOEntities.Order>> GetOrdersAsync(int cantidad)
        {
            var orders = new List<BusinessObjects.POCOEntities.Order>();
            var orderDictionary = new Dictionary<int, BusinessObjects.POCOEntities.Order>();

            using (var cmd = new SqlCommand("GetRecentOrdersWithDetails", _connection, _transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@cantidad", cantidad);

                using var reader = await cmd.ExecuteReaderAsync();

                // Leer órdenes (primer result set)
                while (await reader.ReadAsync())
                {
                    var order = new BusinessObjects.POCOEntities.Order
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        CustomerId = reader.GetString(reader.GetOrdinal("CustomerId")),
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                        ShipAddress = reader.GetString(reader.GetOrdinal("ShipAddress")),
                        ShipCity = reader.GetString(reader.GetOrdinal("ShipCity")),
                        ShipCountry = reader.GetString(reader.GetOrdinal("ShipCountry")),
                        ShipPostalCode = reader.GetString(reader.GetOrdinal("ShipPostalCode")),
                        OrderDetails = new List<BusinessObjects.ValueObjects.OrderDetail>()
                    };
                    orders.Add(order);
                    orderDictionary[order.Id] = order;
                }

                // Pasar al siguiente result set (detalles)
                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    var detail = new BusinessObjects.ValueObjects.OrderDetail
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        Quantity = (short)reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
                    };

                    if (orderDictionary.TryGetValue(detail.OrderId, out var order))
                    {
                        order.OrderDetails.Add(detail);
                    }
                }
            }

            return orders;
        }



        public Task SaveChangesAsync()
        {
            _transaction.Commit();
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}