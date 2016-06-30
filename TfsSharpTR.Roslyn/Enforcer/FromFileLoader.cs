using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.Enforcer
{
    internal class FromFileLoader : IAnalyzerAssemblyLoader
    {
        public static FromFileLoader Instance = new FromFileLoader();

        public void AddDependencyLocation(string fullPath)
        {
        }

        public Assembly LoadFromPath(string fullPath)
        {
            return Assembly.LoadFrom(fullPath);
        }
    }
}
