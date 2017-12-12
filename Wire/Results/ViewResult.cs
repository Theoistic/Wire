using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wire.Results
{
    [AcceptHeader(Header = "text/html")]
    public class ViewResult : RenderedResult<string>
    {
        public ViewResult(object result)
        {
            _contentType = "text/html";
            _result = result as string;
        }

        protected override void Render(Stream s, string t)
        {
            var buffer = Encoding.UTF8.GetBytes(_result);
            s.Write(buffer, 0, buffer.Length);
        }
    }
}
