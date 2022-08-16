using System;
using System.Collections.Generic;
using System.Text;

namespace Wire
{
    /*public class Redirect : BaseResult
    {
        public override bool IsExecutionReady => true;
        public string path { get; private set; }

        public Redirect(string path)
        {
            this.path = path;
        }

        public override void Execute(Context context)
        {
            if (path.StartsWith("/"))
            {
                context.Response.Redirect($"{path}"); // {context.Request.Scheme}://{context.Request.Host}
            }
            else
            {
                context.Response.Redirect(path);
            }
        }
    }*/
}
