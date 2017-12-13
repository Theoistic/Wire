using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    public enum HttpMethod { GET, POST, DELETE, PUT, OPTIONS, PATCH }

    public class APIBehaviour
    {
        public HttpMethod Method { get; set; }
        public UriTemplate Uri { get; set; }
        public Func<Context, object> Function { get; set; }
        public Func<Context, bool> Condition { get; set; }
    }

    public class Context
    {
        public dynamic Parameters { get; internal set; }
        public IDictionary<string, string> QueryString => HttpContext.Request.QueryString.Value.ParseQueryString();
        public HttpContext HttpContext { get; set; }
        public ContextBody Body { get; set; }
    }

    public class ContextBody
    {
        private string _body { get; set; }
        public ContextBody(string body) => _body = body;
        public override string ToString() => _body;
        public T As<T>() where T : class => JsonConvert.DeserializeObject<T>(_body);
        public dynamic As(Type type) => JsonConvert.DeserializeObject(_body, type);
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

        public new void Add(APIBehaviour item)
        {
            Predicate<APIBehaviour> _match = x => x.Uri == item.Uri && x.Method == item.Method;
            if (Exists(_match))
            {
                RemoveAll(_match);
                base.Add(item);
            } else
            {
                base.Add(item);
            }
        }
    }

    public static partial class API 
    {
        public static Dictionary<HttpMethod, APIBehaviours> Behaviours { get; set; } = new Dictionary<HttpMethod, APIBehaviours>() {
            { HttpMethod.GET, new APIBehaviours() },
            { HttpMethod.POST, new APIBehaviours() },
            { HttpMethod.DELETE, new APIBehaviours() },
            { HttpMethod.PUT, new APIBehaviours() },
            { HttpMethod.OPTIONS, new APIBehaviours() },
            { HttpMethod.PATCH, new APIBehaviours() }
        };

        public static void GET(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.GET].Add(path, body, condition);
        public static void POST(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.POST].Add(path, body, condition);
        public static void DELETE(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.DELETE].Add(path, body, condition);
        public static void PUT(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.PUT].Add(path, body, condition);
        public static void OPTIONS(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.OPTIONS].Add(path, body, condition);
        public static void PATCH(string path, Func<Context, object> body, Func<Context, bool> condition = null) => Behaviours[HttpMethod.PATCH].Add(path, body, condition);


        internal static List<Action<Context>> beforeRequest = new List<Action<Context>>();
        internal static List<Action<Context>> afterRequest = new List<Action<Context>>();
        public static void BeforeRequest(Action<Context> body) => beforeRequest.Add(body);
        public static void AfterRequre(Action<Context> body) => afterRequest.Add(body);

        public static Uri GetURI(HttpContext context) => new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}");
        public static APIBehaviour Match(HttpContext context) => Behaviours[context.Request.Method.ToUpper().GetHttpMethod()].FindMatch(GetURI(context));

        public static async Task<bool> Resolve(HttpContext httpContext)
        {
            APIBehaviour behaviour = API.Match(httpContext);
            if (behaviour != null)
            {
                BaseResult result;

                Type[] MimeResolvers = Assembly.GetAssembly(typeof(API)).GetTypes().Where(x => x.GetCustomAttributes(typeof(AcceptHeaderAttribute), true).Length > 0).ToArray();
                string[] SupportedMimesTypes = MimeResolvers.Select(x => (x.GetCustomAttribute(typeof(AcceptHeaderAttribute), true) as AcceptHeaderAttribute).Header).ToArray();

                Context context = new Context
                {
                    Parameters = new ExpandoObject(),
                    HttpContext = httpContext,
                    Body = new ContextBody(httpContext.GetJsonBody())
                };
                var _params = behaviour.Uri.GetParameters(API.GetURI(httpContext));
                foreach (var _p in _params) {
                    (context.Parameters as IDictionary<string, object>).Add(_p.Key, _p.Value);
                }

                string[] header = context.HttpContext.Request.Headers.TryGetValue("Accept").Split(',');
                if (SupportedMimesTypes.Intersect(header).Count() == 0) {
                    header = new string[] { "application/json" };
                }

                beforeRequest.ForEach(x => x.Invoke(context));

                if (behaviour.Condition != null)
                {
                    if(behaviour.Condition.Invoke(context) == false)
                    {
                        result = new JsonResult(new { Error = "Condition Failed." });
                        result.Execute(httpContext);
                        return await Task.FromResult(true);
                    }
                }

                object funcResult = behaviour.Function.Invoke(context);

                afterRequest.ForEach(x => x.Invoke(context));

                Type BaseResultType = MimeResolvers.FirstOrDefault(x => header.Contains((x.GetCustomAttribute(typeof(AcceptHeaderAttribute), true) as AcceptHeaderAttribute).Header));

                if (BaseResultType == null)
                {
                    result = new JsonResult(new { Error = "These mime types are not supported", Types = header });
                    result.Execute(httpContext);
                }
                else
                {
                    result = Activator.CreateInstance(BaseResultType, new[] { funcResult }) as BaseResult;

                    if (result.IsExecutionReady)
                    {
                        result.Execute(httpContext);
                    }
                    else
                    {
                        result = new JsonResult(new { Error = "Accept header does not match the returned type." });
                        result.Execute(httpContext);
                    }
                }

                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        // Allow a fluent plugin architecture.
        public static APIPlugins Plugins = new API.APIPlugins();
        public class APIPlugins { }
    }

    public static partial class API
    {
        public static Dictionary<string, Func<Context, bool>> Conditions = new Dictionary<string, Func<Context, bool>> { };
    }
}
