﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.AutoDeploy
{
    public class AutoDeployTask : BaseTask<AutoDeploySetting>
    {
        public const string KeyHashFileName = "HashDeployment.log";

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<AutoDeploySetting> usrVariables)
        {
            var sourceFolder = tfsVariables.BuildDirectory;
            var setting = usrVariables?.SettingFileData?.AutoDeployTask;
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

            List<string> srcFiles = PrepareFileList(sourceFolder, setting);
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

            var startMsg = StartIIS(willImpersonate, setting, willIISStopStart);
            if (!string.IsNullOrEmpty(startMsg))
                return new TaskStatu("ADT06", startMsg);

            return new TaskStatu("AutoDeploy finished successfully.");
        }

        private string StartRollBack(List<string> srcFiles, string sourceFolder, AutoDeploySettingItem setting)
        {
            return FileOperationHelper.DirectoryCopy(setting.GetBackupFolder(sourceFolder), setting.DeployFolder, true);
        }

        private string StartIIS(bool willImpersonate, AutoDeploySettingItem setting, bool willIISStopStart)
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

        private string StartDeploy(List<string> sourceFiles, string sourceFolder, AutoDeploySettingItem setting)
        {
            List<string> srcErrors = new List<string>();
            foreach (var srcFile in sourceFiles)
            {
                string dplyFile = "";
                try
                {
                    dplyFile = setting.DeployFolder + "\\" + FileOperationHelper.RemoveBaseFolder(sourceFolder, srcFile);
                    File.Copy(srcFile, dplyFile, true);
                }
                catch(Exception ex)
                {
                    srcErrors.Add(srcFile);
                }
            }
            foreach (var srcFile in srcErrors)
            {
                string dplyFile = "";
                try
                {
                    dplyFile = setting.DeployFolder + "\\" + FileOperationHelper.RemoveBaseFolder(sourceFolder, srcFile);
                    File.Copy(srcFile, dplyFile, true);
                }
                catch (Exception ex)
                {
                    return $"Error while copying [{srcFile}] file to [{dplyFile}]";
                }
            }
            return null;
        }

        private string StopIIS(bool willImpersonate, AutoDeploySettingItem setting, bool willIISStopStart)
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

        private List<string> PrepareFileList(string sourceFolder, AutoDeploySettingItem setting)
        {
            bool isDiffDeployment = setting.Mode == AutoDeploySettingItem.KeyDiff;
            var filteredFiles = FindFilesandFilter(sourceFolder, setting);
            var hfNewData = HashOperationHelper.GenerateHashFileStructure(sourceFolder, filteredFiles);
            List<HashItem> oldhfData = isDiffDeployment ?
                HashItem.ParseFromFileLineList(FileOperationHelper.SafeFileReadLines(setting.DeployFolder + KeyHashFileName))
                : new List<HashItem>();

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

                var fullPath = sourceFolder + item.FileName;
                files.Add(fullPath);
            }

            return files;
        }

        private List<string> FindFilesandFilter(string sourceFolder, AutoDeploySettingItem setting)
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
