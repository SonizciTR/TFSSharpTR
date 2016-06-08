using System;
using System.Collections.Generic;
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
    }
}
