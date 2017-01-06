using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.PreBuild.FileControl
{
    public class FileControlTask : BaseTask<FileControlSetting>
    {
        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<FileControlSetting> usrVariables)
        {
            var setting = usrVariables.Config;
            if (setting == null)
                return new TaskStatu("No setting found.");

            var allFiles = TFSHelper.ChangedFiles();
            if (!allFiles.Any())
                return new TaskStatu("No changed file found.");

            string adName = TFSHelper.UserDomainName;
            var usrGroups = TFSHelper.GroupUserJoined();

            foreach (var sourceData in allFiles)
            {
                var sourceFile = sourceData.FilePath;
                var rule = setting.Files.FirstOrDefault(x => x.FileNames.Any(y => sourceFile.EndsWith(y)));

                if (rule == null)
                    continue;

                if (rule.AllowedUser.Any(x => x == adName))
                    continue;

                if (rule.AllowedGroup.Any(x => (usrGroups.Any(y => y.EndsWith(x)))))
                    continue;

                return new TaskStatu("FC01", string.Format("[{0}] file is restirected for [{1}] user.", sourceFile, adName));
            }

            return new TaskStatu(string.Format("All changes controlled successfully. FileCount/TotalRule = {0}/{1}.", allFiles.Count, setting.Files.Count));
        }
    }
}
