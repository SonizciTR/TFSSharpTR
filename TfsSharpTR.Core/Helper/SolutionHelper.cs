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
        public static string[] FindSolutionFiles(string buildPath)
        {
            return Directory.GetFiles(buildPath, "*.sln", SearchOption.AllDirectories);
        }
    }
}
