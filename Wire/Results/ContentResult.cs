
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

        /*public ContentResult(string filename, string contentType)
        {
            this._data = ReadFile(filename);
            this._contentType = contentType;
        }*/

        public ContentResult(byte[] data, string contentType)
        {
            this._data = data;
            this._contentType = contentType;
        }

        /*private byte[] ReadFile(string filename)
        {
            return System.IO.File.ReadAllBytes(System.IO.Path.Combine(API.env.WebRootPath, filename));
        }*/

        public override void Execute(IContext context)
        {
            if (_cachedSerializedResult != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                context.WriteToResponse(_contentType, _data);
            }
        }

        public override bool IsExecutionReady => _data != null;
    }
}
