using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Roslyn.Enforcer
{
    public class AnalyzerEnforcerSetting : BaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "AnalyzerEnforcerTask";
            }
        }

        public List<ReferenceEnforcerItem> References { get; set; }
    }

    public class ReferenceEnforcerItem
    {
        /// <summary>
        /// DLL name to check at cs proj files
        /// </summary>
        public string DLLName { get; set; }

        /// <summary>
        /// If DLLName not exit in csproj file, DLL will be added from this path
        /// </summary>
        public string DLLPath { get; set; }

        /// <summary>
        /// This projects will exclude from adding process
        /// </summary>
        public List<string> ExcludedProjects { get; set; } = new List<string>();

        /// <summary>
        /// This solutions will not be checked
        /// </summary>
        public List<string> ExcludedSolutions { get; set; } = new List<string>();
    }
}
