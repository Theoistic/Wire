using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;

namespace Wire
{
    public class JsonResult : RenderedResult<object>
    {
        private static readonly IsoDateTimeConverter _isoDateTimeConverter = new IsoDateTimeConverter();


        public JsonResult(object result)
        {
            _contentType = "application/json";
            _result = result;
        }

        protected override void Render(Stream s, object t)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_result, _isoDateTimeConverter));
            s.Write(buffer, 0, buffer.Length);
        }
    }
}
