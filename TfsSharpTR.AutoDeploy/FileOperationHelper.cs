using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.AutoDeploy
{
    internal static class FileOperationHelper
    {
        public static string TakeBackup(AutoDeploySettingItem setting, string sourceFolder, string backupFolder)
        {
            string nBckupFolder = setting.GetBackupFolder(sourceFolder);

            var err = DirectoryCopy(sourceFolder, nBckupFolder, true);

            return err;
        }

        public static string DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, overwrite);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }

            return null;
        }

        public static byte[] SafeFileRead(string file)
        {
            try
            {
                return File.ReadAllBytes(file);
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static List<string> SafeFileReadLines(string file)
        {
            try
            {
                return File.ReadAllLines(file).ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static string RemoveBaseFolder(string folderName, string fullFilePath)
        {
            string diffName = fullFilePath.TrimStart(folderName.ToCharArray());
            diffName = diffName.TrimStart('\\', '/');
            return diffName;
        }
    }
}
