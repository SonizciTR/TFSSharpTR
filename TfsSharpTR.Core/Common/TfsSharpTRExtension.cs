using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Common
{
    internal static class TfsSharpTRExtension
    {
        public static ShellStatu ToShellStatu(this TaskStatu source, string taskName)
        {
            var target = new ShellStatu();

            target.IsSuccess = source.IsSuccess;
            var sb = new StringBuilder();
            string msgGeneral = "[No Msg]";
            if(target.IsSuccess)
            {
                msgGeneral = string.Format("[{0}] runned successfully. [{1}]", taskName, source.GeneralMsg);
            }
            else
            {
                msgGeneral = string.Format("[{0}] could not run. Failed. Error Code = [{1}]. Error Msg = [{2}].", taskName, source.Code, source.GeneralMsg);
            }
            sb.AppendLine(msgGeneral);

            if (source.Detail.Any())
            {
                sb.AppendLine("---> Details : ");
                foreach (var item in source.Detail)
                {
                    sb.AppendLine(item);
                }
            }
            sb.AppendLine("************************************************");

            return target;
        }

        /// <summary>
        /// Get all files from folder by extensions
        /// </summary>
        /// <param name="dir">DirectoryInfo type folder info</param>
        /// <param name="extensions">Like .exe, .cs,...</param>
        /// <returns>FileInfo of finded files</returns>
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("Extensions is missng.");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }
    }
}
