using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.AutoDeploy
{
    static class Win32Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr pSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateDirectory(string lpPathName, SECURITY_ATTRIBUTES lpSecurityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);
    }

    internal static class FileOperationHelper
    {
        public static string TakeBackup(AutoDeploySettingItem setting, string sourceFolder, string backupFolder)
        {
            string nBckupFolder = setting.GetBackupFolder(sourceFolder);

            var err = DirectoryCopy(sourceFolder, nBckupFolder, true);

            return err;
        }

        public static bool CreateDirectory(string path, bool isRecursive = false)
        {
            if (!isRecursive)
                return Win32Native.CreateDirectory(String.Concat(@"\\?\", path), null);

            var spltd = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string tmp = spltd[0];
            bool isSucc = true;
            for (int i = 1; i < spltd.Length; i++)
            {
                tmp += ("\\" + spltd[i]);
                if (!Directory.Exists(tmp))
                {
                    isSucc &= CreateDirectory(tmp);
                    if (!isSucc)
                        break;
                }
            }
            return isSucc;
        }

        public static string DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = true)
        {
            try
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
                    //Directory.CreateDirectory(destDirName);
                    var isCrtd = CreateDirectory(destDirName);
                    if (!isCrtd)
                        return "Directory could not created. Path = " + destDirName;
                }


                // Get the file contents of the directory to copy.
                FileInfo[] files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    // Create the path to the new copy of the file.
                    string temppath = Path.Combine(destDirName, file.Name);

                    // Copy the file. Long file path problem
                    //file.CopyTo(temppath, overwrite);
                    Win32Native.CopyFile(file.FullName, temppath, !overwrite);
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
            }
            catch (Exception ex)
            {
                return "FileOperationHelper.DirectoryCopy failed. ErrDetail = " + ex.ToString();
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
