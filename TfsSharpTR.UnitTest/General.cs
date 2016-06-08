using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsSharpTR.Core;
using System.Reflection;
using TfsSharpTR.Core.Model;
using System.Threading;
using System.Collections.Generic;
using TfsSharpTR.Core.Helper;

namespace TfsSharpTR.UnitTest
{
    [TestClass]
    public class General
    {
        private static Dictionary<string, string> DummyTfsVariable()
        {
            var fldr = Environment.CurrentDirectory + "\\";
            var dict = new Dictionary<string, string>();
            dict.Add("AGENT_WorkFolder", fldr);
            dict.Add("SYSTEM_TEAMPROJECT", "");
            dict.Add("BUILD_REQUESTEDFOR", "");
            dict.Add("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "");
            return dict;
        }

        private static Dictionary<string, string> DummyUserVariable(string actionName = "PreBuild")
        {
            string jsnSettingFile = "TestBuildSetting.json";
            var fldr = Environment.CurrentDirectory + "\\";
            var dict = new Dictionary<string, string>();
            dict.Add("SettingFile", jsnSettingFile);
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
    }
}
