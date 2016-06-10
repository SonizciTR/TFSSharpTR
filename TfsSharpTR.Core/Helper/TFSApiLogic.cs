using Microsoft.TeamFoundation.Git.Client;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        private const string urlFindCommitsFiles = "https://{0}/defaultcollection/_apis/git/repositories/{1}/commits/{2}/changes";

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

        public List<string> GitPendingChangeFiles()
        {
            var filesChanged = new List<string>();

            GitRepositoryService grs = new GitRepositoryService();
            grs.Initialize(TeamColl);

            var gitRepo = grs.QueryRepositories(SettingTFS.RepoName).FirstOrDefault();
            if (gitRepo == null)
                return new List<string>();

            string commitId = SettingTFS.BuildSourceVersion;
            string tmpUrl = string.Format(urlFindCommitsFiles, TeamColl.Name, gitRepo.Name, SettingTFS.BuildSourceVersion);
            string rawJson = HttpGet(tmpUrl);
            if (string.IsNullOrEmpty(rawJson))
                return new List<string>();

            var gitCommitData = JsonConvert.DeserializeObject<GitCommitRef>(rawJson);
            var isExist = gitCommitData?.Changes?.Any() ?? false;
            if ( !isExist )
                return new List<string>();

            foreach (var item in gitCommitData.Changes)
            {
                filesChanged.Add(item.Item.Path);
            }

            return filesChanged;
        }
    }
}
