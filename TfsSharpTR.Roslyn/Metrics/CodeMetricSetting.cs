using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Roslyn.Metrics
{
    public class CodeMetricSetting : BaseBuildSetting
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
        /// Maximum allowed Coupling Complexity for Member
        /// </summary>
        public int MaxCouplingComplexityforMember { get; set; }

        /// <summary>
        /// Maximum allowed Coupling Complexity for Class
        /// </summary>
        public int MaxCouplingComplexityforClass { get; set; }

        /// <summary>
        /// Maximum allowed Class Coupling for Member
        /// </summary>
        public int MaxClassCouplingforMember { get; set; }

        /// <summary>
        /// Maximum allowed Class Coupling for Class
        /// </summary>
        public int MaxClassCouplingforClass { get; set; }

        /// <summary>
        /// Maximum allowed Cyclomatic Complexity for Member
        /// </summary>
        public int MaxCyclomaticComplexityforMember { get; set; }

        /// <summary>
        /// Maximum allowed Cyclomatic Complexity for Class
        /// </summary>
        public int MaxCyclomaticComplexityforClass { get; set; }

        /// <summary>
        /// Maximum allowed Depth of Inheritence for Member
        /// </summary>
        public int MaxDepthofInheritenceforMember { get; set; }

        /// <summary>
        /// Maximum allowed Depth of Inheritence for Class
        /// </summary>
        public int MaxDepthofInheritenceforClass { get; set; }

        /// <summary>
        /// Maximum allowed Lines of Code for Classes
        /// </summary>
        public int MaxLinesofCodeforClass { get; set; }

        /// <summary>
        /// Maximum allowed Lines of Code for Class Members
        /// </summary>
        public int MaxLinesofCodeforMembers { get; set; }

        /// <summary>
        /// Minimum allowed Maintainability Index for Member
        /// </summary>
        public int MinMaintainabilityIndexforMember { get; set; }

        /// <summary>
        /// Minimum allowed Maintainability Index for Class
        /// </summary>
        public int MinMaintainabilityIndexforClass { get; set; }

        /// <summary>
        /// Maximum allowed Efferent Coupling
        /// Efferent Coupling is the number of code elemnts that it uses
        /// </summary>
        public int MaxEfferentCoupling { get; set; }

        /// <summary>
        /// Maximum allowed number of parameters for a class member
        /// </summary>
        public int MaxNumberOfParameters { get; set; }

        /// <summary>
        /// Solutions that will not be analyzed
        /// </summary>
        public List<string> ExcludedSolutions { get; set; } = new List<string>();

        /// <summary>
        /// Projects that will not be analyzed
        /// </summary>
        public List<string> ExcludedProjects { get; set; } = new List<string>();
    }
}
