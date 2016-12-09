using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    public class PartialUnitTestSetting : BaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "PartialUnitTest";
            }
        }

        /// <summary>
        /// Max number of log line will be written to screen
        /// </summary>
        public int MaxLogCount { get; set; } = 50;

        /// <summary>
        /// Run Settings File for unit test. Optional.
        /// </summary>
        public string RunSettingFile { get; set; }

        /// <summary>
        /// Acceptable minimum code coverage percantage
        /// </summary>
        public int MinCoveragePerc { get; set; }
    }
}
