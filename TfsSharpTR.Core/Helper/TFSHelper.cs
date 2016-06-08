﻿using Microsoft.TeamFoundation.Client;
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
        private static List<string> grpsUser = null;
        private static List<string> grpsAll = null;
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
            if (grpsUser == null)
                CollectGroups();

            return grpsUser;
        }

        public static List<string> GroupsAll()
        {
            if (grpsAll == null)
                CollectGroups();

            return grpsAll;
        }

        private static bool CollectGroups()
        {
            grpsAll = new List<string>();
            grpsUser = new List<string>();
            string projectUri = ProjectUri;
            TeamFoundationIdentity[] projectGroups = IdentityService.ListApplicationGroups(projectUri, ReadIdentityOptions.None);

            foreach (TeamFoundationIdentity projectGroup in projectGroups)
            {
                bool isMem = IdentityService.IsMember(projectGroup.Descriptor, TFSIdentity.Descriptor);
                if (isMem)
                    grpsUser.Add(projectGroup.DisplayName);
                grpsAll.Add(projectGroup.DisplayName);
            }

            return true;
        }
    }
}
