using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
