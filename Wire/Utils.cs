using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Wire
{
    public static class Utils
    {
        public static HttpMethod GetHttpMethod(this string self)
        {
            return (HttpMethod)Enum.Parse(typeof(HttpMethod), self);
        }

        public static string GetJsonBody(this HttpContext self)
        {
            var bodyStr = "";
            var req = self.Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            req.Body.Position = 0;

            return bodyStr;
        }
        public static IEnumerable<Type> GetAllTypesWithAttribute<T>(this Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public static void AddUnique<T>(this IList<T> self, IEnumerable<T> items)
        {
            foreach (var item in items) {
                if (!self.Contains(item)) {
                    self.Add(item);
                }
            }
        }

    }
}
