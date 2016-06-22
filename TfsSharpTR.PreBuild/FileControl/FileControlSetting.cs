using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.PreBuild.FileControl
{
    public class FileControlSetting : IBaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "FileControlTask";
            }
        }

        public List<FileControlItem> Files { get; set; }
    }

    public class FileControlItem
    {
        /// <summary>
        /// File Name to control
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// These users allowed to change file
        /// </summary>
        public List<string> AllowedUser { get; set; }

        /// <summary>
        /// These group members allowed to change file
        /// </summary>
        public List<string> AllowedGroup { get; set; }

        /// <summary>
        /// These users must not change file
        /// </summary>
        public List<string> RestrictedUser { get; set; }

        /// <summary>
        /// These group members must not change file
        /// </summary>
        public List<string> RestrictedGroup { get; set; }
    }
}
