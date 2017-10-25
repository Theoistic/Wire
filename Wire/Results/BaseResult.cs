using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;

namespace Wire
{
    public abstract class BaseResult
    {
        protected NameValueCollection ResultHeaders;
        public abstract void Execute(HttpContext context);
    }
}
