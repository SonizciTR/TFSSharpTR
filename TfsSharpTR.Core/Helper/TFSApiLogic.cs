using Microsoft.TeamFoundation.Git.Client;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    /// <summary>
    /// TFS connection with Web Api
    /// https://www.visualstudio.com/en-us/docs/integrate/api/git/repositories
    /// </summary>
    internal class TFSApiLogic : TFSBaseLogic
    {
        private static TfsVariable SettingTFS = null;
        //https://{instance}/defaultcollection/_apis/git/repositories/{repository}/pullRequests/{pullRequest}?api-version={version}
        private const string urlFindCommitsFiles = "{0}/_apis/git/repositories/{1}/commits/{2}/changes";
        private const string urlLatestFileVersion = "{0}/_apis/git/repositories/{1}/items/{2}";

        public TFSApiLogic(TfsVariable tfsVar) : base(tfsVar)
        {
            SettingTFS = tfsVar;
        }

        private string HttpGet(string url)
        {
            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// !!! DANGER : if you are using "pull request" and "push" together there is a catch.
        /// WebApi finds changes between "pull request"s or "push"es.
        /// Because of this, first you make "pull request" then "push", at last "pull request"; api brings last"pull request" and "push" changes together.
        /// Like they pulled at the same time.
        /// </summary>
        /// <returns></returns>
        public List<TFSFileState> GitPendingChangeFiles()
        {
            var filesChanged = new List<TFSFileState>();

            GitRepositoryService grs = new GitRepositoryService();
            grs.Initialize(TeamColl);

            var gitRepo = grs.QueryRepositories(SettingTFS.TeamProjectName).FirstOrDefault(x => x.Name == SettingTFS.RepoName);
            if (gitRepo == null)
                return new List<TFSFileState>();

            string tmpUrl = string.Format(urlFindCommitsFiles, TeamColl.Uri, gitRepo.Id, SettingTFS.BuildSourceVersion);
            string rawJson = HttpGet(tmpUrl);
            if (string.IsNullOrEmpty(rawJson))
                return new List<TFSFileState>();

            var gitCommitData = JsonConvert.DeserializeObject<GitCommitRef>(rawJson);
            var isExist = gitCommitData?.Changes?.Any() ?? false;
            if ( !isExist )
                return new List<TFSFileState>();

            foreach (var item in gitCommitData.Changes)
            {
                var tmpState = item.ChangeType.ToSourceControlFileState();

                filesChanged.Add(new TFSFileState(item.Item.Path, tmpState));
            }

            return filesChanged;
        }

        /// <summary>
        /// Downloads the latest version of file from TFS-Git server
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public string DownloadLatestVersion(string fullPath)
        {
            GitRepositoryService grs = new GitRepositoryService();
            grs.Initialize(TeamColl);
            
            var gitRepo = grs.QueryRepositories(SettingTFS.TeamProjectName).FirstOrDefault(x => x.Name == SettingTFS.RepoName);
            if (gitRepo == null)
                throw new Exception("No repo found");

            var tmpUrl = string.Format(urlLatestFileVersion, TeamColl.Uri, gitRepo.Id, fullPath);

            var rawJson = HttpGet(tmpUrl);

            return rawJson;
        }
    }
}
