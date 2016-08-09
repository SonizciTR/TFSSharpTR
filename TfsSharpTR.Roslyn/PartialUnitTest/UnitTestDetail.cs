using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    public class UnitTestDetail
    {
        public string MethodName { get; set; }
        public string AssemblyPath { get; set; }

        public UnitTestDetail()
        {
        }

        public UnitTestDetail(string mthdName, string assmblyPath)
        {
            MethodName = mthdName;
            AssemblyPath = assmblyPath;
        }
    }
}
