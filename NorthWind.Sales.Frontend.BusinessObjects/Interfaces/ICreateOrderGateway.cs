
using NorthWind.Sales.Entities.Dtos.CreateOrder;

namespace NorthWind.Sales.Frontend.BusinessObjects.Interfaces;

// Esta interfaz permitira implementar una clase para encapsular el
// codigo cliente (frontend) que debe consumir la Web API (Web Services) para que permita crear una orden.
public interface ICreateOrderGateway
{
    Task<int> CreateOrderAsync(CreateOrderDto order);

}
