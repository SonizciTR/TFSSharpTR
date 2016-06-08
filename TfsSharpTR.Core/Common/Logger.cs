using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Common
{
    public static class Logger
    {
        //private const string LogFile = "SharpTaskRunner.log";
        private static string LogFile
        {
            get
            {
                return string.Format("SharpTaskRunner{0}.log", DateTime.Now.ToString("yyyyMMdd"));
            }
        }
        private static string LogPath = "";
        private static bool IsLoggingOk = false;

        
        private static string FullLogFile
        {
            get
            {
                if (string.IsNullOrEmpty(LogPath))
                {
                    return null;
                }

                return LogPath + LogFile;
            }
        }

        private static bool Initialize(string foldertoPutLogFile)
        {
            if (!Directory.Exists(foldertoPutLogFile))
                return false;

            LogPath = foldertoPutLogFile;

            IsLoggingOk = !string.IsNullOrEmpty(CheckFileOp(FullLogFile));
            return IsLoggingOk;
        }

        public static bool Set(Dictionary<string, string> tfsVariables)
        {
            try
            {
                if ((tfsVariables == null) || (!tfsVariables.Any()))
                    return false;

                string tmp = null;
                if (tfsVariables.TryGetValue("AGENT_WorkFolder", out tmp))
                {
                    return Initialize(tmp);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Logger.Set failed. Ex Detail = " + ex);
            }

            return false;
        }

        #region File Write

        public static bool Write(Exception ex,
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!IsLoggingOk)
                return false;

            try
            {
                string fMsg = string.Format("({0}) File : {1}. Method : {2}. Ex Detail = {3}", sourceLineNumber, sourceFilePath, memberName, ex);
                return WrtFile(FullLogFile, fMsg);
            }
            catch(Exception exWrtFile)
            {
                Debug.WriteLine("Logger internal hata : " + exWrtFile.ToString());
            }
            return false;
        }

        private static string CheckFileOp(string filePath)
        {
            try
            {
                WrtFile(filePath, DateTime.Now + " : Logger initial check.");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return null;
        }

        private static bool WrtFile(string filePath, string msg)
        {
            using (var fs = new FileStream(filePath, FileMode.Append))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                }
                fs.Close();
            }
            return true;
        }

        #endregion


    }
}
