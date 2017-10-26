using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            RegisterAllZones();
            return builder.UseMiddleware<WireMiddleware>();
        }

        private static void RegisterAllZones()
        {
            List<Type> types = new List<Type>();
            types.AddUnique(Assembly.GetEntryAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            types.AddUnique(Assembly.GetCallingAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            types.AddUnique(Assembly.GetExecutingAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            foreach (Type t in types)
            {
                Activator.CreateInstance(t);
            }
        }
    }
}
