
using Microsoft.Win32;
using System.Diagnostics;
using NorthWind.Sales.Backend.DataContexts.Dapper.Options;
using NorthWind.Sales.Backend.DataContexts.AdoNet.Options;
using NorthWind.Sales.Backend.DataContexts.EFCore.Options;
using NorthWind.Sales.Backend.IoC;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.CreateOrder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.Repositories;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;

var builder = WebApplication.CreateBuilder(args);

// Documentaci�n Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Northwind API", Version = "v1" });
});


// Leer el proveedor desde appsettings.json
var provider = builder.Configuration.GetValue<string>("DatabaseProvider");

// Mostrar el proveedor en consola
Console.WriteLine($"[INFO] Proveedor de base de datos seleccionado: {provider}");

// Inyección dinámica de servicios personalizados
switch (provider)
{
    case "EFCore":
        builder.Services.AddNorthWindSalesServices(
            dbOptions => builder.Configuration
                .GetSection(NorthWind.Sales.Backend.DataContexts.EFCore.Options.DBOptions.SectionKey)
                .Bind(dbOptions)
        );
        break;
    case "Dapper":
        builder.Services.AddNorthWindSalesServices(
            dbOptions => builder.Configuration
                .GetSection(NorthWind.Sales.Backend.DataContexts.Dapper.Options.DBOptions.SectionKey)
                .Bind(dbOptions)
        );
        break;
    case "AdoNet":
        builder.Services.AddNorthWindSalesServices(
            dbOptions => builder.Configuration
                .GetSection(NorthWind.Sales.Backend.DataContexts.AdoNet.Options.DBOptions.SectionKey)
                .Bind(dbOptions)
        );
        break;
    default:
        throw new Exception("Proveedor de base de datos no soportado.");
}

// Habilitar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Mostrar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Ejemplo de endpoint de inserción con medición de tiempo y uso de input/output port
app.MapPost("/CreateOrder", async (
    CreateOrderDto dto,
    ICreateOrderInputPort inputPort,
    ICreateOrderOutputPort outputPort) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await inputPort.Handle(dto);
    stopwatch.Stop();
    Console.WriteLine($"[INFO] Tiempo de inserción: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"[INFO] Proveedor de base de datos seleccionado: {provider}");


    // Si necesitas devolver algo del outputPort, puedes hacerlo aquí
    // Por ejemplo: return Results.Ok(await outputPort.GetResultAsync());
    return Results.Ok(outputPort.OrderId);
});

app.MapGet("/GetOrder/{orderId:int}", async (
    int orderId,
    [FromServices] IQueriesRepository service) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    var order = await service.GetOrderAndDetailsAsync(orderId);

    stopwatch.Stop();
    Console.WriteLine($"[INFO] Tiempo de consulta: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"[INFO] Proveedor de base de datos seleccionado: {provider}");

    if (order is null)
        return Results.NotFound($"Orden con ID {orderId} no encontrada.");

    var result = new OrderWithDetailsDto
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        OrderDate = order.OrderDate,
        ShipAddress = order.ShipAddress,
        ShipCity = order.ShipCity,
        OrderDetails = order.OrderDetails.Select(d => new OrderDetailDto
        {
            OrderId = d.OrderId,
            ProductId = d.ProductId,
            UnitPrice = d.UnitPrice,
            Quantity = d.Quantity,
        }).ToList()
    };

    return Results.Ok(result);
});


app.MapGet("/GetOrders/{cantidad:int}", async (
    int cantidad,
    [FromServices] IQueriesRepository service,
    HttpContext context) =>
{
    // Validación temprana
    if (cantidad <= 0 || cantidad > 1000000)
        return Results.BadRequest("La cantidad debe estar entre 1 y 1,000,000.");

    // Medición de rendimiento
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Ejecución de la consulta
    var orders = await service.GetOrdersAsync(cantidad);

    stopwatch.Stop();
    context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("GetOrders")
        .LogInformation("⏱️ Tiempo de consulta: {Elapsed} ms | Cantidad: {Cantidad}",
            stopwatch.ElapsedMilliseconds, cantidad);

    if (orders is null || orders.Count() == 0)
        return Results.NotFound("No se encontraron órdenes.");

    // Proyección eficiente a DTOs usando expresión explícita
    var results = orders.Select(static order => new OrderWithDetailsDto
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        OrderDate = order.OrderDate,
        ShipAddress = order.ShipAddress,
        ShipCity = order.ShipCity,
        OrderDetails = order.OrderDetails.Select(static d => new OrderDetailDto
        {
            OrderId = d.OrderId,
            ProductId = d.ProductId,
            UnitPrice = d.UnitPrice,
            Quantity = d.Quantity
        }).ToList()
    }).ToList(); // Materializa resultado para evitar múltiples enumeraciones

    return Results.Ok(results);
});


// Leer desde consola cuántos registros insertar
Console.Write("Ingrese cuántos registros desea insertar: ");
if (!int.TryParse(Console.ReadLine(), out int registros))
{
    Console.WriteLine("❌ Entrada inválida. Se utilizará 10 por defecto.");
    registros = 10;
}


// Benchmark desde consola
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var config = services.GetRequiredService<IConfiguration>();
        var providerI = builder.Configuration.GetValue<string>("DatabaseProvider");
        var repository = services.GetRequiredService<ICommandsRepository>();

        Console.WriteLine($"🔄 Insertando {registros} registros con {providerI}...");

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < registros; i++)
        {
            var order = new OrderAggregate
            {
                CustomerId = "ALFKI",
                OrderDate = DateTime.UtcNow,
                ShipAddress = $"Address {i}",
                ShipCity = $"City {i % 100}",
                ShipCountry = $"Country {i % 10}",
                ShipPostalCode = $"{10000 + (i % 90000)}",
                Discount = 10,
                DiscountType = 0,
                ShippingType = (NorthWind.Sales.Backend.BusinessObjects.Enums.ShippingType)1
            };

            // Detalle
            order.AddDetail(
                productId: 1,
                unitPrice: 10 + (i % 50),
                quantity: (short)((i % 5) + 1)
            );

            await repository.CreateOrder(order);
        }

        await repository.SaveChanges();

        sw.Stop();
        Console.WriteLine($"✅ Tiempo total con {providerI}: {sw.ElapsedMilliseconds} ms");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"🔍 Inner: {ex.InnerException.Message}");
            if (ex.InnerException.InnerException != null)
                Console.WriteLine($"🔍 Inner Inner: {ex.InnerException.InnerException.Message}");
        }
    }
}


    app.Run();