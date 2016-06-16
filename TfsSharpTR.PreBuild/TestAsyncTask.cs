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
    public class TestAsyncTask : BaseTask<BaseBuildSetting>
    {
        private string folderPath;
        private const int waitTimeMs = 5000;

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<BaseBuildSetting> usrVariables)
        {
            Stopwatch tmrWatch = Stopwatch.StartNew();
            TaskStatu tsk = new TaskStatu("TestAsyncTask at begining.");

            folderPath = tfsVariables.AgentWorkFolder;
            var fMsg = CheckFileOp(folderPath);
            if (!string.IsNullOrEmpty(fMsg))
            {
                return new TaskStatu("T1", "CheckFileOp failed. Ex = " + fMsg);
            }

            try
            {
                var thr = new Thread(new ThreadStart(TestPostRun));
                thr.Start();
                tsk = new TaskStatu("TestAsyncTask started.");
            }
            catch (Exception ex)
            {
                tsk = new TaskStatu("T2", "TestAsyncTask.Job's ThreadStart start throw exception. Ex = " + ex.ToString());
            }
            
            return tsk;
        }

        

        private string CheckFileOp(string path)
        {
            try
            {
                WrtFile(path, DateTime.Now + " : Initial check.");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return null;
        }

        private bool WrtFile(string path, string msg)
        {
            string fullFile = path + "TfsSharpTaskRunnerTest.log";
            using (var fs = new FileStream(fullFile, FileMode.Append))
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

        private void TestPostRun()
        {
            Thread.Sleep(waitTimeMs);

            for (int i = 0; i < 150; i++)
            {
                WrtFile(folderPath, (i + 1) + ". runned. Time = " + DateTime.Now);
                Thread.Sleep(waitTimeMs);
            }

            WrtFile(folderPath, "TestPostRun finished. Yuppiii. Time = " + DateTime.Now);
        }
    }
}
