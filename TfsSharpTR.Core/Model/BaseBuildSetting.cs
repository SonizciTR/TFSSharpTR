using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public abstract class IBaseBuildSetting
    {
        public abstract string SettingFileAreaName { get; }
    }

    /// <summary>
    /// Build settings, from Json file
    /// </summary>
    public class BaseBuildSetting : IBaseBuildSetting
    {
        public const string KeyBaseConfigArea = "Base";
        public override string SettingFileAreaName
        {
            get
            {
                return KeyBaseConfigArea;
            }
        }

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
