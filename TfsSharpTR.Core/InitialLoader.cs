using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;
using System.Reflection;
using TfsSharpTR.Core.Common;
using Newtonsoft.Json;

namespace TfsSharpTR.Core
{
    public static class InitialLoader
    {
        public static TaskList Get(Dictionary<string, string> tfsVariables, Dictionary<string, string> usrVar)
        {
            try
            {
                Logger.Set(tfsVariables);

                var settTfs = new TfsVariable(tfsVariables);
                var settUsr = new UserVariable<BaseBuildSetting>(usrVar);

                if (settUsr.SettingFileData == null)
                    throw new Exception("User Setting File is missing or wrongly formatted.");

                var dlls = GetDllList(settTfs.AgentWorkFolder);
                var allTasks = GetAllTasks(dlls, settUsr);
                return allTasks;
            }
            catch(Exception ex)
            {
                Logger.Write(ex);
                throw;
            }
        }

        private static TaskList FilterTasks(TaskList allTasks, BaseBuildSetting setting)
        {
            throw new NotImplementedException();
        }

        private static TaskList GetAllTasks(List<string> dllFiles, UserVariable<BaseBuildSetting> setting)
        {
            var tasks = new TaskList();

            foreach (var dll in dllFiles)
            {
                var assmbly = Assembly.LoadFile(dll);
                var fileTyps = assmbly.GetTypes();
                var typs = (from System.Type type in fileTyps
                            where
                            (type.BaseType != null)  && type.BaseType.IsGenericType 
                            && type.BaseType.GetGenericTypeDefinition() == typeof(BaseTask<>)
                            select type).ToList();

                foreach (var item in typs)
                {
                    var tmpTsk = new TaskItem()
                    {
                        DLLName = dll,
                        ClassName = item.FullName,
                        MethodName = "Initializer",
                    };
                    bool isExist = false;
                    if (setting.ActionName == "PreBuild")
                    {
                        isExist = setting.SettingFileData.PreBuildTasks.Any(x => tmpTsk.ClassName.EndsWith(x));
                    }
                    else if (setting.ActionName == "PostBuild")
                    {
                        isExist = setting.SettingFileData.PostBuildTasks.Any(x => tmpTsk.ClassName.EndsWith(x));
                    }
                    
                    if(isExist)
                        tasks.Add(tmpTsk);
                }
            }

            return tasks;
        }

        private static List<string> GetDllList(string folder)
        {
            var dllFiles = Directory.GetFiles(folder, "TfsSharpTR*.dll").ToList();

            dllFiles.Remove("TfsSharpTR.Core");
            if (!dllFiles.Any())
                throw new Exception("No dll files found.");

            return dllFiles;
        }
    }
}
