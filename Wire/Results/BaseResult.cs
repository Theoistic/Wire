using System.Collections.Specialized;

namespace Wire
{
    public abstract class BaseResult
    {
        protected NameValueCollection ResultHeaders;
        public abstract void Execute(IContext context);
        public abstract bool IsExecutionReady { get; }
    }
}
