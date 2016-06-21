using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Common;

namespace TfsSharpTR.AutoDeploy
{
    internal static class IISHelper
    {
        private static ServerManager serverManager = null;

        public static string AppPoolStop(string serverName, string appPoolName)
        {
            try
            {
                serverManager = ServerManager.OpenRemote(serverName);
                var appPoolsAll = serverManager.ApplicationPools;
                var appItm = appPoolsAll.FirstOrDefault(x => x.Name == appPoolName);
                if (appItm != null)
                {
                    if ((appItm.State == ObjectState.Started) || (appItm.State == ObjectState.Starting))
                    {
                        appItm.Stop();
                    }

                    return null;
                }
                return "No appool found by the name of " + appPoolName;
            }
            catch (Exception ex)
            {
                serverManager = null;
                Logger.Write(ex);
                return "IISHelper.AppPoolStop failed. Ex = " + ex.ToString();
            }
        }

        public static string AppPoolStart(string serverName, string appPoolName)
        {
            try
            {
                if (serverManager == null)
                    serverManager = ServerManager.OpenRemote(serverName);

                var appPoolsAll = serverManager.ApplicationPools;
                var appItm = appPoolsAll.FirstOrDefault(x => x.Name == appPoolName);
                if (appItm != null)
                {
                    if ((appItm.State == ObjectState.Stopped) || (appItm.State == ObjectState.Stopping))
                    {
                        appItm.Start();
                    }

                    return null;
                }
                return "No appool found by the name of " + appPoolName;
            }
            catch (Exception ex)
            {
                serverManager = null;
                Logger.Write(ex);
                return "IISHelper.AppPoolStart failed. Ex = " + ex.ToString();
            }
        }
    }
}
