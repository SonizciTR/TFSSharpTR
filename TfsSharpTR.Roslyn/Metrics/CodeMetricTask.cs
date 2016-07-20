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
            var setting = usrVariables.SettingFileData;
            if (setting == null)
                return new TaskStatu("CM01", "No setting found.");

            string buildPath = tfsVariables.BuildSourceDirectory;
            var solutionsPath = SolutionHelper.FindSolutionFiles(buildPath);

            if (!solutionsPath.Any())
                return new TaskStatu("CM02", "No solution file found.");

            foreach (var slnPath in solutionsPath)
            {
                using (var slnProvider = new SolutionProvider())
                {
                    var solution = slnProvider.Get(slnPath).Result;
                    var slnName = Path.GetFileName(slnPath);
                    var projects = solution.Projects.ToList();
                    if(!projects.Any())
                        return new TaskStatu("CM03", string.Format("No solution found in [{0}] solution.", slnName));
                    WriteDetail(string.Format("Calculating metrics for [{0}] solution.", slnName));
                    var metricBag = GetSolutionMetric(projects, solution);

                    var checkResult = CheckMetric(metricBag, setting, slnName);
                    if (!string.IsNullOrEmpty(checkResult))
                        return new TaskStatu("CM04", checkResult);
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
            if (IsMaxExceed(itmMember.CyclomaticComplexity, setting.MaxCyclomaticComplexity))
                WrtLog("Cyclomatic Complexity", nameMember, itmMember.CyclomaticComplexity, setting.MaxCyclomaticComplexity);

            if (IsMaxExceed(itmMember.LinesOfCode, setting.MaxLinesofCodeforMembers))
                WrtLog("Lines of Code", nameMember, itmMember.LinesOfCode, setting.MaxLinesofCodeforMembers);
            
            if (IsMinExceed(itmMember.MaintainabilityIndex, setting.MinMaintainabilityIndex))
                WrtLog("Maintainability Index", nameMember, (int)itmMember.MaintainabilityIndex, setting.MinMaintainabilityIndex);

            if (IsMaxExceed(itmMember.NumberOfParameters, setting.MaxNumberOfParameters))
                WrtLog("Number of Parameters", nameMember, itmMember.NumberOfParameters, setting.MaxNumberOfParameters);
        }

        private void CheckClassRules(ITypeMetric itmClass, CodeMetricSetting setting, string nameClass)
        {
            if (IsMaxExceed(itmClass.ClassCoupling, setting.MaxClassCoupling))
                WrtLog("Class Coupling", nameClass, itmClass.ClassCoupling, setting.MaxClassCoupling);

            if (IsMaxExceed(itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity))
                WrtLog("Cyclomatic Complexity", nameClass, itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity);

            if (IsMaxExceed(itmClass.DepthOfInheritance, setting.MaxDepthofInheritence))
                WrtLog("Depth of Inheritence", nameClass, itmClass.DepthOfInheritance, setting.MaxDepthofInheritence);

            if (IsMaxExceed(itmClass.EfferentCoupling, setting.MaxEfferentCoupling))
                WrtLog("Efferent Coupling", nameClass, itmClass.EfferentCoupling, setting.MaxEfferentCoupling);
            
            if (IsMaxExceed(itmClass.LinesOfCode, setting.MaxLinesofCodeforClass))
                WrtLog("Lines of Code", nameClass, itmClass.LinesOfCode, setting.MaxLinesofCodeforClass);

            int mainIndx = (int)itmClass.MaintainabilityIndex;
            if (IsMinExceed(mainIndx, setting.MinMaintainabilityIndex))
                WrtLog("Maintainability Index", nameClass, mainIndx, setting.MinMaintainabilityIndex);
        }

        private void CheckNameSpaceRules(INamespaceMetric itmClass, CodeMetricSetting setting, string nameClss)
        {
            if (IsMaxExceed(itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity))
                WrtLog("Cyclomatic Complexity", nameClss, itmClass.CyclomaticComplexity, setting.MaxCyclomaticComplexity);

            if (IsMaxExceed(itmClass.DepthOfInheritance, setting.MaxDepthofInheritence))
                WrtLog("Depth of Inheritance", nameClss, itmClass.DepthOfInheritance, setting.MaxDepthofInheritence);

            int mainIndx = (int)itmClass.MaintainabilityIndex;
            if (IsMinExceed(itmClass.MaintainabilityIndex, setting.MinMaintainabilityIndex))
                WrtLog("Maintainability Index", nameClss, mainIndx, setting.MinMaintainabilityIndex);
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

        private bool WrtLog(string ruleName, string source, int metricValue, int maxValue)
        {
            return WrtLog(string.Format("{0} is exceed by {1}. Value / Max is {2} / {3}.", ruleName, source, metricValue, maxValue));
        }

        private bool WrtLog(string msg)
        {
            ++logCounter;
            return WriteDetail(msg);
        }

        private List<MetricProject> GetSolutionMetric(List<Project> projects, Solution solution)
        {
            var metricCalculator = new CodeMetricsCalculator();
            var depo = new List<MetricProject>(projects.Count);

            foreach (var prj in projects)
            {
                var tmpProject = new MetricProject();
                tmpProject.ProjectName = prj.Name;
                tmpProject.AssemblyName = prj.AssemblyName;
                tmpProject.Metric = (metricCalculator.Calculate(prj, solution).Result).ToList();
                
                depo.Add(tmpProject);
            }

            return depo;
        }
    }
}
