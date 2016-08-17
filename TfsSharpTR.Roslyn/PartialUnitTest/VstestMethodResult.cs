﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    internal class VstestMethodResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string MethodName { get; set; }
        public VstestMethodResult(string data)
        {
            var spltd = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (spltd.Length < 3)
                throw new Exception("vstest output line process failed.");

            Message = spltd[0];
            IsSuccess = Message == "Success";
            MethodName = spltd[1];
        }
    }
}