﻿using Newtonsoft.Json;
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
    public abstract class BaseTask<Tsetting> where Tsetting : BaseBuildSetting
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
            try
            {
                Stopwatch watchGeneral = Stopwatch.StartNew();
                Logger.Set(tfsVarRaw);

                var tfsVar = new TfsVariable(tfsVarRaw);
                var usrVar = new UserVariable<Tsetting>(usrVarRaw);
                var bsSetting = usrVar.SettingFileData;

                TFSHelper.Initialize(tfsVar);

                if (bsSetting == null)
                    throw new Exception(string.Format("{0}.{1} tasks setting could not deserialized.", className, methodName));

                Stopwatch watchTask = Stopwatch.StartNew();
                var tmpResult = Job(tfsVar, usrVar);
                watchTask.Stop();

                if (intrDetailContainer.Any() && !tmpResult.Detail.Any())
                {
                    tmpResult.Detail = intrDetailContainer;
                }

                var rtnData = tmpResult.ToShellStatu(Path.GetFileNameWithoutExtension(dllName) + " -> " + className);
                watchGeneral.Stop();
                rtnData.Msgs.Add(string.Format("*** FINISH > [{0}] task runned/total (ms) : {1}/{2}", 
                                    className, watchTask.Elapsed.TotalMilliseconds, watchGeneral.Elapsed.TotalMilliseconds));

                return rtnData;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                throw;
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
            if (watch != null)
            {
                watch.Stop();
                msg = string.Format("{0} => {1}. Total Time (ms) : {2}", DateTime.Now, message, watch.Elapsed.TotalMilliseconds);
            }
            else
            {
                msg = string.Format("{0} => {1}.", DateTime.Now, message);
            }

            intrDetailContainer.Add(msg);

            return true;
        }
    }
}
