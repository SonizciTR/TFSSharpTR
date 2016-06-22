using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.AutoDeploy
{
    public class AutoDeployTask : BaseTask<AutoDeploySetting>
    {
        public const string KeyHashFileName = "HashDeployment.log";

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<AutoDeploySetting> usrVariables)
        {
            var sourceFolder = tfsVariables.BuildDirectory;
            var setting = usrVariables?.SettingFileData;
            if (setting == null)
                return new TaskStatu("ADT01", "No setting loaded.");

            var inptControl = setting.CheckRules();
            if (!string.IsNullOrEmpty(inptControl))
                return new TaskStatu("ADT02", inptControl);

            if (setting.TakeBackup)
            {
                WriteDetail("Backup started.");
                var bckMsg = FileOperationHelper.TakeBackup(setting, sourceFolder, setting.BackupFolder);
                if (!string.IsNullOrEmpty(bckMsg))
                    return new TaskStatu("ADT03", bckMsg);
                WriteDetail("Backup finished.");
            }

            List<HashItem> fileHashInfo;
            List<string> srcFiles = PrepareFileList(sourceFolder, setting, out fileHashInfo);
            WriteDetail($"For deploy; {srcFiles.Count} files found.");

            bool willImpersonate = !string.IsNullOrEmpty(setting.ImpersonateName);
            bool willIISStopStart = !string.IsNullOrEmpty(setting.AppPoolName) && !string.IsNullOrEmpty(setting.IISServerName);

            var appMsg = StopIIS(willImpersonate, setting, willIISStopStart);
            if (!string.IsNullOrEmpty(appMsg))
                return new TaskStatu("ADT04", appMsg);

            string deplMsg = StartDeploy(srcFiles, sourceFolder, setting);
            if (!string.IsNullOrEmpty(deplMsg))
            {
                string rllMsg = StartRollBack(srcFiles, sourceFolder, setting);
                return new TaskStatu("ADT05", deplMsg);
            }
            string msgWrtHash = WriteNewHashFile(setting, fileHashInfo);

            var startMsg = StartIIS(willImpersonate, setting, willIISStopStart);
            if (!string.IsNullOrEmpty(startMsg))
                return new TaskStatu("ADT06", startMsg);

            return new TaskStatu("AutoDeploy finished successfully.");
        }

        private string WriteNewHashFile(AutoDeploySetting setting, List<HashItem> fileHashInfo)
        {
            string fileName = setting.DeployFolder + "\\" + KeyHashFileName;
            File.WriteAllLines(fileName, fileHashInfo.Select(x => x.HashLine).ToArray());
            return null;
        }

        private string StartRollBack(List<string> srcFiles, string sourceFolder, AutoDeploySetting setting)
        {
            return FileHelper.DirectoryCopy(setting.GetBackupFolder(sourceFolder), setting.DeployFolder, true);
        }

        private string StartIIS(bool willImpersonate, AutoDeploySetting setting, bool willIISStopStart)
        {
            if (willIISStopStart)
            {
                string msStart = IISHelper.AppPoolStart(setting.IISServerName, setting.AppPoolName);
                if (!string.IsNullOrEmpty(msStart) && setting.MustStopIISAppPool)
                {
                    return "AppPool start failed. " + msStart;
                }
            }

            return null;
        }

        private string StartDeploy(List<string> sourceFiles, string sourceFolder, AutoDeploySetting setting)
        {
            List<string> srcErrors = new List<string>();
            if (!Directory.Exists(setting.DeployFolder))
                FileHelper.CreateDirectory(setting.DeployFolder);

            foreach (var srcFile in sourceFiles)
            {
                string dplyFile = "";
                try
                {
                    dplyFile = setting.DeployFolder + "\\" + FileHelper.RemoveBaseFolder(sourceFolder, srcFile);
                    string folder = Path.GetDirectoryName(dplyFile);
                    if (!Directory.Exists(folder))
                        FileHelper.CreateDirectory(folder, true);
                    File.Copy(srcFile, dplyFile, true);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                    srcErrors.Add(srcFile);
                }
            }
            foreach (var srcFile in srcErrors)
            {
                string dplyFile = "";
                try
                {
                    dplyFile = setting.DeployFolder + "\\" + FileHelper.RemoveBaseFolder(sourceFolder, srcFile);
                    File.Copy(srcFile, dplyFile, true);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                    return $"Error while copying [{srcFile}] file to [{dplyFile}]";
                }
            }

            return srcErrors.Any() ? "Could not deploy these files : " + string.Join(", ", srcErrors) : null;
        }

        private string StopIIS(bool willImpersonate, AutoDeploySetting setting, bool willIISStopStart)
        {
            if (willImpersonate)
            {
                var rsltImp = Impersonator.Change(setting.ImpersonateName, setting.ImpersonatePass, setting.ImpersonateDomain);
                if (!string.IsNullOrEmpty(rsltImp))
                    return rsltImp;
            }

            if (willIISStopStart)
            {
                string msgIISStop = IISHelper.AppPoolStop(setting.IISServerName, setting.AppPoolName);
                if (!string.IsNullOrEmpty(msgIISStop) && setting.MustStopIISAppPool)
                {
                    return "AppPool stop failed. " + msgIISStop;
                }
            }

            return null;
        }

        private List<string> PrepareFileList(string sourceFolder, AutoDeploySetting setting, out List<HashItem> fileHashInfo)
        {
            bool isDiffDeployment = setting.Mode == AutoDeploySetting.KeyDiff;
            var filteredFiles = FindFilesandFilter(sourceFolder, setting);
            var hfNewData = HashOperationHelper.GenerateHashFileStructure(sourceFolder, filteredFiles);
            fileHashInfo = hfNewData;
            List<HashItem> oldhfData = new List<HashItem>();

            if (isDiffDeployment)
            {
                string oldFile = setting.DeployFolder + "\\" + KeyHashFileName;
                if (File.Exists(oldFile))
                {
                    oldhfData = HashItem.ParseFromFileLineList(FileHelper.SafeFileReadLines(oldFile));
                }
            }

            List<string> files = FindDeploymentFiles(sourceFolder, filteredFiles, hfNewData, oldhfData);

            return files;
        }

        private List<string> FindDeploymentFiles(string sourceFolder, List<string> filteredFiles, List<HashItem> hfNewData, List<HashItem> oldhfData)
        {
            if (!oldhfData.Any())
                return filteredFiles;

            var files = new List<string>();
            foreach (var item in hfNewData)
            {
                bool isOldFile = oldhfData.Any(x => x.HashLine == item.HashLine);
                if (isOldFile)
                    continue;

                var fullPath = sourceFolder + "\\" + item.FileName;
                files.Add(fullPath);
            }

            return files;
        }

        private List<string> FindFilesandFilter(string sourceFolder, AutoDeploySetting setting)
        {
            var allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList();

            if (setting.ExcludedFiles.Any())
            {
                var files = new List<string>();
                foreach (var itm in allFiles) // Maybe later i will add pattern look
                {
                    bool isExst = setting.ExcludedFiles.Any(x => itm.EndsWith(x));
                    if (isExst)
                        continue;
                    files.Add(itm);
                }
                return files;
            }
            else
                return allFiles;
        }

    }
}
