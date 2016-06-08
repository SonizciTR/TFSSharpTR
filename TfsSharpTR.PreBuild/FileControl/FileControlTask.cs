using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.Model;

namespace TfsSharpTR.PreBuild.FileControl
{
    public class FileControlTask : BaseTask<FileControlSetting>
    {
        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<FileControlSetting> usrVariables)
        {
            throw new NotImplementedException();
        }
    }
}
