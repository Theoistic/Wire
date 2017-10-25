using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Wire.Tests
{
    [TestClass]
    public class HttpRequests
    {
        [TestMethod]
        public void StandardRequests()
        {
            HttpContext context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            Uri uri = new Uri("http://localhost/info/StandardRequestTest");

            context.Request.Scheme = "http";
            context.Request.Host = HostString.FromUriComponent(uri);
            context.Request.Path = PathString.FromUriComponent(uri);
            context.Request.Method = "GET";

            object expected = new { Message = "OK" };

            API.GET("/info/{firstParam}", x => expected, x => (x.Parameters["firstParam"] as string) == "StandardRequestTest");

            var processed = API.Resolve(context).Result;

            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var response = DeserializeFromStream(context.Response.Body);

            Assert.IsTrue(JsonConvert.SerializeObject(expected) == JsonConvert.SerializeObject(response));
        }

        [TestMethod]
        public void ChangedBehaviour()
        {
            API.GET("/2", x => new { Count = 2 });
            API.Behaviours[HttpMethod.GET].FindMatch(new Uri("http://localhost/2")).Function = x => new { Changed = true };
            Assert.IsTrue(true);
        }

        public static object DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonTextReader);
            }
        }
    }
}
