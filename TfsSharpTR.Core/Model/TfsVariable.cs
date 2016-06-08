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
        public string CollectionId
        {
            get
            {
                return Get("SYSTEM_COLLECTIONID");

            }
        }

        /// <summary>
        /// The name of the Build Definition the current build belongs to
        /// </summary>
        public string BuildDefinitionaName
        {
            get
            {
                return Get("BUILD_DEFINITIONNAME");

            }
        }

        /// <summary>
        /// The working directory for this agent. By default $(Agent.RootDirectory)\_work.
        /// </summary>
        public string AgentWorkFolder
        {
            get
            {
                return Get("AGENT_WorkFolder");
            }
        }

        /// <summary>
        /// The URL of the team foundation collection
        /// </summary>
        public string CollectionUri
        {
            get
            {
                return Get("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
            }
        }

        /// <summary>
        /// This user the build was requested for. In a CI build this will be the user who performed the check-in that triggered the build.
        /// </summary>
        public string BuildRequestedUser
        {
            get
            {
                return Get("BUILD_REQUESTEDFOR");
            }
        }

        /// <summary>
        /// The name of the team project that this build belongs to
        /// </summary>
        public string TeamProjectName
        {
            get
            {
                return Get("SYSTEM_TEAMPROJECT");
            }
        }
    }
}
