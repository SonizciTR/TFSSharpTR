using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core
{
    /// <summary>
    /// Every task that want to trigger should be inhereted from this class
    /// </summary>
    /// <typeparam name="Tsetting">Your task setting model shoul be inhereted from BuildSetting</typeparam>
    public abstract class BaseTask<Tsetting> where Tsetting : BaseBuildSetting, new()
    {
        private Tsetting SettingforTask { get; set; }

        private List<string> intrDetailContainer = new List<string>();

        /// <summary>
        /// This is where shell script trigger. A cocoon ;)
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="tfsVarRaw"></param>
        /// <param name="usrVarRaw"></param>
        /// <returns></returns>
        public ShellStatu Initializer(string dllName, string className, string methodName, Dictionary<string, string> tfsVarRaw, Dictionary<string, string> usrVarRaw)
        {
            Stopwatch watchGeneral = Stopwatch.StartNew();
            string taskLongName = string.Empty;

            try
            {
                taskLongName = Path.GetFileNameWithoutExtension(dllName) + " -> " + className;
                var tfsVar = new TfsVariable(tfsVarRaw);
                var usrVar = new UserVariable<Tsetting>(usrVarRaw);
                Logger.Set(usrVar.WorkingPath);

                var bsSetting = usrVar.SettingFileData;
                if (bsSetting == null)
                    return new ShellStatu(false, string.Format("{0}.{1} tasks setting could not deserialized.", className, methodName));

                TFSHelper.Initialize(tfsVar);
                
                Stopwatch watchTask = Stopwatch.StartNew();
                var tmpResult = Job(tfsVar, usrVar);
                watchTask.Stop();

                if (intrDetailContainer.Any() && !tmpResult.Detail.Any())
                {
                    tmpResult.Detail = intrDetailContainer;
                }

                var rtnData = tmpResult == null ? new ShellStatu() : tmpResult.ToShellStatu(taskLongName);
                watchGeneral.Stop();
                rtnData.Msgs.Add(string.Format("*** FINISH > [{0}] task runned/total (ms) : {1}/{2}", 
                                    className, watchTask.Elapsed.TotalMilliseconds, watchGeneral.Elapsed.TotalMilliseconds));

                rtnData.Msgs.Add("---------------------------------------");

                return rtnData;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return new ShellStatu(false, string.Format("!!! BaseTask.Initializer failed. TaskName = [{0}]. ExDetail = [{1}].", taskLongName, ex));
            }
        }

        /// <summary>
        /// This is where developed real job
        /// </summary>
        /// <param name="tfsVariables"></param>
        /// <param name="usrVariables"></param>
        /// <returns></returns>
        public abstract TaskStatu Job(TfsVariable tfsVariables, UserVariable<Tsetting> usrVariables);

        /// <summary>
        /// Use this method to add detail messages. If you add directly to response, this will be ignored.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="watch"></param>
        /// <returns></returns>
        public bool WriteDetail(string message, Stopwatch watch = null)
        {
            string msg = string.Empty;
            string tmpMsg = ReFormatMessage(message);
            if (watch != null)
            {
                watch.Stop();
                msg = string.Format(" - {0} => {1} Total Time (ms) : {2}", DateTime.Now, tmpMsg, watch.Elapsed.TotalMilliseconds);
            }
            else
            {
                msg = string.Format(" - {0} => {1}", DateTime.Now, tmpMsg);
            }

            intrDetailContainer.Add(msg);

            return true;
        }

        private static readonly string[] lineEndings = new[] { ".", "!", "...", ".", "=>" };

        private static string ReFormatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return "";
            if (lineEndings.Any(x => message.EndsWith(x) ))
                return message;

            return message.Trim() + ".";
        }
    }
}
