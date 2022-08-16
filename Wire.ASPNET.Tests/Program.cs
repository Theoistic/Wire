using Wire;
using Wire.ASPNET;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWire();

API.GET("/", x =>
{
    return new { Message = "Hello World" };
});

app.Run();
