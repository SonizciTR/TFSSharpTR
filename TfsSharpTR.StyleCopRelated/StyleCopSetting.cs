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
        public int MaxErrorCount { get; set; }
        public bool TreatWarnAsError { get; set; }
        public string SettingFile { get; set; }

    }
}
