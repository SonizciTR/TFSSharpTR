using ArchiMetrics.Analysis.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;
using Microsoft.CodeAnalysis;
using ArchiMetrics.Analysis.Common.Metrics;
using ArchiMetrics.Analysis;

namespace TfsSharpTR.Roslyn.Metrics
{
    public class CodeMetricTask : BaseTask<CodeMetricSetting>
    {
        private int logCounter = 0;

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<CodeMetricSetting> usrVariables)
        {
            var setting = usrVariables.Data;
            if (setting == null)
                return new TaskStatu("CM01", "No setting found");

            string buildPath = tfsVariables.BuildSourceDirectory;
            var solutionsPath = string.IsNullOrEmpty(tfsVariables.SolutiontoBuild) ? 
                SolutionHelper.FindSolutionFiles(buildPath)
                : new string[] { tfsVariables.SolutiontoBuild };

            if (!solutionsPath.Any())
                return new TaskStatu("CM02", "No solution file found");

            foreach (var slnPath in solutionsPath)
            {
                if (setting.ExcludedSolutions.Any(x => x == slnPath))
                    continue;

                using (var slnProvider = new SolutionProvider())
                {
                    var solution = slnProvider.Get(slnPath).Result;
                    var slnName = Path.GetFileName(slnPath);
                    var projects = solution.Projects.ToList();
                    if(!projects.Any())
                        return new TaskStatu("CM03", string.Format("No solution found in [{0}] solution", slnName));
                    WriteDetail(string.Format("Calculating metrics for [{0}] solution", slnName));
                    var metricBag = GetSolutionMetric(projects, solution, setting);
                    if (metricBag == null)
                        return new TaskStatu("CM04", "CodeMetric failed due to an project build or style error.");

                    var checkResult = CheckMetric(metricBag, setting, slnName);
                    if (!string.IsNullOrEmpty(checkResult))
                        return new TaskStatu("CM05", checkResult);
                }
            }

            return new TaskStatu("CodeMetrics check finished successfully.");
        }

        private string CheckMetric(List<MetricProject> metricBag, CodeMetricSetting setting, string solutionName)
        {
            foreach (var itmGroup in metricBag)
            {   
                foreach (var itmSolution in itmGroup.Metric)
                {
                    string nameSln = itmSolution.Name;

                    CheckNameSpaceRules(itmSolution, setting, nameSln);
                    if (logCounter > setting.MaxLogCount)
                        return "Maximum log count reached.";

                    foreach (var itmClass in itmSolution.TypeMetrics)
                    {
                        string nameClass = nameSln + "." + itmClass.Name;

                        CheckClassRules(itmClass, setting, nameClass);

                        if (logCounter > setting.MaxLogCount)
                            return "Maximum log count reached.";

                        foreach (var itmMember in itmClass.MemberMetrics)
                        {
                            string nameMember = nameClass + "." + itmMember.Name;
                            CheckMemberRules(itmMember, setting, nameMember);
                        }

                        if (logCounter > setting.MaxLogCount)
                            return "Maximum log count reached.";
                    }
                }
                if (logCounter > setting.MaxLogCount)
                    return "Maximum log count reached.";
            }

            return logCounter > 0 ? string.Format("Metric check failed for  [{0}] solution", solutionName) : null;
        }

        private void CheckMemberRules(IMemberMetric itmMember, CodeMetricSetting setting, string nameMember)
        {
            if (IsMaxExceed(itmMember.CyclomaticComplexity, setting.MaxCyclomaticComplexityforMember))
                WrtLog(MetricSource.Member, "Cyclomatic Complexity", nameMember, itmMember.CyclomaticComplexity, setting.MaxCyclomaticComplexityforMember);

            if (IsMaxExceed(itmMember.LinesOfCode, setting.MaxLinesofCodeforMembers))
                WrtLog(MetricSource.Member, "Lines of Code", nameMember, itmMember.LinesOfCode, setting.MaxLinesofCodeforMembers);
            
            if (IsMinExceed(itmMember.MaintainabilityIndex, setting.MinMaintainabilityIndexforMember))
                WrtLog(MetricSource.Member, "Maintainability Index", nameMember, (int)itmMember.MaintainabilityIndex, setting.MinMaintainabilityIndexforMember);

            if (IsMaxExceed(itmMember.NumberOfParameters, setting.MaxNumberOfParameters))
                WrtLog(MetricSource.Member, "Number of Parameters", nameMember, itmMember.NumberOfParameters, setting.MaxNumberOfParameters);

            
        }

