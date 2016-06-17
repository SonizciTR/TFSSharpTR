using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.AutoDeploy
{
    public class AutoDeploySetting : BaseBuildSetting
    {
        public AutoDeploySettingItem AutoDeployTask { get; set; }
    }

    public class AutoDeploySettingItem
    {
        public const string KeyDiff = "DIFF";
        public const string KeyAll = "ALL";

        /// <summary>
        /// Default is ALL. Possible combinations:
        /// DIFF : Synchronized deployment of only new files
        /// ALL : Synchronized deployment of all files
        /// </summary>
        public string Mode { get; set; } = "ALL";

        /// <summary>
        /// Files that do not want to deploy
        /// </summary>
        public List<string> ExcludedFiles { get; set; }

        /// <summary>
        /// Do you want to take backup before deployment
        /// </summary>
        public bool TakeBackup { get; set; }

        /// <summary>
        /// Backup copy folder
        /// </summary>
        public string BackupFolder { get; set; }

        /// <summary>
        /// Backup fodler naming. Default is [NameofSourceFolder]_[yyyy.MM.dd.HH.mm.ss]
        /// </summary>
        public string BackupPostFixNaming { get; set; } = "_yyyy.MM.dd.HH.mm.ss";

        /// <summary>
        /// Target folder for deployment
        /// </summary>
        public string DeployFolder { get; set; }

        /// <summary>
        /// Default is false. Task always try to stop AppPool. This flags control what happens after stop.
        /// If you set true, AppPool stop fails means deployment fails. Stops deployment.
        /// If you set false, even AppPool stop fails task will try to replace files and go on deployment.
        /// </summary>
        public bool MustStopIISAppPool { get; set; }

        /// <summary>
        /// What is the name of the AppPool to stop.
        /// </summary>
        public string AppPoolName { get; set; }

        /// <summary>
        /// What is the server name of the Deployment IIS to stop and start.
        /// </summary>
        public string IISServerName { get; set; }

        /// <summary>
        /// Set this for impersonation needed. User Name
        /// </summary>
        public string ImpersonateName { get; set; }

        /// <summary>
        /// Set this for impersonation needed. User Password
        /// </summary>
        public string ImpersonatePass { get; set; }

        /// <summary>
        /// Set this for impersonation needed. User Name
        /// </summary>
        public string ImpersonateDomain { get; set; }

        public string CheckRules()
        {
            if (string.IsNullOrEmpty(DeployFolder))
                return "DeployFolder can not be null.";

            if (TakeBackup && string.IsNullOrEmpty(BackupFolder))
                return "TakeBackup set true but BackupFolder is empty.";

            if (MustStopIISAppPool && string.IsNullOrEmpty(AppPoolName))
                return "MustStopIISAppPool set true but AppPoolName is empty.";

            bool isPrsCorrect = false;
            try
            {
                string tmp = BackupPostFix;
                isPrsCorrect = !string.IsNullOrEmpty(tmp);
            }
            catch { }
            if (!isPrsCorrect)
                return "BackupPostFixNaming is incorrect. Please select from DateTime ToString override options.";

            return null;
        }

        public string BackupPostFix
        {
            get
            {
                return DateTime.Now.ToString(BackupPostFixNaming);
            }
        }

        public string GetBackupFolder(string sourceFolder)
        {
            return BackupFolder + "\\" + Path.GetDirectoryName(sourceFolder) + BackupPostFix;
        }
    }
}
