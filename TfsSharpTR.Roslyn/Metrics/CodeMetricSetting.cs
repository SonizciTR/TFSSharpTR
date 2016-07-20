using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Roslyn.Metrics
{
    public class CodeMetricSetting : IBaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "CodeMetric";
            }
        }

        /// <summary>
        /// Maximum error logged to screen. Check will be canceled above this number.
        /// </summary>
        public int MaxLogCount { get; set; }

        /// <summary>
        /// Maximum allowed Coupling Complexity
        /// </summary>
        public int MaxCouplingComplexity { get; set; }

        /// <summary>
        /// Maximum allowed Class Coupling
        /// </summary>
        public int MaxClassCoupling { get; set; }

        /// <summary>
        /// Maximum allowed Cyclomatic Complexity
        /// </summary>
        public int MaxCyclomaticComplexity { get; set; }

        /// <summary>
        /// Maximum allowed Depth of Inheritence
        /// </summary>
        public int MaxDepthofInheritence { get; set; }

        /// <summary>
        /// Maximum allowed Lines of Code for Classes
        /// </summary>
        public int MaxLinesofCodeforClass { get; set; }

        /// <summary>
        /// Maximum allowed Lines of Code for Class Members
        /// </summary>
        public int MaxLinesofCodeforMembers { get; set; }

        /// <summary>
        /// Minimum allowed Maintainability Index
        /// </summary>
        public int MinMaintainabilityIndex { get; set; }

        /// <summary>
        /// Maximum allowed Efferent Coupling
        /// Efferent Coupling is the number of code elemnts that it uses
        /// </summary>
        public int MaxEfferentCoupling { get; internal set; }

        /// <summary>
        /// Maximum allowed number of parameters for a class member
        /// </summary>
        public int MaxNumberOfParameters { get; internal set; }
    }
}
