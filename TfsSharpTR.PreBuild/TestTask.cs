using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.PreBuild
{
    /// <summary>
    /// This is just a POC (proof of concept) class. 
    /// </summary>
    public class TestTask : BaseTask<BaseBuildSetting>
    {
        private string folderPath;
        private const int waitTimeMs = 5000;

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<BaseBuildSetting> usrVariables)
        {
            Stopwatch tmrWatch = Stopwatch.StartNew();
            TaskStatu tsk = new TaskStatu("TestTask start successfully");
            
            DisplayAllTfsVariables(tfsVariables);

            WriteDetail("This how to use detail method :)", tmrWatch);
            return tsk;
        }

        private void DisplayAllTfsVariables(TfsVariable tfsVariables)
        {
            foreach (var item in tfsVariables.RawData)
            {
                WriteDetail(item.Key + " - " + item.Value);
            }
        }
    }
}
