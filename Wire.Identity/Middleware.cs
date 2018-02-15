using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Wire.Jwt;
using System.Linq;

namespace Wire.Identity
{
    public static class WireIdentityMiddleware
    {
        public static IApplicationBuilder UseIdentity(this IApplicationBuilder builder, Func<object> register, Func<LoginModel, bool> login)
        {
            API.POST("/register", x =>
            {
                try
                {
                    var result = register();
                } catch(Exception ex)
                {
                    return ex;
                }
                return null;
            });
            API.GET("/login", x =>
            {
                LoginModel model = x.Body.As<LoginModel>();

                if (login(model) == false)
                    return new { Error = "Invalid credentials" };

                var claims = new Dictionary<string, object> {
                    { "name", model.Username },
                    { "admin", true }
                };

                IdentityToken token = new IdentityToken { Token = JsonWebToken.NewToken(claims) };

                x.HttpContext.Session.Set("IdentityToken", token);

                return token;
            });
            API.BeforeRequest(x =>
            {
                IdentityToken identityToken = x.HttpContext.Session.Get<IdentityToken>("IdentityToken");
                if (identityToken != null)
                {
                    JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(identityToken.Token);
                    x.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(tokenInfo.Claims.Select(c => new Claim(c.Key , c.Value as string)), "jwt"));
                }
            });
            return builder;
        }
    }

    public class IdentityToken
    {
        public string Token { get; set; }
    }
}
