using System;
using System.Collections.Generic;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    internal class VstestConsoleParser
    {
        private const string CnstUnitTestStartLine = "Starting test discovery, please wait...";
        private const string CnstUnitTestEndLine = "Results File:";
        private const string CnstCoverageStartLine = "Attachments:";

        public bool IsAllSucceeded { get; set; }
        public List<VstestMethodResult> Result { get; set; } = new List<VstestMethodResult>();
        public string TrxFilePath { get; set; }
        public string CoverageFilePath { get; set; }
        public string AssemblyName { get; set; }

        public VstestConsoleParser()
        {
            IsAllSucceeded = false;
        }

        public VstestConsoleParser(int exitCode, string consoleOutput, string dllName)
        {
            IsAllSucceeded = exitCode == 0;
            AssemblyName = System.IO.Path.GetFileName(dllName);
            StartParsing(consoleOutput);
        }

        private void StartParsing(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new Exception("vstest output can not be null.");

            var prsdtoLines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (prsdtoLines.Length < 9)
                throw new Exception("Not enough line to process vstest output.");

            bool isMethodResultArea = false;
            for (int i = 0; i < prsdtoLines.Length; i++)
            {
                if (prsdtoLines[i].StartsWith(CnstUnitTestStartLine))
                {
                    isMethodResultArea = true;
                    continue;
                }
                else if (prsdtoLines[i].StartsWith(CnstUnitTestEndLine))
                {
                    isMethodResultArea = false;
                    TrxFilePath = prsdtoLines[i].Replace(CnstUnitTestEndLine, "").Trim();
                }
                else if(prsdtoLines[i] == CnstCoverageStartLine)
                {
                    CoverageFilePath = prsdtoLines[i + 1].Trim();
                }

                if (isMethodResultArea)
                {
                    var tmp = new VstestMethodResult(prsdtoLines[i]);
                    Result.Add(tmp);
                }
            }
        }
    }
}
