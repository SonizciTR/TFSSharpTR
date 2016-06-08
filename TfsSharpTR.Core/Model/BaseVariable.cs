using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public class BaseVariable
    {
        public Dictionary<string, string> RawData { get; set; } = new Dictionary<string, string>();

        public BaseVariable(Dictionary<string, string> keys)
        {
            RawData = keys;
        }

        internal string Get(string keyWord)
        {
            string tmp;

            return RawData.TryGetValue(keyWord, out tmp) ? tmp : null;
        }
    }
}
