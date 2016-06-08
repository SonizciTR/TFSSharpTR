using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public class UserVariable<Tsetting> : BaseVariable where Tsetting : BaseBuildSetting
    {
        public UserVariable(Dictionary<string, string> keys) : base(keys)
        {
        }

        public string ActionName
        {
            get
            {
                return Get("Action");
            }
        }

        public string SettingFile
        {
            get
            {
                return Get("SettingFile");
            }
        }

        private string SettingFileDataString
        {
            get
            {
                string fileLocation = Get("SettingFile");
                bool isExist = !string.IsNullOrEmpty(fileLocation) && File.Exists(fileLocation);
                return isExist ? File.ReadAllText(fileLocation) : null;
            }
        }

        public Tsetting SettingFileData
        {
            get
            {
                string fData = SettingFileDataString;
                if (string.IsNullOrEmpty(fData))
                    return default(Tsetting);

                return JsonConvert.DeserializeObject<Tsetting>(fData);
            }
        }
    }
}
