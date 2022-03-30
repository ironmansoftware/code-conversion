using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter.Test.Languages.CSharp
{
    public class Class
    {
        void Method()
        {
            int i = 0;
            int x = 1;
            switch (i)
            {
                case 2:
                    x = 2;
                    break;
                case 3:
                    x = 3;
                    break;
                default:
                    break;
            }
        }
    }
}
