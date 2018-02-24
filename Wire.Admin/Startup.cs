using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Solidb;
using System.Data.SqlClient;
using Wire.Jwt;

namespace Wire.Admin
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWire(env);
            Solidbase.Strategy = () => new SqlConnection("Server=.\\SQLEXPRESS;Database=NewSolidb;Trusted_Connection=True;");
            API.GET("/api/{Type}", x => 
            {
                Solidbase list = new Solidbase(x.Parameters.Type);
                return list.ToList();
            });
            API.GET("/api/{Type}/{Id}", x =>
            {
                Solidbase list = new Solidbase(x.Parameters.Type);
                return list.FirstOrDefault(z => z.Id == x.Parameters.Id);
            });
            API.POST("/api/{Type}", x =>
            {
                Solidbase list = new Solidbase(x.Parameters.Type);
                list.Add(x.Body.As<dynamic>());
                return list.ToList();
            });
            API.DELETE("/api/{Type}/{Id}", x =>
            {
                Solidbase list = new Solidbase(x.Parameters.Type);
                var itm = list.FirstOrDefault(z => z.Id == x.Parameters.Id);
                list.Remove(itm);
                return true;
            });

            //API.Plugins.AddJwt(x => x.Username == x.Password, JwtMode.Session);

            API.Conditions.Add("Authentication", x => x.HttpContext.User.Identity.IsAuthenticated);

            API.GET("/peep", x => "Hello There..", API.Conditions["Authentication"]);
        }
    }
}
