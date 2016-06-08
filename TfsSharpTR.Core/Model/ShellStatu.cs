using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public class ShellStatu
    {
        public bool IsSuccess { get; set; }
        public List<string> Msgs { get; set; }
    }
}
