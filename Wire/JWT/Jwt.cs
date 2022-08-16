using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Primitives;
using System.Net;

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
                    x.ResponseCookies.Add(new System.Net.Cookie("IdentityToken", token.Token)); 
                }

                return token;
            });
            API.BeforeRequest(x =>
            {
                if (mode.HasFlag(JwtMode.Header)) {
                    string? Authorization = x.RequestHeaders.Get("Authorization");
                    if (!string.IsNullOrEmpty(Authorization))
                    {
                        try
                        {
                            string token = Authorization.Split(' ')[1];
                            JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(token);

                            x.User = new ClaimsPrincipal(new ClaimsIdentity(tokenInfo.Claims.Select(c => new Claim(c.Key, c.Value.ToString())), "jwt"));
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
                    Cookie identityToken = x.RequestCookies.FirstOrDefault(x => x.Name == "IdentityToken"); // .Session.Get<TokenValidationModel>("IdentityToken");
                    if (identityToken != null)
                    {
                        string token = identityToken.Value;
                        try {  }
                        catch { }
                        JsonWebToken.TokenInformation tokenInfo = JsonWebToken.DecodeToken(token);
                        List<Claim> claims = tokenInfo.Claims.Select(c => new Claim(c.Key, c.Value.ToString())).ToList();
                        x.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                    }
                }
            });
            API.GET("/token/validate", x =>
            {
                return x.User?.Identity?.IsAuthenticated ?? false;
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
