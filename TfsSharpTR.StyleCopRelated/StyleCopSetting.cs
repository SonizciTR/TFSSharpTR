using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.StyleCopRelated
{  
    public class StyleCopSetting : BaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "StyleCopTask";
            }
        }

        public List<string> ExcludedFiles { get; set; }

        /// <summary>
        /// Acceptable max error count
        /// </summary>
        public int MaxErrorCount { get; set; }
        public bool TreatWarnAsError { get; set; }
        public string SettingFile { get; set; }

        /// <summary>
        /// Maximum number of error displayed in the build screen
        /// </summary>
        public int MaxLogCount { get; set; }

        private const int displayErrorCount = 50;
        public int MaxLogCounttoDisplay
        {
            get
            {
                var tmp = MaxErrorCount > MaxLogCount ? MaxErrorCount : MaxLogCount;

                return tmp == 0 ? displayErrorCount : tmp;
            }
        }
    }
}
