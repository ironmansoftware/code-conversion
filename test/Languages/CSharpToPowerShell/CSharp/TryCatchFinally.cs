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
            try
            {
                var item = new object();
            }
            catch (Exception ex)
            {
                var item = new object();
            }
            catch
            {
                var item = new object();
            }
            finally
            {
                var item = new object();
            }
        }
    }
}
