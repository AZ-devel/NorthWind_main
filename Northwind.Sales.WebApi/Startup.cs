using System.Diagnostics;
using NorthWind.Sales.Backend.DataContexts.EFCore.Options;
using NorthWind.Sales.Backend.DataContexts.Dapper.Options;
using NorthWind.Sales.Backend.DataContexts.AdoNet.Options;
using NorthWind.Sales.Backend.IoC;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.CreateOrder;
using NorthWind.Sales.Entities.Dtos.CreateOrder;

namespace Northwind.Sales.WebApi;

// Esto expone 2 metodos de extension para configurar los servicios web
// y agregar los middlewares y endpoints de la Web API

internal static class Startup
{
    //Metodo de extension.
    public static WebApplication CreateWebApplication(this WebApplicationBuilder builder)
    {
        //Configurar APIExplorer para descubrir y exponer 
        //los metadatos de los "endpoints" de la aplicacion.
        builder.Services.AddEndpointsApiExplorer();

        //Agregar el generador que construye los objetos de
        // documentacion de Swagger que tengan la funcionalidad de: ApiExplorer.
        builder.Services.AddSwaggerGen();

        // Leer el proveedor desde la configuración
        var provider = builder.Configuration.GetValue<string>("DatabaseProvider");

        switch (provider)
        {
            case "EFCore":
                builder.Services.AddNorthWindSalesServices(
                    dbOptions => builder.Configuration.GetSection(NorthWind.Sales.Backend.DataContexts.EFCore.Options.DBOptions.SectionKey).Bind(dbOptions)
                );
                break;
            case "Dapper":
                builder.Services.AddNorthWindSalesServices(
                    dbOptions => builder.Configuration.GetSection(DapperDBOptions.SectionKey).Bind(dbOptions)
                );
                break;
            case "AdoNet":
                builder.Services.AddNorthWindSalesServices(
                    dbOptions => builder.Configuration.GetSection(AdoNetDBOptions.SectionKey).Bind(dbOptions)
                );
                break;
            default:
                throw new InvalidOperationException("Proveedor de base de datos no soportado.");
        }

        // Agregar el servicio CORS para clientes (Web, Movil, etc.) que se ejecuten
        // en el navegador Web (Blazor, React, Angular).
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(config =>
            {
                config.AllowAnyMethod();
                config.AllowAnyHeader();
                config.AllowAnyOrigin();
            });
        });

        return builder.Build();
    }

    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        // Endpoint funcional para pruebas POST
        app.MapPost("/CreateOrder", async (
            CreateOrderDto dto,
            ICreateOrderInputPort inputPort,
            ICreateOrderOutputPort outputPort,
            IConfiguration config // Inyecta la configuración aquí
        ) =>
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("[DEBUG] Ejecutando /CreateOrder");

            await inputPort.Handle(dto);

            stopwatch.Stop();
            var provider = config.GetValue<string>("DatabaseProvider");
            Console.WriteLine($"[INFO] Tiempo de inserción: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"[INFO] Proveedor: {provider}");

            return Results.Ok("Inserción completada");
        });

        // Agregar el middleware CORS
        app.UseCors();

        return app;
    }
}
