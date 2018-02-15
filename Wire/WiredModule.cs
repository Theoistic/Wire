using System;
using System.Collections.Generic;
using System.Text;

namespace Wire
{
    [APIModule]
    public class WiredModule
    {
        // General HTTP Methods
        public void GET(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.GET(path, body, condition);
        public void POST(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.POST(path, body, condition);
        public void DELETE(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.DELETE(path, body, condition);
        public void PUT(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.PUT(path, body, condition);
        public void OPTIONS(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.OPTIONS(path, body, condition);
        public void PATCH(string path, Func<Context, object> body, Func<Context, bool> condition = null) => API.PATCH(path, body, condition);

        // Request Pre-Post Handling
        public void BeforeRequest(Action<Context> body) => API.BeforeRequest(body);
        public void AfterRequest(Action<Context> body) => API.AfterRequest(body);
    }
}
