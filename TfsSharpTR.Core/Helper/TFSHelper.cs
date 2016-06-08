using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    public static class TFSHelper
    {
        private static TfsVariable initSetting = null;
        private static TfsTeamProjectCollection teamColl = null;
        private static IIdentityManagementService idService = null;
        private static TeamFoundationIdentity tfsId = null;
        

        public static bool Initialize(TfsVariable tfsVar)
        {
            initSetting = tfsVar;
            return true;
        }

        private static TfsTeamProjectCollection TeamColl
        {
            get
            {
                if(teamColl == null)
                {
                    teamColl = new TfsTeamProjectCollection(new Uri(initSetting.CollectionUri));
                }
                return teamColl;
            }
        }
        private static IIdentityManagementService IdentityService
        {
            get
            {
                if (idService == null)
                {
                    idService  = TeamColl.GetService(typeof(IIdentityManagementService)) as IIdentityManagementService;
                }
                return idService;
            }
        }
        private static TeamFoundationIdentity TFSIdentity
        {
            get
            {
                if (tfsId == null)
                {
                    tfsId = IdentityService.ReadIdentity(IdentitySearchFactor.DisplayName, 
                        initSetting.BuildRequestedUser, 
                        MembershipQuery.None, 
                        ReadIdentityOptions.None);
                }
                return tfsId;
            }
        }

        private static string ProjectUri
        {
            get
            {
                var vcs = TeamColl.GetService(typeof(VersionControlServer)) as VersionControlServer;
                var tp = vcs.GetTeamProject(initSetting.TeamProjectName);
                return tp.ArtifactUri.AbsoluteUri;
            }
        }

        public static List<string> GroupUserJoined()
        {
            var grps = new List<string>();
            string projectUri = ProjectUri;
            TeamFoundationIdentity[] projectGroups = IdentityService.ListApplicationGroups(projectUri, ReadIdentityOptions.None);

            //Dictionary<IdentityDescriptor, object> descSet = new Dictionary<IdentityDescriptor, object>(IdentityDescriptorComparer.Instance);

            foreach (TeamFoundationIdentity projectGroup in projectGroups)
            {
                //descSet[projectGroup.Descriptor] = projectGroup.Descriptor;
                bool isMem = IdentityService.IsMember(projectGroup.Descriptor, TFSIdentity.Descriptor);
                if (isMem)
                    grps.Add(projectGroup.DisplayName);
            }

            //// Expanded membership of project groups
            //projectGroups = IdentityService.ReadIdentities(descSet.Keys.ToArray(), MembershipQuery.Expanded, ReadIdentityOptions.None);

           
            //// Collect all descriptors
            //foreach (TeamFoundationIdentity projectGroup in projectGroups)
            //{
            //    foreach (IdentityDescriptor mem in projectGroup.Members)
            //    {
            //        descSet[mem] = mem;
            //        grps.Add(mem.Identifier);
            //    }
            //}

            return grps;
        }

        //public static List<string> GroupsAll()
        //{
        //    TeamColl.
        //}
    }
}
