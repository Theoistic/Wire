using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Wire.Jwt
{
    [Flags]
    public enum JwtMode { Header = 0, Session = 1 }
    public static class APIJwt
    {
        public static void AddJwt(this API.APIPlugins self, Func<LoginModel, bool> UserValidation, JwtMode mode)
        {
            API.POST("/token", x =>
            {
                LoginModel model = x.Body.As<LoginModel>();

                if (UserValidation(model) == false)
                    return new { Error = "Invalid credentials" };

                var claims = new Dictionary<string, object> {
                    { "name", model.UserName },
                    { "admin", true }
                };

                TokenValidationModel token = new TokenValidationModel {
                    Token = JsonWebToken.NewToken(claims)
                };

                if (mode.HasFlag(JwtMode.Session))
                {
                    x.HttpContext.Session.Set("IdentityToken", token);
                }

                return token;
            });
            API.BeforeRequest(x =>
            {
                if (mode.HasFlag(JwtMode.Header)) { 
                    if (x.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues values))
                    {
                        try
                        {
                            string token = values.ToString().Split(' ')[1];
                            JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(token);

                            x.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(tokenInfo.Claims.Select(c => new Claim(c.Key, c.Value as string)), "jwt"));
                        }
                        catch (Exception ex)
                        {
                            // fail silently.
                            // throw ex;
                        }
                    }
                }

                if (mode.HasFlag(JwtMode.Session))
                {
                    TokenValidationModel identityToken = x.HttpContext.Session.Get<TokenValidationModel>("IdentityToken");
                    if (identityToken != null)
                    {
                        JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(identityToken.Token);
                        List<Claim> claims = tokenInfo.Claims.Select(c => new Claim(c.Key, c.Value.ToString())).ToList();
                        x.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
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
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class TokenValidationModel
    {
        public string Token { get; set; }
    }
}
