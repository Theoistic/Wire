using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Net;
using System.Dynamic;

namespace Wire.ASPNET
{
    public class WireASPNETContext : IContext
    {
        public HttpMethod HttpMethod => context.Request.Method.GetHttpMethod();

        public Uri URL => new Uri($"{context.Request.Host}{context.Request.Path}");

        public dynamic Parameters { get; } = new ExpandoObject();

        public IDictionary<string, string> QueryString => context.Request.QueryString.Value.ParseQueryString();

        public ContextBody Body { get; set; }
        public IPrincipal? User { get; set; }

        public NameValueCollection RequestHeaders => new NameValueCollection();

        public CookieCollection RequestCookies => new CookieCollection();

        public Stream RequestStream => context.Request.Body;

        public Stream ResponseStream => context.Response.Body;

        public CookieCollection ResponseCookies => new CookieCollection();

        public void WriteToResponse(string ContentType, byte[] data = null)
        {
            if (data != null)
                context.Response.Body.Write(data, 0, data.Length);
        }

        internal HttpContext context { get; set; }

        public WireASPNETContext(HttpContext context)
        {
            this.context = context;
        }
    }
}
