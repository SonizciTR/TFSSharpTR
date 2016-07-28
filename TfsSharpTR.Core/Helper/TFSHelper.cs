using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    public static class TFSHelper
    {
        private static Lazy<TFSClientLogic> logicClient = new Lazy<TFSClientLogic>(() => new TFSClientLogic(initSetting));
        private static Lazy<TFSApiLogic> logicApi = new Lazy<TFSApiLogic>(() => new TFSApiLogic(initSetting));
        private static TfsVariable initSetting = null;
        private static TFSGroup groupStash = null;

        public static bool Initialize(TfsVariable tfsVar)
        {
            initSetting = tfsVar;
            return true;
        }

        public static bool Check()
        {
            if (initSetting == null)
                throw new Exception("TFSHelper not initialized.");

            return true;
        }

        public static List<string> GroupUserJoined()
        {
            Check();
            if (groupStash == null)
                groupStash = logicClient.Value.GroupInformartions();

            return groupStash.grpsUser;
        }

        public static List<string> GroupsAll()
        {
            Check();
            if (groupStash == null)
                groupStash = logicClient.Value.GroupInformartions();

            return groupStash.grpsAll;
        }

        public static string UserDomainName
        {
            get
            {
                Check();
                return logicClient.Value.UniqueName;
            }
        }

        /// <summary>
        /// Finds changed file of this request.
        /// There is a DANGER for git. Please look :TFSApiLogic implementation
        /// </summary>
        /// <returns></returns>
        public static List<TFSFileState> ChangedFiles()
        {
            Check();
            if ((initSetting.RepoProvider == "TfsGit") || (initSetting.RepoProvider == "Git"))
            {   
                return logicApi.Value.GitPendingChangeFiles();
            }

            return logicClient.Value.TfsPendingChangeFiles();
        }

        public static string DownloadFile(string filePath)
        {
            Check();
            if ((initSetting.RepoProvider == "TfsGit") || (initSetting.RepoProvider == "Git"))
            {   
                return logicApi.Value.DownloadLatestVersion(filePath);
            }

            return null;
            //return logicClient.Value.TfsPendingChangeFiles();
        }
    }
}
