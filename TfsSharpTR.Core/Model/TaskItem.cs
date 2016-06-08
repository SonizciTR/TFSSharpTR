using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public class TaskList : List<TaskItem>
    {
    }

    public class TaskItem
    {
        public string DLLName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
    }
}
