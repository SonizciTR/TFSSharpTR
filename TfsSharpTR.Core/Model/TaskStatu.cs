using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Model
{
    public class TaskStatu
    {
        public bool IsSuccess { get; set; }
        public string Code { get; set; } = "-1";
        public string GeneralMsg { get; set; }
        public List<string> Detail { get; set; } = new List<string>();

        public TaskStatu(string successMsg, List<string> details = null)
        {
            IsSuccess = true;
            GeneralMsg = successMsg;
            Code = "00";
            Detail = details ?? new List<string>();
        }

        public TaskStatu(string errorCode, string errorMsg, List<string> details = null)
        {
            if (Code == "00")
                throw new Exception("Zero is success code. You can not set zero for error codes.");

            IsSuccess = false;
            Code = errorCode;
            GeneralMsg = errorMsg;
            Detail = details ?? new List<string>();
        }
    }
}
