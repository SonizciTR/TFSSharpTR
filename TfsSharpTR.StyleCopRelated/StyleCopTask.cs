using StyleCop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.StyleCopRelated
{
    /// <summary>
    /// This code's StyleCop part looked from Richard Fennell's code. https://github.com/rfennell
    /// Thanks a lot Richard Fennell.
    /// https://github.com/rfennell/StyleCopCmdLine/blob/master/source/StyleCopWrapper/Wrapper.cs
    /// </summary>
    public class StyleCopTask : BaseTask<StyleCopSetting>
    {
        private object _lckObj = new object();
        private bool IsRunned = false;
        private StyleCopSetting GlobalSetting = null;

        public List<string> violateError { get; set; } = new List<string>();
        public List<string> violateWarn { get; set; } = new List<string>();
        public string SourceBaseFolder { get; set; }

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<StyleCopSetting> usrVariables)
        {
            GlobalSetting = usrVariables?.SettingFileData;
            SourceBaseFolder = tfsVariables.BuildSourceDirectory;
            if (GlobalSetting == null)
                return new TaskStatu("SCT02", "No setting loaded.");

            bool isExclusionExist = GlobalSetting.ExcludedFiles.Any();
            SourceBaseFolder = tfsVariables.BuildSourceDirectory;
            WriteDetail("Source Folder : " + SourceBaseFolder);
            WriteDetail("Exclusion files : " +
                (isExclusionExist ? string.Join(", ", GlobalSetting.ExcludedFiles) : "None"));

            var srcFilesAll = Directory.GetFiles(SourceBaseFolder, "*.cs", SearchOption.AllDirectories).ToList();
            List<string> srcFilestoCheck;
            if (isExclusionExist)
                srcFilestoCheck = FilterFiles(srcFilesAll, GlobalSetting.ExcludedFiles);
            else
                srcFilestoCheck = srcFilesAll;

            WriteDetail(string.Format("File Count (All/Check) : {0}/{1}", srcFilesAll.Count, srcFilestoCheck.Count));
            
            bool isRunOk = RunStyleCopRules(srcFilestoCheck, tfsVariables, usrVariables);
            if (!IsRunned)
                return new TaskStatu("SCT03", "StyleCop engine did not runned.");

            if (!isRunOk)
                return new TaskStatu("SCT01", "StyleCopTask failed.");

            return violateError.Count > GlobalSetting.MaxErrorCount ? new TaskStatu("SCT04", "There is too much error.") : new TaskStatu("StyleCopTask finished successfully");
        }

        private bool RunStyleCopRules(List<string> srcFilestoCheck, TfsVariable tfsVariables, UserVariable<StyleCopSetting> usrVariables)
        {
            List<string> addInPaths = new List<string> { usrVariables.WorkingPath + "\\" };
            string styleSettingFile = FindRuleFile(usrVariables);

            // Create the StyleCop console. But do not initialise the addins as this can cause modal dialogs to be shown on errors
            var console = new StyleCopConsole(styleSettingFile, false, "StyleCopXmlOutputFile.xml", null, false);

            // make sure the UI is not dispayed on error
            console.Core.DisplayUI = false;

            // declare the add-ins to load
            console.Core.Initialize(addInPaths, true);

            // Create the configuration.
            Configuration configuration = new Configuration(new string[0]);

            // Create a CodeProject object for these files. we use a time stamp for the key and the current directory for the cache location
            CodeProject project = new CodeProject(DateTime.Now.ToLongTimeString().GetHashCode(), @".\", configuration);

            foreach (var itmFile in srcFilestoCheck)
            {
                console.Core.Environment.AddSourceCode(project, itmFile, null);
            }

            try
            {
                // Subscribe to events
                console.OutputGenerated += this.OnOutputGenerated;
                console.ViolationEncountered += this.OnViolationEncountered;

                // Analyze the source files
                CodeProject[] projects = new[] { project };
                console.Start(projects, true);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                // Unsubscribe from events
                console.OutputGenerated -= this.OnOutputGenerated;
                console.ViolationEncountered -= this.OnViolationEncountered;
            }

            return violateError.Count > usrVariables.SettingFileData.MaxErrorCount;
        }

        private string FindRuleFile(UserVariable<StyleCopSetting> usrVariables)
        {
            string styleSettingFile = string.Empty;

            if (!string.IsNullOrEmpty(usrVariables.SettingFileData.SettingFile))
            {
                string onlyFileName = Path.GetFileName(usrVariables.SettingFileData.SettingFile);
                styleSettingFile = onlyFileName == usrVariables.SettingFileData.SettingFile ?
                    Path.Combine(usrVariables.WorkingPath, usrVariables.SettingFileData.SettingFile)
                    : usrVariables.SettingFileData.SettingFile;
                if (File.Exists(styleSettingFile))
                    WriteDetail("Custom StyleCop rule file = [" + styleSettingFile + "]");
                else
                {
                    styleSettingFile = string.Empty;
                    WriteDetail("Custom StyleCop rule assigned but not found in the folder = [" + styleSettingFile + "]");
                    WriteDetail("Default rules will be runned.");
                }
            }
            else
                WriteDetail("No Custom StyleCop rule file. Default rules will be runned.");

            return styleSettingFile;
        }

        private int counter = 0;
        private void OnViolationEncountered(object sender, ViolationEventArgs e)
        {
            string file = string.Empty;
            if (e.SourceCode != null && !string.IsNullOrEmpty(e.SourceCode.Path))
            {
                file = e.SourceCode.Path;
            }
            else if (e.Element != null &&
                e.Element.Document != null &&
                e.Element.Document.SourceCode != null &&
                e.Element.Document.SourceCode.Path != null)
            {
                file = e.Element.Document.SourceCode.Path;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(file);
            var lnNumber = string.Format(CultureInfo.CurrentUICulture, ". LineNumber: {0}, ", e.LineNumber.ToString(CultureInfo.CurrentCulture));
            sb.AppendLine(lnNumber);
            var chcId = string.Format(CultureInfo.CurrentUICulture, "CheckId: {0}, ", e.Violation.Rule.CheckId ?? string.Empty);
            sb.AppendLine(chcId);
            var msg = string.Format(CultureInfo.CurrentUICulture, "Message: {0}, ", e.Message);
            sb.AppendLine(msg);

            if (e.Warning)
                violateWarn.Add(file);
            else
                violateError.Add(file);

            // Prepend the rule check-id to the message.
            string mRule = string.Concat(e.Violation.Rule.CheckId ?? "NoRuleCheckId", ": ", e.Message);

            lock (_lckObj)
            {
                IsRunned = true;
                ++counter;
                if (counter < GlobalSetting.MaxLogCounttoDisplay)
                {
                    string relPath = FileHelper.RemoveBaseFolder(SourceBaseFolder, file);
                    WriteDetail(string.Format("{0} [{1}] Line {2}", mRule, relPath, e.LineNumber));
                }
                else if(counter == GlobalSetting.MaxLogCounttoDisplay)
                {
                    WriteDetail(string.Format("Maximum error logging is exceeded. Max is {0}", GlobalSetting.MaxLogCounttoDisplay));
                }
            }
        }

        private void OnOutputGenerated(object sender, OutputEventArgs e)
        {
            lock (_lckObj)
            {
                IsRunned = true;
                string msg = e.Output.Trim();
                if (!msg.StartsWith("Pass"))
                    WriteDetail(msg);
            }
        }

        private List<string> FilterFiles(List<string> srcFilesAll, List<string> excludedFiles)
        {
            if ((excludedFiles == null) || (!excludedFiles.Any()))
                return srcFilesAll;

            var tmpExcluded = excludedFiles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            var checkGroup = new List<string>();
            foreach (var itmFile in srcFilesAll)
            {
                bool isMatch = tmpExcluded.Any(x => itmFile.EndsWith(x));

                if (isMatch)
                    continue;

                checkGroup.Add(itmFile);
            }
            return checkGroup;
        }
    }
}
