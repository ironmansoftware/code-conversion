using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellToolsPro.CodeConversion.Test.Languages.CSharpToPowerShell5.CSharp
{
    public class NewClass : BaseClass, IBaseInterface
    {
        public NewClass(string test)
        {
            this.Process = test;
        }
        public string Test(string value)
        {
            this.Process = value;
            return "Hello";
        }

        public static void StuffSetter(string value)
        {
            NewClass.stuff = value;
        }

        public string Process { get; set; }
        private string Process2 { get; set; }
        private string test;
        static string stuff;
    }
}
