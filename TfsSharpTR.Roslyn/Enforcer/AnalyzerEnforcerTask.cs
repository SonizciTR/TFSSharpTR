﻿using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Model;
using TfsSharpTR.Roslyn.Enforcer;

namespace TfsSharpTR.Roslyn.Enforcer
{
    /// <summary>
    /// This task checks cs projs all analyzer references. If reference is missing, it adds to csproj.
    /// With this you can add your custom nuget analysis rules if it was not added.
    /// Your build processes will run rules by this way.
    /// </summary>
    public class AnalyzerEnforcerTask : BaseTask<AnalyzerEnforcerSetting>
    {
        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<AnalyzerEnforcerSetting> usrVariables)
        {
            string settingOkMsg = CheckSetting(usrVariables);
            var setting = usrVariables.SettingFileData;
            if (!string.IsNullOrEmpty(settingOkMsg))
                return new TaskStatu("RET01", settingOkMsg);
            string buildPath = tfsVariables.BuildSourceDirectory;
            string[] slnFiles = FindSolutionFiles(buildPath);
            if ((slnFiles == null) || (!slnFiles.Any()))
                return new TaskStatu("RET02", "No solution file found.");

            var workspace = MSBuildWorkspace.Create();

            foreach (var slnItem in slnFiles)
            {
                var solution = workspace.OpenSolutionAsync(slnItem).Result;
                
                foreach (var libItem in setting.References)
                {
                    AnalyzerReference analyzer = new AnalyzerFileReference(libItem.DLLPath, FromFileLoader.Instance);

                    foreach (var project in solution.Projects)
                    {
                        bool analyzerExists = false;
                        foreach (var analyzerReference in project.AnalyzerReferences)
                        {
                            if (analyzerReference.FullPath.EndsWith(libItem.DLLName))
                            {
                                analyzerExists = true;
                                break;
                            }
                        }

                        if (!analyzerExists)
                        {
                            WriteDetail(string.Format("{0} project is missing {1} library. Adding...", project.Name, libItem.DLLName));
                            solution = solution.AddAnalyzerReference(project.Id, analyzer);
                        }
                    }
                }

                workspace.TryApplyChanges(solution);
            }

            return new TaskStatu("Enforce finished successfully.");
        }

        private string CheckSetting(UserVariable<AnalyzerEnforcerSetting> usrVariables)
        {
            var setting = usrVariables?.SettingFileData;
            if (setting == null)
                return "Settings is missing.";

            if ((setting.References == null) || (!setting.References.Any()))
                return "No outside library found in setting.";

            foreach (var item in setting.References)
            {
                if (!File.Exists(item.DLLPath))
                    return string.Format("Could not find the \"{0}\" library from [{1}].", item.DLLName, item.DLLPath);
            }

            return null;
        }

        private string[] FindSolutionFiles(string buildPath)
        {
            return Directory.GetFiles(buildPath, "*.sln", SearchOption.AllDirectories);
        }
    }
}
