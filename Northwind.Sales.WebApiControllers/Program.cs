using Microsoft.OpenApi.Models;
//using NorthWind.Sales.Backend.DataContexts.Dapper.Options;
using NorthWind.Sales.Backend.DataContexts.AdoNet.Options;
//using NorthWind.Sales.Backend.DataContexts.EFCore.Options;
using NorthWind.Sales.Backend.IoC;

var builder = WebApplication.CreateBuilder(args);

// Documentación Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Northwind API", Version = "v1" });
});

// Inyección de servicios personalizados
builder.Services.AddNorthWindSalesServices(
    dbOptions => builder.Configuration.GetSection(DBOptions.SectionKey).Bind(dbOptions)
);

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

// Registrar soporte para controladores
builder.Services.AddControllers();

var app = builder.Build();

// Mostrar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

//Registrar el middleware para controladores
app.MapControllers();

app.Run();
