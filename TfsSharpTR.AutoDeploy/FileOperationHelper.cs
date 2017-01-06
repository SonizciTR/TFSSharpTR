using System.Text.RegularExpressions;
using TfsSharpTR.Core.Helper;

namespace TfsSharpTR.AutoDeploy
{
    internal static class FileOperationHelper
    {
        public static string TakeBackup(AutoDeploySetting setting, string sourceFolder, string backupFolder)
        {
            string nBckupFolder = setting.GetBackupFolder(sourceFolder);

            var err = FileHelper.DirectoryCopy(sourceFolder, nBckupFolder, true);

            return err;
        }
    }
}
