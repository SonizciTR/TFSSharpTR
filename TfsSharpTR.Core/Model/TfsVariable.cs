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

        /// <summary>
        /// Modifies how the build agent cleans things up. See : https://www.visualstudio.com/en-us/docs/build/define/repository
        /// </summary>
        public string BuildClean => Get("Build.Clean");

        /// <summary>
        /// If you want to disable downloading the sources specified on the Repository tab, define and set it to false.
        /// 
        /// Note: Several of the predefined variables mentioned below will not be defined while this variable is set to false.
        /// </summary>
        public string BuildSyncSources => Get("Build.SyncSources");

        /// <summary>
        /// If you need more detailed logs to debug build problems, define and set it to true.
        /// </summary>
        public string SystemDebug => Get("System.Debug");

        /// <summary>
        /// The local path on the agent where all folders for a given build definition are created. For example: c:\agent\_work\1
        /// </summary>
        public string AgentBuildDirectory => Get("AGENT_BUILDDIRECTORY");

        /// <summary>
        /// The local path on the agent where all folders for a given build definition are created. For example: c:\agent\_work\1
        /// </summary>
        public string AgentHomeDirectory => Get("AGENT_HOMEDIRECTORY");

        /// <summary>
        /// The ID of the agent.
        /// </summary>
        public string AgentId => Get("AGENT_ID");

        /// <summary>
        /// The name of the agent that is registered with the pool.
        /// 
        /// If you are using an on-premises agent, this directory is specified by you.See Deploy a Windows build agent or Cross-platform build and release agent.
        /// </summary>
        public string AgentName => Get("AGENT_NAME");

        /// <summary>
        /// The local path on the agent where any artifacts are copied to before being pushed to their destination. For example: c:\agent\_work\1\a.
        /// 
        /// A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts steps.
        /// 
        /// 
        /// Note: This directory is purged before each new build, so you don't have to clean it up yourself.
        /// </summary>
        public string ArtifactStagingDirectory => Get("BUILD_ARTIFACTSTAGINGDIRECTORY");

        /// <summary>
        /// The ID of the record for the completed build.
        /// </summary>
        public string BuildId => Get("BUILD_BUILDID");

        /// <summary>
        /// The name of the completed build. You can specify the build number format that generates this value on the General tab.
        /// 
        /// A typical use of this variable is to make it part of the label format, which you specify on the repository tab.
        /// 
        /// Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will fail.
        /// </summary>
        public string BuildNumber => Get("BUILD_BUILDNUMBER");

        /// <summary>
        /// The URI for the build. For example: vstfs:///Build/Build/1430.
        /// </summary>
        public string BuildUri => Get("BUILD_BUILDURI");

        /// <summary>
        /// The local path on the agent you can use as an output folder for compiled binaries. For example: c:\agent\_work\1\b.
        /// 
        /// By default, new build definitions are not set up to clean this directory.You can define your build to clean it up on the Repository tab.
        /// </summary>
        public string BuildDirectory => Get("BUILD_BINARIESDIRECTORY");

        /// <summary>
        /// The version of the build definition.
        /// </summary>
        public string BuildDefinitionVersion => Get("BUILD_DEFINITIONVERSION");

        /// <summary>
        /// https://www.visualstudio.com/en-us/docs/build/define/variables#identity_values
        /// 
        /// Note: This value can contain whitespace or other invalid label characters.In these cases, the label format will fail.
        /// </summary>
        public string BuildQueuedBy => Get("BUILD_QUEUEDBY");

        /// <summary>
        /// https://www.visualstudio.com/en-us/docs/build/define/variables#identity_values
        /// </summary>
        public string BuildQueuedById => Get("BUILD_QUEUEDBYID");

        /// <summary>
        /// The value you've selected for Clean on the https://www.visualstudio.com/en-us/docs/build/define/repository
        /// </summary>
        public string RepoClean => Get("BUILD_REPOSITORY_CLEAN");

        /// <summary>
        /// The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s.
        /// 
        /// By default, new build definitions update only the changed files.You can modify how files are downloaded on the Repository tab.
        /// </summary>
        public string RepoLocalPath => Get("BUILD_REPOSITORY_LOCALPATH");

        /// <summary>
        /// Defined if your repository is Team Foundation Version Control. The name of the TFVC workspace used by the build agent.
        /// 
        /// For example, if the Agent.BuildDirectory is c:\agent\_work\12 and the Agent.Id is 8, the workspace name could be: ws_12_8
        /// </summary>
        public string RepoTFVCworkspace => Get("BUILD_REPOSITORY_TFVC_WORKSPACE");

        /// <summary>
        /// https://www.visualstudio.com/en-us/docs/build/define/variables#identity_values
        /// </summary>
        public string BuildRequestForId => Get("BUILD_REQUESTEDFORID");

        /// <summary>
        /// The name of the branch the build was queued for.
        /// 
        /// Git repo branch or pull request: The last path segment in the ref. For example, in refs/heads/master this value is master.
        /// TFVC repo branch: The last path segment in the root server path for the workspace. For example in $/teamproject/main this value is main.
        /// TFVC repo gated check-in or shelveset build is the name of the shelveset.For example, Gated_2016-06-06_05.20.51.4369;username @live.com or myshelveset;username @live.com.
        /// Note: In TFVC, if you are running a gated check-in build or manually building a shelveset, you cannot use this variable in your build number format.
        /// </summary>
        public string BuildSourceBranchName => Get("BUILD_SOURCEBRANCHNAME");

        /// <summary>
        /// The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s.
        /// 
        /// By default, new build definitions update only the changed files.You can modify how files are downloaded on the Repository tab.
        /// </summary>
        public string BuildSourceDirectory => Get("BUILD_SOURCESDIRECTORY");

        /// <summary>
        /// The local path on the agent where any artifacts are copied to before being pushed to their destination. For example: c:\agent\_work\1\a.
        /// 
        /// A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts steps.
        /// 
        /// 
        /// Note: This directory is purged before each new build, so you don't have to clean it up yourself.
        /// </summary>
        public string BuildStagingDirectory => Get("BUILD_STAGINGDIRECTORY");

        /// <summary>
        /// The value you've selected for Checkout submodules on the repository tab.
        /// </summary>
        public string RepoGitSubModuleCheckout => Get("BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT");

        /// <summary>
        /// The local path on the agent where the test results are created. For example: c:\agent\_work\1\TestResults
        /// </summary>
        public string TestResultDirectory => Get("COMMON_TESTRESULTSDIRECTORY");

        /// <summary>
        /// Use the OAuth token to access the REST API.
        /// </summary>
        public string AccessToken => Get("SYSTEM_ACCESSTOKEN");

        /// <summary>
        /// The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s.
        /// 
        /// By default, new build definitions update only the changed files.You can modify how files are downloaded on the Repository tab.
        /// </summary>
        public string WorkingDirectory => Get("SYSTEM_DEFAULTWORKINGDIRECTORY");

        /// <summary>
        /// The ID of the build definition.
        /// </summary>
        public string SystemDefinitionId => Get("SYSTEM_DEFINITIONID");

        /// <summary>
        /// The ID of the team project that this build belongs to.
        /// </summary>
        public string TeamProjectId => Get("SYSTEM_TEAMPROJECTID");

        /// <summary>
        /// Set to True if the script is being run by a build step.
        /// </summary>
        public string TFBuild => Get("TF_BUILD");
    }
}
