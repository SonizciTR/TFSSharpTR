using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    /// <summary>
    /// All the data gathered from: https://www.visualstudio.com/en-us/docs/build/define/variables
    /// </summary>
    public class TfsVariable : BaseVariable
    {
        public TfsVariable(Dictionary<string, string> keys) : base(keys)
        {
        }

        /// <summary>
        /// The GUID of the team foundation collection
        /// </summary>
        public string CollectionId => Get("SYSTEM_COLLECTIONID");

        /// <summary>
        /// The name of the Build Definition the current build belongs to
        /// </summary>
        public string BuildDefinitionaName => Get("BUILD_DEFINITIONNAME");

        /// <summary>
        /// The working directory for this agent. By default $(Agent.RootDirectory)\_work.
        /// </summary>
        public string AgentWorkFolder => Get("AGENT_WorkFolder");

        /// <summary>
        /// The URL of the team foundation collection
        /// </summary>
        public string CollectionUri => Get("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");

        /// <summary>
        /// This user the build was requested for. In a CI build this will be the user who performed the check-in that triggered the build.
        /// </summary>
        public string BuildRequestedUser => Get("BUILD_REQUESTEDFOR");

        /// <summary>
        /// The name of the team project that this build belongs to
        /// </summary>
        public string TeamProjectName => Get("SYSTEM_TEAMPROJECT");

        /// <summary>
        /// Defined if your repository is Team Foundation Version Control.
        /// 
        /// If you are running a gated build or a shelveset build, this is set to the name of the shelveset you are building.
        /// 
        /// Note: This variable yields a value that is invalid for build use in a build number format
        /// </summary>
        public string TFVCShelveSet => Get("BUILD_SOURCETFVCSHELVESET");

        /// <summary>
        /// The type of repository you selected.
        /// 
        /// TfGit: TFS Git repository
        /// TfsVersionControl: Team Foundation Version Control
        /// Git: Git repository hosted on an external server
        /// GitHub
        /// Svn: Subversion
        /// </summary>
        public string RepoProvider => Get("BUILD_REPOSITORY_PROVIDER");

        /// <summary>
        /// The name of the repository.
        /// </summary>
        public string RepoName => Get("BUILD_REPOSITORY_NAME");

        /// <summary>
        /// The branch the build was queued for. Some examples:
        ///
        /// Git repo branch: refs/heads/master
        /// Git repo pull request: refs/pull/1/merge
        /// TFVC repo branch: $/teamproject/main
        /// TFVC repo gated check-in: Gated_2016-06-06_05.20.51.4369;username @live.com
        /// TFVC repo shelveset build: myshelveset; username @live.com
        ///  When you use this variable in your build number format, the forward slash characters (/) are replaced with underscore characters _).
        /// 
        /// Note: In TFVC, if you are running a gated check-in build or manually building a shelveset, you cannot use this variable in your build number format.
        /// </summary>
        public string SourceBranch => Get("BUILD_SOURCEBRANCH");

        /// <summary>
        /// Uses SourceBranch variable and Regex and get value
        /// </summary>
        public string SourceBranchPullId
        {
            get
            {
                string tmp = SourceBranch;
                if (string.IsNullOrEmpty(tmp))
                    return null;

                return tmp.StartsWith("refs/pull/") ? Regex.Replace(tmp, "[^0-9]+", string.Empty) : null;
            }
        }

        /// <summary>
        /// The URL for the repository. For example:
        /// 
        /// Git: https://fabrikamfiber.visualstudio.com/_git/Scripts
        /// TFVC: https://fabrikamfiber.visualstudio.com/
        /// </summary>
        public string RepoUri => Get("BUILD_REPOSITORY_URI");

        /// <summary>
        /// The latest version control change that is included in this build.
        /// 
        /// Git: The commit ID.
        /// TFVC: the changeset.
        /// </summary>
        public string BuildSourceVersion => Get("BUILD_SOURCEVERSION");
        
    }
}
