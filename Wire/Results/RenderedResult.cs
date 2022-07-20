using System;
using System.IO;

namespace Wire
{
    public abstract class RenderedResult<T> : BaseResult
    {
        protected T _result;
        protected string _contentType = "text/html";
        private MemoryStream _cachedSerializedResult;
        protected abstract void Render(Stream s, T t);
        
        public override void Execute(Context context)
        {
            var response = context.Response;
            response.ContentType = _contentType;
            if (_cachedSerializedResult != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                Render(response.OutputStream, _result);
            }
        }

        public override bool IsExecutionReady => _result != null;
    }
}
