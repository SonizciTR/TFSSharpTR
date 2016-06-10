using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.Core.Helper
{
    internal class TFSApiLogic : TFSBaseLogic
    {
        private static TfsVariable initSetting = null;

        public TFSApiLogic(TfsVariable tfsVar) : base(tfsVar)
        {
            initSetting = tfsVar;
        }

        private string HttpGet(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public List<string> GitPendingChangeFiles()
        {
            var filesChanged = new List<string>();

            string shelveName = initSetting.TFVCShelveSet;

            return filesChanged;
        }
    }
}
