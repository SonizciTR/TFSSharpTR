using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    /// <summary>
    /// Build settings, from Json file
    /// </summary>
    public class RawBasicBuildSetting
    {
        internal const string KeyBaseConfigArea = "Base";

        /// <summary>
        /// Which tasks will run at PreBuild
        /// </summary>
        public List<string> PreBuildTasks { get; set; }

        /// <summary>
        /// Which tasks will run at PostBuild
        /// </summary>
        public List<string> PostBuildTasks { get; set; }
    }
}
