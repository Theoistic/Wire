using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wire
{
    public class Redirect : BaseResult
    {
        public override bool IsExecutionReady => true;
        public string path { get; private set; }

        public Redirect(string path)
        {
            this.path = path;
        }

        public override void Execute(HttpContext context)
        {
            if (path.StartsWith("/"))
            {
                context.Response.Redirect($"{context.Request.Scheme}://{context.Request.Host}{path}");
            }
            else
            {
                context.Response.Redirect(path);
            }
        }
    }
}
