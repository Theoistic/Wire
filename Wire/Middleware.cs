using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        public static IServiceCollection AddWire(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            return services;
        }

        public static IApplicationBuilder UseWire(this IApplicationBuilder builder, IHostingEnvironment env)
        {
            API.env = env;
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
