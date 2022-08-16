using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wire
{
    public enum HttpMethod { GET, POST, DELETE, PUT, OPTIONS, PATCH }

    public class APIBehaviour
    {
        public HttpMethod Method { get; set; }
        public UriTemplate Uri { get; set; }
        public WireAction Function { get; set; }
        public WireCondition Condition { get; set; }
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

        public void Add(string path, WireAction function, WireCondition condition = null) => Add(new APIBehaviour { Uri = new UriTemplate(path), Function = function, Condition = condition });

        public new void Add(APIBehaviour item)
        {
            Predicate<APIBehaviour> _match = x => x.Uri == item.Uri && x.Method == item.Method;
            if (Exists(_match)) // Replaces an existing binding..
            {
                RemoveAll(_match);
                base.Add(item);
            } else
            {
                base.Add(item);
            }
        }
    }

    public delegate bool WireCondition(IContext context);
    public delegate object WireAction(IContext contxt);

    public delegate void HttpRequest(string path, WireAction body, WireCondition condition = null);

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

        //public static APIBehaviours Rules { get; private set; } = new APIBehaviours();

        public static void GET(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.GET].Add(path, body, condition);
        public static void POST(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.POST].Add(path, body, condition);
        public static void DELETE(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.DELETE].Add(path, body, condition);
        public static void PUT(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.PUT].Add(path, body, condition);
        public static void OPTIONS(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.OPTIONS].Add(path, body, condition);
        public static void PATCH(string path, WireAction body, WireCondition condition = null) => Behaviours[HttpMethod.PATCH].Add(path, body, condition);

        public static IContext FromHttpListener(HttpListenerContext httpListenerContext)
        {
            var context = new Context { HttpContext = httpListenerContext };
            var request = httpListenerContext.Request;
            var queryString = request.QueryString;
            context.Body = new ContextBody(request.InputStream);
            context.Parameters = new ExpandoObject();
            foreach (var key in queryString.AllKeys)
            {
                context.Parameters[key] = queryString[key];
            }
            return context;
        }

        public static void RULE(string path, WireAction body)
        {
            beforeRequest.Add((context) =>
            {
                Uri uri = context.URL;
                UriTemplate tmpl = new UriTemplate(path);
                if(tmpl.GetParameters(uri) != null)
                {
                    object ruleResult = body(context);
                    if(ruleResult != null)
                    {
                        if (ruleResult is BaseResult)
                        {
                            Type typeOfResult = ruleResult.GetType();
                            (ruleResult as BaseResult).Execute(context);
                        }
                        else
                        {
                            var result = new JsonResult(ruleResult);
                            result.Execute(context);
                        }
                    }
                }
            });
        } 

        //public static object Call(HttpMethod method, string path, Context context) => Behaviours[method].FindMatch(new Uri($"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{path}")).Function(context);


        internal static List<Action<IContext>> beforeRequest = new List<Action<IContext>>();
        internal static List<Action<IContext>> afterRequest = new List<Action<IContext>>();

        public static void BeforeRequest(Action<IContext> body) => beforeRequest.Add(body);
        public static void AfterRequest(Action<IContext> body) => afterRequest.Add(body);

        public static Uri GetURI(IContext context) => context.URL;
        public static APIBehaviour Match(IContext context) => Behaviours[context.HttpMethod].FindMatch(GetURI(context));

        public static async Task<bool> Resolve(IContext context)
        {
            APIBehaviour behaviour = API.Match(context);
            if (behaviour != null)
            {
                BaseResult result;

                var _params = behaviour.Uri.GetParameters(API.GetURI(context));
                foreach (var _p in _params) {
                    (context.Parameters as IDictionary<string, object>).Add(_p.Key, _p.Value);
                }

                Action<object> deliverResult = (givenResult) => {
                    if (givenResult is BaseResult)
                    {
                        Type typeOfResult = givenResult.GetType();
                        (givenResult as BaseResult).Execute(context);
                    }
                    else
                    {
                        result = new JsonResult(givenResult);
                        result.Execute(context);
                    }
                };

                /*try
                {
                    List<object> RuleResults = new List<object>();
                    Rules.FindMatchs(API.GetURI(httpContext)).ForEach(x =>
                    {
                        var _paramsOfRule = x.Uri.GetParameters(API.GetURI(httpContext));
                        Context ruleContext = CreateContext();
                        foreach (var _p in _paramsOfRule)
                        {
                            (ruleContext.Parameters as IDictionary<string, object>)[_p.Key] = _p.Value;
                        }
                        RuleResults.Add(x.Function.Invoke(ruleContext));
                    });
                    if (RuleResults.Any(x => x != null))
                    {
                        deliverResult(RuleResults.FirstOrDefault(x => x != null));
                    }
                } catch (Exception ex)
                {
                    deliverResult(new { Message = ex.Message });
                }*/

                beforeRequest.ForEach(x => x.Invoke(context));

                if (behaviour.Condition != null)
                {
                    if(behaviour.Condition.Invoke(context) == false)
                    {
                        result = new JsonResult(new { Error = "Condition Failed." });
                        result.Execute(context);
                        return await Task.FromResult(true);
                    }
                }

                object funcResult = behaviour.Function.Invoke(context);

                afterRequest.ForEach(x => x.Invoke(context));

                deliverResult(funcResult);

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
        public static Dictionary<string, WireCondition> Conditions = new Dictionary<string, WireCondition> { };
    }
}
