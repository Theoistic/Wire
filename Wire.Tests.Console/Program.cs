
using Wire;
using Wire.Jwt;

// Start the server
var server = new WireHTTPServer(8082);

// add jwt authentication
API.Plugins.AddJwt(x => x.UserName == "theo", JwtMode.Header);

// declare conditional function
WireCondition IsAuthenticated = (context) => context.User?.Identity?.IsAuthenticated ?? false;

// Sample API endpoint
API.GET("/", x => new { Message = "Hello World!" });

// Sample API endpoint with conditional function
API.GET("/secure", x => new { Message = "Secret Content" }, IsAuthenticated);

// Wait for key press to terminate program
server.Wait();
