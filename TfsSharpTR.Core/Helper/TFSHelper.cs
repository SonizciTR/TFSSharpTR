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
        private static Lazy<TFSLogic> logic = new Lazy<TFSLogic>(() => new TFSLogic(initSetting));
        private static TfsVariable initSetting = null;

        public static bool Initialize(TfsVariable tfsVar)
        {
            initSetting = tfsVar;
            return true;
        }

        public static bool Check()
        {
            if (initSetting == null)
                throw new Exception("TFSHelper not initialized.");

            return true;
        }

        public static List<string> GroupUserJoined()
        {
            Check();
            return logic.Value.GroupUserJoined();
        }

        public static List<string> GroupsAll()
        {
            Check();
            return logic.Value.GroupsAll();
        }

        public static string UserDomainName
        {
            get
            {
                Check();
                return logic.Value.UniqueName;
            }
        }

        public static List<string> ChangedFiles()
        {
            Check();
            return logic.Value.GroupsAll();
        }
    }
}
