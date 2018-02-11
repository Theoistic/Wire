using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wire
{
    public class ContentResult : BaseResult
    {
        protected byte[] _data;

        protected string _contentType = "text/html";
        private MemoryStream _cachedSerializedResult;

        public ContentResult() { }

        public ContentResult(string filename, string contentType)
        {
            this._data = ReadFile(filename);
            this._contentType = contentType;
        }

        public ContentResult(byte[] data, string contentType)
        {
            this._data = data;
            this._contentType = contentType;
        }

        private byte[] ReadFile(string filename)
        {
            return System.IO.File.ReadAllBytes(System.IO.Path.Combine(API.env.WebRootPath, filename));
        }

        public override void Execute(HttpContext context)
        {
            HttpResponse response = context.Response;
            response.ContentType = _contentType;
            if (_cachedSerializedResult != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                response.Body.Write(_data, 0, _data.Length);
            }
        }

        public override bool IsExecutionReady => _data != null;
    }
}
