using Shipping.DHL.Common;
using System.ServiceModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddScoped<DHL24WebapiPortClient>(_ =>
{
    var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

    var endpoint = new EndpointAddress("https://sandbox.dhl24.com.pl/webapi2/provider/service.html?ws=1");

    return new DHL24WebapiPortClient(binding, endpoint);
});

builder.Services.AddControllers();

builder.Services.Configure<DhlAuth>(
    builder.Configuration.GetSection("DhlAuth"));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Run();

