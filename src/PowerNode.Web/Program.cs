using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PowerNode.Web;
using PowerNode.Web.Servicios;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(_ => http);

// El motor se arma UNA vez al arrancar, no por pantalla: el JSON de la norma se descarga una sola
// vez y las trece tablas quedan en memoria. Son 39 KB.
builder.Services.AddSingleton(await MotorNom.CargarAsync(http));

await builder.Build().RunAsync();
