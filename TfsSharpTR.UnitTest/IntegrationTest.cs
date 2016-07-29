using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsSharpTR.Core;
using System.Reflection;
using TfsSharpTR.Core.Model;
using System.Threading;
using System.Collections.Generic;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.PreBuild.FileControl;
using TfsSharpTR.PreBuild;
using System.Linq;
using TfsSharpTR.StyleCopRelated;
using TfsSharpTR.AutoDeploy;
using TfsSharpTR.Roslyn.Enforcer;
using TfsSharpTR.Roslyn.Metrics;
using TfsSharpTR.Roslyn.PartialUnitTest;

namespace TfsSharpTR.UnitTest
{
    /// <summary>
    /// Actually this class is integration test. These are all for debugging.
    /// </summary>
    [TestClass]
    public class IntegrationTest
    {
        private static Dictionary<string, string> DummyTfsVariable()
        {
            var fldr = Environment.CurrentDirectory + "\\";
            var dict = new Dictionary<string, string>();
            dict.Add("AGENT_WORKFOLDER", fldr);
            dict.Add("SYSTEM_TEAMPROJECT", "");
            dict.Add("BUILD_REQUESTEDFOR", "");
            dict.Add("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "");
            dict.Add("BUILD_SOURCEVERSION", "");
            dict.Add("BUILD_REPOSITORY_NAME", "");
            dict.Add("BUILD_REPOSITORY_PROVIDER", "");
            dict.Add("BUILD_BINARIESDIRECTORY", @"");
            dict.Add("BUILD_SOURCESDIRECTORY", @"C:\Kod\Test2015AutoDeploy");
            dict.Add("BUILD_REPOSITORY_LOCALPATH", @"");

            return dict;
        }

        private static Dictionary<string, string> DummyUserVariable(string actionName = "PreBuild")
        {
            string jsnSettingFile = "TestBuildSetting.json";
            var fldr = Environment.CurrentDirectory + "\\";
            var dict = new Dictionary<string, string>();
            dict.Add("SettingFile", fldr + jsnSettingFile);
            dict.Add("Action", actionName);

            return dict;
        }

        [TestMethod]
        public void FirstCall()
        {
            var dictTfs = DummyTfsVariable();
            var dictUsr = DummyUserVariable();

            var datas = InitialLoader.Get(dictTfs, dictUsr);

            Assert.IsTrue(datas.Count > 0);
        }

        [TestMethod]
        public void SecondCall()
        {
            var dictTfs = DummyTfsVariable();
            var dictUsr = DummyUserVariable("PostBuild");
            var datas = InitialLoader.Get(dictTfs, dictUsr);
            var tskTest = datas.Find(x => x.ClassName.EndsWith("TestTask"));
            if ((datas == null) || (datas.Count == 0) || (tskTest == null))
            {
                Assert.IsTrue(true, "No task found to run.");
                return;
            }

            var oPrms = new object[]
            {
                tskTest.DLLName,
                tskTest.ClassName,
                tskTest.MethodName,
                dictTfs,
                dictUsr
            };

            var assembly = Assembly.LoadFile(tskTest.DLLName);
            var typ = assembly.GetType(tskTest.ClassName);
            MethodInfo Method = typ.GetMethod(tskTest.MethodName);
            object myInstance = Activator.CreateInstance(typ);
            var rslt = Method.Invoke(myInstance, oPrms) as ShellStatu;

            //Thread.Sleep(5000 * 150 + 10000 + 10);
            Thread.Sleep(10000);

            Assert.IsTrue(rslt.IsSuccess);
        }

        [TestMethod]
        public void TfsHelperTest()
        {
            var dictTfs = DummyTfsVariable();
            var tfs = new TfsVariable(dictTfs);
            TFSHelper.Initialize(tfs);
            var usrGroups = TFSHelper.GroupUserJoined();

            Assert.IsTrue(usrGroups.Count > 0);
        }

        private ShellStatu RunTask(Func<string, string, string, Dictionary<string, string>, Dictionary<string, string>, ShellStatu> tsk)
        {
            var dictTfs = DummyTfsVariable();
            var dictUsr = DummyUserVariable();
            var tfs = new TfsVariable(dictTfs);
            var usr = new UserVariable<FileControlSetting>(dictUsr);

            return tsk("xFile", "xClass", "xMethod", dictTfs, dictUsr);
        }

        [TestMethod]
        public void TestTask()
        {
            var tsk = new TestTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void TestAsyncTask()
        {
            var tsk = new TestAsyncTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void FileControlTask()
        {
            var tsk = new FileControlTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void StyleCopTask()
        {
            var tsk = new StyleCopTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void AutoDeployTask()
        {
            var tsk = new AutoDeployTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void AnalyzerEnforcerTask()
        {
            var tsk = new AnalyzerEnforcerTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void CodeMetricTask()
        {
            var tsk = new CodeMetricTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }

        [TestMethod]
        public void PartialUnitTestTask()
        {
            var tsk = new PartialUnitTestTask();
            var rslt = RunTask(tsk.Initializer);

            var rMsg = rslt.Msgs.Any() ? rslt.Msgs[0] : "No Message";
            Assert.IsTrue(rslt.IsSuccess, rMsg);
        }
    }
}
