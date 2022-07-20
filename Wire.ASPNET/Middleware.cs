using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Wire
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
            var context = new Context();
            var processRequest = await API.Resolve(context);
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
            WireMiddleware.services = services;
            WireMiddleware.services.AddDistributedMemoryCache();
            //WireMiddleware.services.AddSession();
            return WireMiddleware.services;
        }

        public static IApplicationBuilder UseWire(this IApplicationBuilder builder, IHostingEnvironment env, bool registerModules = true)
        {
            WireMiddleware.builder = builder;
            //API.env = env;

            //builder.ApplicationServices.GetService<IServiceCollection>().AddSession();
            //WireMiddleware.services.AddSession();
            //WireMiddleware.builder.UseSession();

            if (registerModules)
            {
                RegisterAPIModules();
            }
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
            /*List<Assembly> moduleAssemblies = Utils.GetModuleAssemblies();
            if (moduleAssemblies != null)
            {
                foreach (var modAsm in moduleAssemblies)
                {
                    types.AddUnique(modAsm.GetAllTypesWithAttribute<APIModuleAttribute>());
                }
            }*/
            moduleInstances = new List<object>();
            foreach (Type t in types)
            {
                moduleInstances.Add(Activator.CreateInstance(t));
            }
        }
    }
}
