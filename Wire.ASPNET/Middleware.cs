using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Wire.ASPNET
{
    public class WireMiddleware
    {
        private readonly RequestDelegate _next;

        internal static IServiceCollection services { get; set; }
        internal static IApplicationBuilder builder { get; set; }

        public WireMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        [Obsolete("Non-Functional after the rewrite to take asp.net out of the core library.")]
        public async Task Invoke(HttpContext httpContext)
        {
            var processRequest = await ASPNET.API.Resolve(httpContext);
            if(!processRequest)
            {
                await _next.Invoke(httpContext);
            }
        }
    }

    public static class WireMiddlewareExtensions
    {
        [Obsolete("Highly experimental feature.")]
        public static IServiceCollection AddWire(this IServiceCollection services)
        {
            WireMiddleware.services = services;
            WireMiddleware.services.AddDistributedMemoryCache();
            return WireMiddleware.services;
        }

        public static IApplicationBuilder UseWire(this IApplicationBuilder builder)
        {
            WireMiddleware.builder = builder;
            return WireMiddleware.builder.UseMiddleware<WireMiddleware>();
        }

        private static List<object> moduleInstances { get; set; }
        private static void RegisterAPIModules()
        {
            List<Type> types = new List<Type>();
            types.AddUnique(Assembly.GetEntryAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            types.AddUnique(Assembly.GetCallingAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            types.AddUnique(Assembly.GetExecutingAssembly().GetAllTypesWithAttribute<APIModuleAttribute>());
            foreach(var _refAsm in Assembly.GetCallingAssembly().GetReferencedAssemblies()) {
                Assembly asm = Assembly.Load(_refAsm);
                if (asm != null) {
                    types.AddUnique(asm.GetAllTypesWithAttribute<APIModuleAttribute>());
                }
            }
            moduleInstances = new List<object>();
            foreach (Type t in types)
            {
                moduleInstances.Add(Activator.CreateInstance(t));
            }
        }
    }
    
    internal static partial class API
    {
        public static async Task<bool> Resolve(HttpContext context)
        {

            IContext wireContext = FromHttpContext(context);
            return await Wire.API.Resolve(wireContext);
        }
        
        public static IContext FromHttpContext(HttpContext context)
        {
            WireASPNETContext wireContext = new WireASPNETContext(context)
            {
                
            };
            return wireContext;
        }
    }
}
