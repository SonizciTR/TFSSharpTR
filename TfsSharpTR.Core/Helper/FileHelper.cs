using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;

namespace TfsSharpTR.Core.Helper
{
    public static class FileHelper
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

        public static bool CreateDirectory(string path, bool isRecursive = false)
        {
            if (!isRecursive)
                return Win32Native.CreateDirectory(string.Concat(@"\\?\", path), null);

            var tmpDir = new DirectoryInfo(path);
            if(Directory.Exists(tmpDir.Parent.FullName))
            {
                return Win32Native.CreateDirectory(string.Concat(@"\\?\", path), null);
            }
            else
            {
                return CreateDirectory(tmpDir.Parent.FullName);
            }
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
                Logger.Write(ex);

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
                Logger.Write(ex);
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
                Logger.Write(ex);
            }
            return null;
        }

        public static string RemoveBaseFolder(string folderName, string fullFilePath)
        {
            string diffName = fullFilePath.Replace(folderName, "").TrimStart('/', '\\');
            diffName = diffName.TrimStart('\\', '/');
            return diffName;
        }

        /// <summary>
        /// Try to find with asteriks(*) charater styled pattern.
        /// 
        /// </summary>
        /// <param name="sourceFullPath">example :> C:\Folder1\Folder2\....\1.txt</param>
        /// <param name="patternToLook">exmple :> *.txt</param>
        /// <returns>Is pattern found in source path</returns>
        public static bool IsFilePatternExist(string sourceFullPath, string patternToLook)
        {
            var reg = new Regex(WildcardToRegex(patternToLook), RegexOptions.IgnoreCase);
            return reg.IsMatch(sourceFullPath);
        }

        private static string WildcardToRegex(string pattern)
        {
            return pattern.Replace(@"\", @"\").Replace("*", @"\S+").Replace(".", @"\.");
            //return pattern.Replace("*", ".*").Replace("?", ".").Replace(".", "\\.");
            //return pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".");
        }
    }
}
