using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text.Json;

namespace Wire
{

    public interface IContext
    {
        HttpMethod HttpMethod { get; }
        Uri URL { get; }
        dynamic Parameters { get; } 
        IDictionary<string, string> QueryString { get; }
        ContextBody Body { get; set; }
        IPrincipal? User { get; set; }
        NameValueCollection RequestHeaders { get; }
        CookieCollection RequestCookies { get; }
        Stream RequestStream { get; }
        Stream ResponseStream { get; }
        CookieCollection ResponseCookies { get; }
        void WriteToResponse(string ContentType, byte[] data = null);
    }

    public class Context : IContext
    {
        public HttpMethod HttpMethod => HttpContext.Request.HttpMethod.ToUpper().GetHttpMethod();
        public Uri URL => HttpContext.Request.Url;

        public dynamic Parameters { get; internal set; } = new ExpandoObject();
        public IDictionary<string, string> QueryString => HttpContext.Request.QueryString.ParseQueryString();
        internal HttpListenerContext HttpContext { get; set; }
        public ContextBody Body { get; set; }
        
        public IPrincipal? User { get; set; }

        public NameValueCollection RequestHeaders => HttpContext.Request.Headers;
        public CookieCollection RequestCookies => HttpContext.Request.Cookies;
        public Stream RequestStream => HttpContext.Request.InputStream;


        public Stream ResponseStream => HttpContext.Response.OutputStream;
        public CookieCollection ResponseCookies => HttpContext.Response.Cookies;

        public void WriteToResponse(string ContentType, byte[] data = null)
        {
            SetContentType(ContentType);
            if (data != null)
                HttpContext.Response.OutputStream.Write(data, 0, data.Length);
        }

        public void SetContentType(string ContentType)
        {
            HttpContext.Response.ContentType = ContentType;
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        /*private HttpListenerRequest Request => HttpContext.Request;
        private HttpListenerResponse Response => HttpContext.Response;*/
    }

    public class ContextBody
    {
        private string _body { get; set; }
        public ContextBody(string body) => _body = body;
        public ContextBody(Stream stream)
        {
            using var reader = new StreamReader(stream);
            _body = reader.ReadToEnd();
        }
        
        public override string ToString() => _body;

        public T As<T>() where T : class
        {
            if (_body.ValidateJSON())
            {
                return JsonSerializer.Deserialize<T>(_body);
            }
            else
            {
                var _bodyDict = _body.Split('&').Select(q => q.Split('='))
                   .ToDictionary(k => k[0], v => v[1]); // might need to decode the value since its stored in a query string.
                var _bodyJsonified = JsonSerializer.Serialize(_bodyDict);
                return JsonSerializer.Deserialize<T>(_bodyJsonified);
            }
        }

        public dynamic As(Type type) => JsonSerializer.Deserialize(_body, type);
    }
}
