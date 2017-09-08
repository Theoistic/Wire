using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    public class WireMiddleware
    {
        private readonly RequestDelegate _next;

        public WireMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var processRequest = await API.Resolve(httpContext);
            if(!processRequest)
            {
                await _next.Invoke(httpContext);
            }
        }
    }

    public static class WireMiddlewareExtensions
    {
        public static IApplicationBuilder UseWire(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WireMiddleware>();
        }
    }
}
