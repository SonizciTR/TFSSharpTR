using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Helper
{
    public static class SolutionHelper
    {
        public static string[] FindSolutionFiles(string buildPath, string filter = "*.sln")
        {
            return Directory.GetFiles(buildPath, filter, SearchOption.AllDirectories);
        }
    }
}
