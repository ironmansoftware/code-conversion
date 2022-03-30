using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.Test.Languages.CSharp
{
    public class Class
    {
        public void Method()
        {
            var eq = 1 == 2;
            var notEq = 1 != 2;
            var or = 1 == 2 || 2 == 1;
            var and = 1 == 2 && 2 == 1;
            var gt = 1 > 2;
            var lt = 1 < 2;
            var ge = 1 >= 2;
            var le = 1 <= 2;
            var plus = 1 + 1;
            var minus = 1 - 1;
            var bor = 1 | 1;
        }
    }
}
