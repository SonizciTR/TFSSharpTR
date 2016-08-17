using System;
using System.Collections.Generic;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    internal class VstestConsoleParser
    {
        private const string CnstUnitTestStartLine = "Starting test discovery, please wait...";
        private const string CnstUnitTestEndLine = "Results File:";

        public bool IsAllSucceeded { get; set; }
        public List<VstestMethodResult> Result { get; set; } = new List<VstestMethodResult>();

        public VstestConsoleParser()
        {
            IsAllSucceeded = false;
        }

        public VstestConsoleParser(int exitCode, string consoleOutput)
        {
            IsAllSucceeded = exitCode == 0;
            StartParsing(consoleOutput);
        }

        private void StartParsing(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new Exception("vstest output can not be null.");

            var plstNewLine = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (plstNewLine.Length < 9)
                throw new Exception("Not enough line to process vstest output.");

            bool isProcessArea = false;
            for (int i = 0; i < plstNewLine.Length; i++)
            {
                if (plstNewLine[i].StartsWith(CnstUnitTestStartLine))
                {
                    isProcessArea = true;
                    continue;
                }
                else if (plstNewLine[i].StartsWith(CnstUnitTestEndLine))
                {
                    isProcessArea = false;
                    break;
                }

                if (isProcessArea)
                {
                    var tmp = new VstestMethodResult(plstNewLine[i]);
                    Result.Add(tmp);
                }
            }
        }
    }
}
