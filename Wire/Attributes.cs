using System;
using System.Collections.Generic;
using System.Text;

namespace Wire
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AuthAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class APIModuleAttribute : Attribute { }
}
