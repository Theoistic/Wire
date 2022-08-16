﻿using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Wire
{
    public static class Utils
    {
        public static HttpMethod GetHttpMethod(this string self)
        {
            return (HttpMethod)Enum.Parse(typeof(HttpMethod), self);
        }

        public static string GetJsonBody(this IContext self)
        {
            var bodyStr = "";

            // Allows using several time the stream in ASP.Net Core
            //req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader = new StreamReader(self.RequestStream, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            self.RequestStream.Position = 0;

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

        public static Dictionary<string, string> ParseQueryString(this string self)
        {
            var queryString = new Dictionary<string, string>();
            var query = self.Split('&');
            foreach (var item in query)
            {
                var keyValue = item.Split('=');
                queryString.Add(keyValue[0], keyValue[1]);
            }
            return queryString;
        }

        public static Dictionary<string, string> ParseQueryString(this NameValueCollection requestQueryString)
        {
            //Dictionary<string, string> rc = new Dictionary<string, string>();
            //string[] ar1 = requestQueryString.Split(new char[] { '&', '?' });
            //foreach (string row in ar1)
            //{
            //    if (string.IsNullOrEmpty(row)) continue;
            //    int index = row.IndexOf('=');
            //    rc[Uri.UnescapeDataString(row.Substring(0, index))] = Uri.UnescapeDataString(row.Substring(index + 1)); // use Unescape only parts          
            //}
            //return rc;
            
            var dict = new Dictionary<string, string>();
            if (requestQueryString != null)
            {
                foreach (string key in requestQueryString.AllKeys)
                {
                    dict.Add(key, requestQueryString[key]);
                }
            }
            return dict;
        }

        public static bool HasValue(this ExpandoObject self, string property)
        {
            return (self as IDictionary<string, object>).ContainsKey(property);
        }

        public static T TryGetValue<T>(this ExpandoObject self, string property)
        {
            return self.HasValue(property) ? (self as IDictionary<string, T>)[property] : default(T);
        }

        /*public static string TryGetValue(this IHeaderDictionary self, string property)
        {
            if(self.TryGetValue(property, out StringValues value))
            {
                return value.ToString();
            }
            return "";
        }*/

        public static byte[] ToBytes(this string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        public static string FromBytes(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        internal static APIBehaviours FindMatchs(this APIBehaviours behaviours, Uri path)
        {
            APIBehaviours _behaviours = new APIBehaviours();
            foreach (APIBehaviour temp in behaviours)
            {
                var parameters = temp.Uri.GetParameters(path);
                if (parameters != null)
                {
                    _behaviours.Add(temp);
                }
            }
            return _behaviours;
        }

        /*public static List<Assembly> GetModuleAssemblies()
        {
            if (API.env == null)
                return null;

            List<Assembly> _asm = new List<Assembly>();

            string _moduleDirectory = Path.Combine(API.env.ContentRootPath, "modules");
            if (!Directory.Exists(_moduleDirectory))
                return null;

            foreach(var f in Directory.EnumerateFiles(_moduleDirectory, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    _asm.Add(Assembly.LoadFile(f));
                } catch(Exception ex)
                {
                    // unable to load file .. maybe unsupported format.
                }
            }

            return _asm;
        }*/

        public static bool ValidateJSON(this string s)
        {
            try
            {
                var test = JsonSerializer.Serialize(s);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
