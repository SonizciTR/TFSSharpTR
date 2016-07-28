using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    internal class TFSClientLogic : TFSBaseLogic
    {
        private static IIdentityManagementService idService = null;
        private static TeamFoundationIdentity tfsId = null;

        public TFSClientLogic(TfsVariable tfsVar) : base(tfsVar)
        {
            idService = TeamColl.GetService(typeof(IIdentityManagementService)) as IIdentityManagementService;
            tfsId = IdentityService.ReadIdentity(IdentitySearchFactor.DisplayName,
                        initSetting.BuildRequestedUser,
                        MembershipQuery.None,
                        ReadIdentityOptions.None);
        }

        private IIdentityManagementService IdentityService => idService;

        private TeamFoundationIdentity TFSIdentity => tfsId;

        private VersionControlServer VerControlServer => TeamColl.GetService(typeof(VersionControlServer)) as VersionControlServer;

        public string UniqueName => TFSIdentity.UniqueName;

        private string ProjectUri
        {
            get
            {
                var iCStructureServ = TeamColl.GetService<ICommonStructureService>();
                var tmProject = iCStructureServ.ListAllProjects().FirstOrDefault(x => x.Name == initSetting.TeamProjectName);
                return tmProject?.Uri;

                //var vcs = TeamColl.GetService(typeof(VersionControlServer)) as VersionControlServer;
                //var tp = vcs.GetTeamProject(initSetting.TeamProjectName);
                //return tp.ArtifactUri.AbsoluteUri;
            }
        }

        public TFSGroup GroupInformartions()
        {
            var grpList = new TFSGroup();
            string projectUri = ProjectUri;
            TeamFoundationIdentity[] projectGroups = IdentityService.ListApplicationGroups(projectUri, ReadIdentityOptions.None);

            foreach (TeamFoundationIdentity projectGroup in projectGroups)
            {
                bool isMem = IdentityService.IsMember(projectGroup.Descriptor, TFSIdentity.Descriptor);
                if (isMem)
                    grpList.grpsUser.Add(projectGroup.DisplayName);

                grpList.grpsAll.Add(projectGroup.DisplayName);
            }

            return grpList;
        }

        public List<TFSFileState> TfsPendingChangeFiles()
        {
            var filesChanged = new List<TFSFileState>();

            string shelveName = initSetting.TFVCShelveSet;
            var shelveDetail = shelveName?.Split(';');
            if ((shelveDetail == null) || (shelveDetail.Count() != 2))
                return new List<TFSFileState>();

            var changeGrp = VerControlServer.QueryShelvedChanges(shelveDetail[0], shelveDetail[1]);
            foreach (var chng in changeGrp)
            {
                foreach (var item in chng.PendingChanges)
                {
                    var tmpState = item.ToSourceControlFileState();
                    filesChanged.Add(new TFSFileState(item.LocalOrServerItem, tmpState));
                }
            }

            return filesChanged;
        }
    }
}
