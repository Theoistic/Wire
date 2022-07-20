using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wire
{
    public class JsonResult : RenderedResult<object>
    {
        public JsonResult(object result)
        {
            _contentType = "application/json";
            _result = result;
        }

        protected override void Render(Stream s, object t)
        {
            var buffer = JsonSerializer.SerializeToUtf8Bytes(_result);
            s.Write(buffer, 0, buffer.Length);
        }
    }
}
