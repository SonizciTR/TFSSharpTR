using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    public class UnitTestDetail
    {
        public string FilePath { get; set; }
        public string AssemblyName { get; set; }

        public UnitTestDetail()
        {
        }

        public UnitTestDetail(string file, string assemblyName)
        {
            FilePath = file;
            AssemblyName = assemblyName;
        }
    }
}
