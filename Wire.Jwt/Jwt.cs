using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Wire.Jwt
{
    public static class APIJwt
    {
        public static void AddJwt(this API.APIPlugins self, Func<LoginModel, bool> UserValidation)
        {
            API.POST("/token", x =>
            {
                LoginModel model = x.Body.As<LoginModel>();

                if (UserValidation(model) == false)
                    return new { Error = "Invalid credentials" };

                var claims = new Dictionary<string, object> {
                    { "name", model.Username },
                    { "admin", true }
                };

                return new
                {
                    token = JsonWebToken.NewToken(claims)
                };
            });
            API.BeforeRequest(x =>
            {
                if (x.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues values))
                {
                    try
                    {
                        string token = values.ToString().Split(' ')[1];
                        JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(token);

                        x.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, tokenInfo.Claims.First().Value as string)
                        }, "jwt"));
                    }
                    catch (Exception ex)
                    {
                        // fail silently.
                        // throw ex;
                    }
                }
            });
            API.GET("/token/validate", x =>
            {
                return x.HttpContext.User.Identity.IsAuthenticated;
            });
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    internal class TokenValidationModel
    {
        public string Token { get; set; }
    }
}
