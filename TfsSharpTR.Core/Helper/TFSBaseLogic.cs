using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    internal class TFSBaseLogic
    {
        public static TfsVariable initSetting = null;
        private static TfsTeamProjectCollection teamColl = null;

        public TFSBaseLogic(TfsVariable tfsVar)
        {
            initSetting = tfsVar;
            teamColl = new TfsTeamProjectCollection(new Uri(initSetting.CollectionUri));
        }

        public TfsTeamProjectCollection TeamColl => teamColl;
    }
}
