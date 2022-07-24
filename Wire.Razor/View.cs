using System;
using Microsoft.AspNetCore.Http;
using RazorLight;
using System.Text;
using System.Reflection;

namespace Wire.Razor
{
    public class View : BaseResult
    {
        public override bool IsExecutionReady => true;
        public string result { get; private set; }

        public View(string viewName, object model = null)
        {
            var engine = new RazorLightEngineBuilder()
              .UseFilesystemProject(Assembly.GetExecutingAssembly().Location)
              .UseMemoryCachingProvider()
              .Build();

            result = engine.CompileRenderAsync($"Views/{viewName}.cshtml", model).Result;
        }

        public override void Execute(Context context)
        {
            context.Response.ContentType = "text/html";
            var buffer = Encoding.UTF8.GetBytes(result);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
