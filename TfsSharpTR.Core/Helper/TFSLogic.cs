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
    internal class TFSLogic
    {
        private static TfsVariable initSetting = null;
        private static List<string> grpsUser = null;
        private static List<string> grpsAll = null;
        private static TfsTeamProjectCollection teamColl = null;
        private static IIdentityManagementService idService = null;
        private static TeamFoundationIdentity tfsId = null;

        public TFSLogic(TfsVariable tfsVar)
        {
            initSetting = tfsVar;
        }

        private TfsTeamProjectCollection TeamColl
        {
            get
            {
                if (teamColl == null)
                {
                    teamColl = new TfsTeamProjectCollection(new Uri(initSetting.CollectionUri));
                }
                return teamColl;
            }
        }
        private IIdentityManagementService IdentityService
        {
            get
            {
                if (idService == null)
                {
                    idService = TeamColl.GetService(typeof(IIdentityManagementService)) as IIdentityManagementService;
                }
                return idService;
            }
        }
        private TeamFoundationIdentity TFSIdentity
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
        private VersionControlServer VerControlServer
        {
            get
            {
                return TeamColl.GetService(typeof(VersionControlServer)) as VersionControlServer;
            }
        }

        public string UniqueName
        {
            get
            {
                return TFSIdentity.UniqueName;
            }
        }

        

        private string ProjectUri
        {
            get
            {
                var vcs = TeamColl.GetService(typeof(VersionControlServer)) as VersionControlServer;
                var tp = vcs.GetTeamProject(initSetting.TeamProjectName);
                return tp.ArtifactUri.AbsoluteUri;
            }
        }

        public List<string> GroupUserJoined()
        {
            if (grpsUser == null)
                CollectGroups();

            return grpsUser;
        }

        public List<string> GroupsAll()
        {
            if (grpsAll == null)
                CollectGroups();

            return grpsAll;
        }

        private bool CollectGroups()
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

        public List<string> TfsPendingChangeFiles()
        {
            var filesChanged = new List<string>();

            string shelveName = initSetting.TFVCShelveSet;
            var shelveDetail = shelveName?.Split(';');
            if ((shelveDetail == null) || (shelveDetail.Count() != 2))
                return new List<string>();

            var changeGrp = VerControlServer.QueryShelvedChanges(shelveDetail[0], shelveDetail[1]);
            foreach (var chng in changeGrp)
            {
                foreach (var item in chng.PendingChanges)
                {
                    filesChanged.Add(item.LocalOrServerItem);
                }
            }

            return filesChanged;
        }

        public List<string> GitPendingChangeFiles()
        {
            var filesChanged = new List<string>();

            string shelveName = initSetting.TFVCShelveSet;

            return filesChanged;
        }
    }
}
