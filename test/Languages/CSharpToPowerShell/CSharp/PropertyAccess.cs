using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.Test.Languages.CSharp
{
    public class PropertyAccess
    {
        public void Method()
        {
            var timeZoneInfo = new TimeZoneInfo();
            var variable = timeZoneInfo.DisplayName;
        }
    }
}
