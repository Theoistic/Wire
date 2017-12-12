using System;
using System.Collections.Generic;
using System.Text;

namespace Wire
{
    class AcceptHeaderAttribute : Attribute
    {
        public string Header { get; set; }
    }
}
