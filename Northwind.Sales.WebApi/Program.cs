//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

using Northwind.Sales.WebApi;

WebApplication.CreateBuilder(args)
    .CreateWebApplication()
    .ConfigureWebApplication()
    .Run();