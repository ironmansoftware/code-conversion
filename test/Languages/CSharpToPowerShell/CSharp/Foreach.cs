using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.Test.Languages.CSharp
{
    public class Class
    {
        public void Method(string[] strings)
        {
            foreach(var item in strings)
            {
                var str = item;
                continue;
            }
        }
    }
}
