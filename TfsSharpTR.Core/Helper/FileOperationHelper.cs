using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;

namespace TfsSharpTR.Core.Helper
{
    

    public static class FileOperationHelper
    {
        public static string TakeBackup(AutoDeploySettingItem setting, string sourceFolder, string backupFolder)
        {
            string nBckupFolder = setting.GetBackupFolder(sourceFolder);

            var err = FileHelper.DirectoryCopy(sourceFolder, nBckupFolder, true);

            return err;
        }
    }
}
