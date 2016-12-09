using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.PreBuild.FileControl
{
    public class FileControlSetting : BaseBuildSetting
    {
        public override string SettingFileAreaName
        {
            get
            {
                return "FileControlTask";
            }
        }

        public List<FileControlItem> Files { get; set; } = new List<FileControlItem>();
    }

    public class FileControlItem
    {
        /// <summary>
        /// File Name to control
        /// </summary>
        public List<string> FileNames { get; set; }

        /// <summary>
        /// These users allowed to change file
        /// </summary>
        public List<string> AllowedUser { get; set; }

        /// <summary>
        /// These group members allowed to change file
        /// </summary>
        public List<string> AllowedGroup { get; set; }
    }
}
