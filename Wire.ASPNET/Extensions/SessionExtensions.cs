using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Wire
{
    public static class SessionExtensions
    {
        public static void Set(this ISession session, string key, object value)
        {
            session.Set(key, JsonSerializer.Serialize(value).ToBytes());
        }

        public static T? Get<T>(this ISession session, string key)
        {
            byte[] bytes;

            var value = session.TryGetValue(key, out bytes);

            return value == false ? default(T) : JsonSerializer.Deserialize<T>(bytes.FromBytes());
        }
    }
}
