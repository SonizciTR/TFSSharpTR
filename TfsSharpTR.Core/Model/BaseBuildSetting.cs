﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    /// <summary>
    /// BaseTask's setting model should be implemented from this class
    /// </summary>
    public abstract class BaseBuildSetting
    {
        /// <summary>
        /// Which part of setting file to parse. This is the key name of whole json file. Deserialize to this object
        /// </summary>
        public abstract string SettingFileAreaName { get; }
    }

    /// <summary>
    /// Build settings, from Json file
    /// </summary>
    public class RawBasicBuildSetting : BaseBuildSetting
    {
        internal const string KeyBaseConfigArea = "Base";
        public override string SettingFileAreaName
        {
            get
            {
                return KeyBaseConfigArea;
            }
        }

        /// <summary>
        /// Which tasks will run at PreBuild
        /// </summary>
        public List<string> PreBuildTasks { get; set; }

        /// <summary>
        /// Which tasks will run at PostBuild
        /// </summary>
        public List<string> PostBuildTasks { get; set; }
    }
}
