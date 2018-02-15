using System;
using System.Collections.Generic;
using System.Reflection;
using Wire;
using System.Linq;

namespace Wire.Admin.Gen
{
    public class AdminEntityAttribute : Attribute { }

    [APIModule]
    public class AdminModule : WiredModule
    {
        public AdminModule()
        {
            List<Type> types = typeof(AdminEntityAttribute).Assembly.GetAllTypesWithAttribute<AdminEntityAttribute>().ToList();

            foreach (var t in types)
            {
                GET($"/{t.Name}", x =>
                {
                    return new { description = t.FullName };
                });
            }
        }
    }
}
