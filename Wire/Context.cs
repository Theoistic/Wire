using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text.Json;

namespace Wire
{
    public class Context
    {
        public dynamic Parameters { get; internal set; } = new ExpandoObject();
        public IDictionary<string, string> QueryString => HttpContext.Request.QueryString.ParseQueryString();
        public HttpListenerContext HttpContext { get; set; }
        public ContextBody Body { get; set; }

        public IPrincipal? User { get; set; }

        public HttpListenerRequest Request => HttpContext.Request;
        public HttpListenerResponse Response => HttpContext.Response;
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