        private void CheckClassRules(ITypeMetric itmClass, CodeMetricSetting setting, string nameClass)
        {
            if (IsMaxExceed(itmClass.ClassCoupling, setting.MaxClassCouplingforClass))
                WrtLog(MetricSource.Class, "Class Coupling", nameClass, itmClass.ClassCoupling, setting.MaxClassCouplingforClass);

            if (IsMaxExceed(itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexityforClass))
                WrtLog(MetricSource.Class, "Cyclomatic Complexity", nameClass, itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexityforClass);

            if (IsMaxExceed(itmClass.DepthOfInheritance, setting.MaxDepthofInheritenceforClass))
                WrtLog(MetricSource.Class, "Depth of Inheritence", nameClass, itmClass.DepthOfInheritance, setting.MaxDepthofInheritenceforClass);

            if (IsMaxExceed(itmClass.EfferentCoupling, setting.MaxEfferentCoupling))
                WrtLog(MetricSource.Class, "Efferent Coupling", nameClass, itmClass.EfferentCoupling, setting.MaxEfferentCoupling);
            
            if (IsMaxExceed(itmClass.LinesOfCode, setting.MaxLinesofCodeforClass))
                WrtLog(MetricSource.Class, "Lines of Code", nameClass, itmClass.LinesOfCode, setting.MaxLinesofCodeforClass);

            int mainIndx = (int)itmClass.MaintainabilityIndex;
            if (IsMinExceed(mainIndx, setting.MinMaintainabilityIndexforClass))
                WrtLog(MetricSource.Class, "Maintainability Index", nameClass, mainIndx, setting.MinMaintainabilityIndexforClass);
        }

        private void CheckNameSpaceRules(INamespaceMetric itmClass, CodeMetricSetting setting, string nameClss)
        {
            //Cheking namespaces CyclomaticComplexity is seems ridiculous
            //if (IsMaxExceed(itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity))
            //    WrtLog("Cyclomatic Complexity", nameClss, itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity);

            //if (IsMaxExceed(itmClass.DepthOfInheritance, setting.MaxDepthofInheritenceforClass))
            //    WrtLog("Depth of Inheritance", nameClss, itmClass.DepthOfInheritance, setting.MaxDepthofInheritenceforClass);

            int mainIndx = (int)itmClass.MaintainabilityIndex;
            if (IsMinExceed(itmClass.MaintainabilityIndex, setting.MinMaintainabilityIndexforClass))
                WrtLog(MetricSource.Namespace, "Maintainability Index", nameClss, mainIndx, setting.MinMaintainabilityIndexforClass);
        }

        private bool IsMaxExceed(double metricValue, int maxValue)
        {
            int tmpMetric = (int)metricValue;
            return IsMaxExceed(tmpMetric, maxValue);
        }

        private bool IsMaxExceed(int metricValue, int maxValue)
        {
            return (maxValue > 0) && (metricValue > maxValue);
        }

        private bool IsMinExceed(double metricValue, int minValue)
        {
            int tmpMetric = (int)metricValue;
            return IsMinExceed(tmpMetric, minValue);
        }

        private bool IsMinExceed(int metricValue, int minValue)
        {
            return (minValue > 0) && (metricValue < minValue);
        }

        private bool WrtLog(MetricSource src, string ruleName, string source, int metricValue, int maxValue)
        {
            ++logCounter;
            return WriteDetail(string.Format("({4}) - {0} is exceed by [{1}]. Value/Limit is {2}/{3}", ruleName, source, metricValue, maxValue, src));
        }

        private List<MetricProject> GetSolutionMetric(List<Project> projects, Solution solution, CodeMetricSetting setting)
        {
            var metricCalculator = new CodeMetricsCalculator();
            var depo = new List<MetricProject>(projects.Count);

            foreach (var prj in projects)
            {
                if (setting.ExcludedProjects.Any(x => x == prj.Name))
                    continue;

                var tmpProject = new MetricProject();
                tmpProject.ProjectName = prj.Name;
                tmpProject.AssemblyName = prj.AssemblyName;
                try
                {
                    tmpProject.Metric = (metricCalculator.Calculate(prj, solution).Result).ToList();

                    depo.Add(tmpProject);
                }
                catch(Exception ex)
                {
                    string errMsg = string.Format("ERROR : [{0}] project has errors. Could not be processed. Check your project with SyleCop. Correct SA1603 error. ErrorDetail = {1}", prj.Name, ex.ToString());
                    WriteDetail(errMsg);
                    return null;
                }
            }

            return depo;
        }
    }
}
