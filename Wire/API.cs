using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    public class APIBehaviour
    {
        public UriTemplate Uri { get; set; }
        public Func<Context, object> Function { get; set; }
        public Func<Context, bool> Condition { get; set; }
    }

    public class Context
    {
        public IDictionary<string, object> Parameters { get; set; }
        public HttpContext HttpContext { get; set; }
    }

    public class APIBehaviours : List<APIBehaviour>
    {
        public APIBehaviour FindMatch(Uri uri) {
            foreach (APIBehaviour temp in this) {
                var parameters = temp.Uri.GetParameters(uri);
                if (parameters != null) {
                    return temp;
                }
            }
            return null;
        }

        public void Add(string path, Func<Context, object> function, Func<Context, bool> condition = null) => Add(new APIBehaviour { Uri = new UriTemplate(path), Function = function, Condition = condition });
    }

    public class APIBehaviourSection : APIBehaviours
    {
        public Func<Context, bool> Condition { get; set; }
    }

    public static class API
    {
        public static Dictionary<string, APIBehaviours> _APIBehaviours { get; set; } = new Dictionary<string, APIBehaviours>() {
            { "GET", new APIBehaviours() },
            { "POST", new APIBehaviours() },
            { "DELETE", new APIBehaviours() },
            { "PUT", new APIBehaviours() },
            { "OPTIONS", new APIBehaviours() },
            { "PATCH", new APIBehaviours() }
        };

        public static void GET(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["GET"].Add(path, body, condition);
        public static void POST(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["POST"].Add(path, body, condition);
        public static void DELETE(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["DELETE"].Add(path, body, condition);
        public static void PUT(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["PUT"].Add(path, body, condition);
        public static void OPTIONS(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["OPTIONS"].Add(path, body, condition);
        public static void PATCH(string path, Func<Context, object> body, Func<Context, bool> condition = null) => _APIBehaviours["PATCH"].Add(path, body, condition);

        public static Uri GetURI(HttpContext context) => new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}");
        public static APIBehaviour Match(HttpContext context) => _APIBehaviours[context.Request.Method.ToUpper()].FindMatch(GetURI(context));

        public static async Task<bool> Resolve(HttpContext httpContext)
        {
            APIBehaviour behaviour = API.Match(httpContext);
            if (behaviour != null)
            {
                Context context = new Context
                {
                    Parameters = behaviour.Uri.GetParameters(API.GetURI(httpContext)),
                    HttpContext = httpContext
                };
                if (behaviour.Condition != null)
                {
                    if(behaviour.Condition.Invoke(context) == false)
                    {
                        return false; // we should not proceed since the condition was found and failed for this behaviour.
                    }
                }
                BaseResult result = new JsonResult(behaviour.Function.Invoke(context));
                result.Execute(httpContext);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public abstract class BaseResult
    {
        protected NameValueCollection ResultHeaders;
        public abstract void Execute(HttpContext context);
    }

    public abstract class RenderedResult<T> : BaseResult
    {
        protected T _result;
        protected string _contentType = "text/html";
        private MemoryStream _cachedSerializedResult;
        protected abstract void Render(Stream s, T t);

        public override void Execute(HttpContext context) {
            HttpResponse response = context.Response;
            response.ContentType = _contentType;
            if(_cachedSerializedResult != null) {
                throw new NotImplementedException();
            } else {
                Render(response.Body, _result);
            }
        }
    }

    public class JsonResult : RenderedResult<object>
    {
        private static readonly IsoDateTimeConverter _isoDateTimeConverter = new IsoDateTimeConverter();

        public JsonResult(object result, string contentType) : this(result) {
            _contentType = contentType;
        }

        public JsonResult(object result) {
            _contentType = "application/json";
            _result = result;
        }

        protected override void Render(Stream s, object t) {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_result, _isoDateTimeConverter));
            s.Write(buffer, 0, buffer.Length);
        }
    }
}
