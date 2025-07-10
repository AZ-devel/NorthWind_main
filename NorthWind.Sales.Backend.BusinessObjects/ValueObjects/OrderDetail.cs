namespace NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

//  FUNCIÓN: Permite guardar el detalle de la orden.
//           Son inmutables (solo de lectura).
public class OrderDetail
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }

    public OrderDetail() { } // Constructor requerido por EF Core

    public OrderDetail(int productId, decimal unitPrice, short quantity)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public OrderDetail(int orderId, int productId, decimal unitPrice, short quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Order Order { get; set; }

}


