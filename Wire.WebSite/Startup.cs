using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.WebUtilities;
using Wire.Razor;
using Wire.Jwt;

namespace Wire.WebSite
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWire();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseWire(env);

            //API.Plugins.AddJwt(x => x.Username == x.Password, JwtMode.Header | JwtMode.Session);

            //API.GET("/admin", x => new View("Admin", new IndexModel { }));

            /*API.RULE("/admin/{#path}", x => (x.HttpContext.User.Identity.IsAuthenticated ? null : new { Message = "Not authenticated." }));
            // Admin
            API.GET("/login", x =>
            {
                return new View("login", new IndexModel { Name = "This is the login screen" });
            });
            API.GET("/admin/blog", x =>
            {
                return new View("blog", new IndexModel { Name = "This is the blog screen" });
            });
            API.GET("/admin/pages", x =>
            {
                return new View("pages", new IndexModel { Name = "This is the pages screen" });
            });
            API.GET("/admin/media", x =>
            {
                return new View("media", new IndexModel { Name = "This is the media screen" });
            });
            API.GET("/admin/menus", x =>
            {
                return new View("menus", new IndexModel { Name = "This is the menus screen" });
            });*/



            API.GET("/file/{filename}", x =>
            {
                return new ContentResult(x.Parameters.filename, "image/png");
            });

            API.POST("/upload", x =>
            {
                // TODO. get the file content from the httpContext
                var newFileName = string.Empty;

                if (x.HttpContext.Request.Form.Files != null)
                {
                    var fileName = string.Empty;

                    var files = x.HttpContext.Request.Form.Files;

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //Getting FileName
                            fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                            //Getting file Extension
                            var FileExtension = Path.GetExtension(fileName);

                            // concating  FileName + FileExtension
                            newFileName = fileName + FileExtension;

                            // Combines two strings into a path.
                            fileName = Path.Combine(API.env.WebRootPath, "") + $@"\{newFileName}";

                            using (FileStream fs = System.IO.File.Create(fileName))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }
                        }
                    }
                }
                return new { OK = 200 };
            });
        }
    }

    
}
