using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    /// <summary>
    /// User settings that manage at web build screen
    /// </summary>
    /// <typeparam name="Tsetting"></typeparam>
    public class UserVariable<Tsetting> : CommonVariable where Tsetting : class
    {
        private string taskBelong;
        public UserVariable(string taskName, Dictionary<string, string> keys) : base(keys)
        {
            taskBelong = typeof(Tsetting) == typeof(RawBasicBuildSetting) 
                ? RawBasicBuildSetting.KeyBaseConfigArea 
                : taskName;
        }

        /// <summary>
        /// PreBuild or PostBuild
        /// </summary>
        public string ActionName => Get("Action");

        /// <summary>
        /// Json setting file and Task Libraries will look from this file's folder
        /// </summary>
        public string SettingFile => Get("SettingFile");

        /// <summary>
        /// Json setting file's all text
        /// </summary>
        private string DataasString
        {
            get
            {
                string fileLocation = Get("SettingFile");
                bool isExist = !string.IsNullOrEmpty(fileLocation) && File.Exists(fileLocation);
                return isExist ? File.ReadAllText(fileLocation) : null;
            }
        }

        /// <summary>
        /// Task's special setting from json file
        /// </summary>
        public Tsetting Data
        {
            get
            {
                string fData = DataasString;
                if (string.IsNullOrEmpty(fData))
                    return default(Tsetting);

                string keyName = taskBelong;
                if (keyName == RawBasicBuildSetting.KeyBaseConfigArea)
                {
                    return JsonConvert.DeserializeObject<Tsetting>(fData);
                }
                else
                {
                    JObject msg = JObject.Parse(fData);
                    var tmpPart = msg[keyName];
                    if (tmpPart == null)
                        return null;

                    return JsonConvert.DeserializeObject<Tsetting>(tmpPart.ToString());
                }
            }
        }

        /// <summary>
        ///  Where is the all the libraries are working. Because Powershell script copying the libraries to a latest location. Task libraries folder.
        /// </summary>
        public string WorkingPath
        {
            get
            {
                if (string.IsNullOrEmpty(SettingFile))
                    return null;

                return Path.GetDirectoryName(SettingFile) + "\\";
            }
        }
    }
}
