using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NorthWind.Sales.BlazorClient;
using NorthWind.Sales.Frontend.IoC;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddNorthWindSalesServices(client =>
{
    client.BaseAddress =
    new Uri(builder.Configuration["WebApiAddress"]);
});

await builder.Build().RunAsync();
