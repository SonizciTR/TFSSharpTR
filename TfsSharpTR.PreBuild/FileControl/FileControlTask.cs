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
            var setting = usrVariables.SettingFileData?.FileControlTask;
            if (setting == null)
                return new TaskStatu("No setting found.");

            var filesModified = TFSHelper.ChangedFiles();
            if(!filesModified.Any())
                return new TaskStatu("No changed file found.");

            string adName = TFSHelper.UserDomainName;
            var usrGroups = TFSHelper.GroupUserJoined();

            foreach (var file in filesModified)
            {
                var rule = setting.FirstOrDefault(x => file.EndsWith(x.FileName));

                if (rule == null)
                    continue;

                if (rule.AllowedUser.Any(x => x == adName))
                    continue;

                if (rule.AllowedGroup.Any(x => (usrGroups.Any(y => y.EndsWith(x)))))
                    continue;

                if (rule.RestrictedUser.Any(x => x == adName))
                    return new TaskStatu("FC01", string.Format("[{0}} file is restirected for [{1}] user.", file, adName) );

                var restrictedGrps = rule.RestrictedGroup.Intersect(usrGroups).ToList();
                if (restrictedGrps.Any())
                    return new TaskStatu("FC02", string.Format("[{0}} file is restirected for [{1}] group.", file, restrictedGrps[0]));
            }

            return new TaskStatu(string.Format("All changes controlled successfully. FileCount/TotalRule = {0}/{1}.", filesModified.Count, setting.Count));
        }
    }
}
