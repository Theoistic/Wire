
using Wire;

var server = new WireHTTPServer(8082);

API.GET("/", x => new { Message = "Hello World!" });

Console.ReadLine();
server.Stop();
