using System;

namespace Wire
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AuthAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class APIModuleAttribute : Attribute { }
}
