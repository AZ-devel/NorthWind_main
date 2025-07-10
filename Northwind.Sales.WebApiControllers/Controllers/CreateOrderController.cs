using Microsoft.AspNetCore.Mvc;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.CreateOrder;
using NorthWind.Sales.Entities.Dtos.CreateOrder;

namespace Northwind.Sales.WebApiControllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateOrderController : ControllerBase
    {
        private readonly ICreateOrderInputPort _inputPort;
        private readonly ICreateOrderOutputPort _presenter;

        public CreateOrderController(ICreateOrderInputPort inputPort, ICreateOrderOutputPort presenter)
        {
            _inputPort = inputPort;
            _presenter = presenter;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromBody] CreateOrderDto orderDto)
        {
            await _inputPort.Handle(orderDto);
            return Ok(_presenter.OrderId);
        }
    }
}
