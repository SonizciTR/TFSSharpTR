using StyleCop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.StyleCopRelated
{
    public class StyleCopTask : BaseTask<StyleCopSetting>
    {
        private object _lckObj = new object();
        public List<string> violateError { get; set; } = new List<string>();
        public List<string> violateWarn { get; set; } = new List<string>();

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<StyleCopSetting> usrVariables)
        {
            bool isExclusionExist = (usrVariables.SettingFileData != null) || (usrVariables.SettingFileData.ExcludedFiles != null);
            string sourceCodePath = "";
            WriteDetail("Source Folder : " + sourceCodePath);
            WriteDetail("Exclusion files : " +
                (isExclusionExist ? string.Join(", ", usrVariables.SettingFileData.ExcludedFiles) : "None"));

            var srcFilesAll = Directory.GetFiles(sourceCodePath, "*.cs", SearchOption.AllDirectories).ToList();
            List<string> srcFilestoCheck;
            if (isExclusionExist)
                srcFilestoCheck = FilterFiles(srcFilesAll, usrVariables.SettingFileData?.ExcludedFiles);
            else
                srcFilestoCheck = srcFilesAll;
            WriteDetail(string.Format("File Count (All/Check) : {0}/{1}", srcFilesAll.Count, srcFilestoCheck.Count));

            bool isRunOk = RunStyleCopRules(srcFilestoCheck, usrVariables);

            return isRunOk ? new TaskStatu("StyleCopTask finished successfully") : new TaskStatu("SCT01", "StyleCopTask failed.");
        }

        private bool RunStyleCopRules(List<string> srcFilestoCheck, UserVariable<StyleCopSetting> usrVariables)
        {
            // Create the StyleCop console. But do not initialise the addins as this can cause modal dialogs to be shown on errors
            var console = new StyleCopConsole(usrVariables.SettingFileData.SettingFile, false, "StyleCopXmlOutputFile.xml", null, false);

            // make sure the UI is not dispayed on error
            console.Core.DisplayUI = false;

            // declare the add-ins to load
            console.Core.Initialize(null, true);

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
            finally
            {
                // Unsubscribe from events
                console.OutputGenerated -= this.OnOutputGenerated;
                console.ViolationEncountered -= this.OnViolationEncountered;
            }

            return violateError.Count > usrVariables.SettingFileData.MaxErrorCount;
        }

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

            lock (this)
            {
                WriteDetail(string.Format("{0} [{1}] Line {2}", mRule, file, e.LineNumber));
            }
        }

        private void OnOutputGenerated(object sender, OutputEventArgs e)
        {
            //Is it really necessery???
            //lock (_lckObj)
            //{
            //    WriteDetail(e.Output.Trim());
            //}
        }

        private List<string> FilterFiles(List<string> srcFilesAll, List<string> excludedFiles)
        {
            if ((excludedFiles == null) || (!excludedFiles.Any()))
                return srcFilesAll;

            var tmp = new List<string>();
            foreach (var itmFile in srcFilesAll)
            {
                bool isMatch = excludedFiles.Any(x => itmFile.EndsWith(itmFile));

                if (isMatch)
                    continue;

                tmp.Add(itmFile);
            }
            return tmp;
        }
    }
}
