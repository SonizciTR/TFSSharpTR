﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsSharpTR.Core;
using TfsSharpTR.Core.DiffEngine;
using TfsSharpTR.Core.Helper;
using TfsSharpTR.Core.Model;
using TfsSharpTR.Core.Common;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics;
using Microsoft.VisualStudio.Coverage.Analysis;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    /// <summary>
    /// Find changed files. Then found members and check those member has used in test methods.
    /// 
    /// </summary>
    public class PartialUnitTestTask : BaseTask<PartialUnitTestSetting>
    {
        const string cmdParams = @"{0} /Tests:{1} /logger:trx /Enablecodecoverage";

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<PartialUnitTestSetting> usrVariables)
        {
            var setting = usrVariables.Config;
            if (setting == null)
                return new TaskStatu("PUT01", "No setting found.");

            var changedAll = TFSHelper.ChangedFiles();
            if (!changedAll.Any())
                return new TaskStatu("No code change found on TFS");

            var codes = changedAll.Where(x => x.FilePath.EndsWith(".cs")).ToList();
            var codesChanged = codes.Where(x => x.State == SourceControlFileState.Changed).ToList();
            var codesAdded = codes.Where(x => x.State == SourceControlFileState.Added).ToList();
            if (codesChanged.Count + codesAdded.Count < 1)
                return new TaskStatu("No code changes found to check for partial unit test");

            var tmpFile = codesChanged.Any() ? codesChanged[0] : codesAdded[0];
            Solution gSolution = GetSolutionInfo(tfsVariables, setting, tmpFile);
            if (gSolution == null)
                return new TaskStatu("PUT03", "No suitable solution found");

            var tmpMethodsforChanged = GetChangeforChangedFiles(codesChanged, tfsVariables, gSolution);
            if (tmpMethodsforChanged == null)
                return new TaskStatu("PUT04", "No suitable project document found for changed list");
            var tmpMethodsforAdded = GetChangeforChangedAdded(codesAdded, tfsVariables, gSolution);
            if (tmpMethodsforAdded == null)
                return new TaskStatu("PUT05", "No suitable project document found for added list");

            int totalMethod = tmpMethodsforAdded.MethodCount + tmpMethodsforChanged.MethodCount;
            WriteDetail(string.Format("{0} number of methods will be looked for unit test", totalMethod));
            var unitTesttoCheck = CheckforUnitTest(setting, gSolution, tfsVariables, tmpMethodsforChanged, tmpMethodsforAdded);
            if (unitTesttoCheck == null)
                return new TaskStatu("PUT06", "No unit test found for partial check");

            bool isAllSucc = RunUnitTests(tfsVariables, setting, unitTesttoCheck, usrVariables.WorkingPath);

            return isAllSucc ? new TaskStatu("Partial Unit Test check successful") : new TaskStatu("PUT07", "Partial Unit Test  failed");
        }

        private bool RunUnitTests(TfsVariable tfsVariables, PartialUnitTestSetting setting, List<UnitTestDetail> unitTesttoCheck, string workingPath)
        {
            Stopwatch watch = Stopwatch.StartNew();
            string cmdRaw = string.IsNullOrEmpty(setting.RunSettingFile) ? cmdParams : cmdParams + " /Settings:{2}";
            var groupedUnitTest = unitTesttoCheck.GroupBy(x => x.AssemblyPath);
            bool isSucc = false;
            var cmdResults = new List<VstestConsoleParser>();
            foreach (var itmGrp in groupedUnitTest)
            {
                var sb = new StringBuilder();
                foreach (var itmUnitTest in itmGrp)
                {
                    sb.Append(itmUnitTest.MethodName + ",");
                }

                string tstMethodLine = sb.ToString().TrimEnd(',');
                string settFile = Path.Combine(workingPath, setting.RunSettingFile);
                WriteDetail($"Unit Test Setting file found : {File.Exists(settFile)}. Path = [{settFile}]");
                string prms = string.Format(cmdRaw, itmGrp.Key, tstMethodLine, settFile);

                var runResult = RunMsUnitTestExe(prms, itmGrp.Key);
                cmdResults.Add(runResult);

                DisplayResult(runResult, itmGrp.Key, tstMethodLine);
                isSucc = runResult.IsAllSucceeded;

                if (!isSucc)
                    break;
            }
            if (isSucc && cmdResults.Any())
                isSucc &= CalculateCoverage(tfsVariables, setting, cmdResults);

            WriteDetail("All test methods runned", watch);
            return isSucc;
        }

        private void DisplayResult(VstestConsoleParser runResult, string assemblyName, string unitTestMethodNames)
        {
            WriteDetail($"These results are for [{assemblyName}] assembly's [{unitTestMethodNames}] methods : ");

            foreach (var item in runResult.Warnings)
            {
                WriteDetail(item);
            }
            foreach (var item in runResult.Result)
            {
                WriteDetail(item.MethodName + " => run is " + item.Message);
            }
            string lastMsg = runResult.IsAllSucceeded ? "Overall partial unit test succedded" : "Overall partial unit test failed";
            WriteDetail(lastMsg);
        }

        private bool CalculateCoverage(TfsVariable tfsVariables, PartialUnitTestSetting setting, List<VstestConsoleParser> cmdResults)
        {
            try
            {
                foreach (var itmOut in cmdResults)
                {
                    WriteDetail($"[{Path.GetFileName(itmOut.CoverageFilePath)}] file is starting to read.");
                    using (var coverInfo = CoverageInfo.CreateFromFile(itmOut.CoverageFilePath))
                    {
                        var dllName = itmOut.AssemblyName.ToLowerInvariant();
                        foreach (var module in coverInfo.Modules)
                        {
                            if (module.Name != dllName)
                                continue;

                            byte[] coverageBuffer = module.GetCoverageBuffer(null);
                            using (var reader = module.Symbols.CreateReader())
                            {
                                uint methodId;
                                string mthdName, undecoratedMthd, className, namespaceName;

                                var lines = new List<BlockLineRange>();

                                while (reader.GetNextMethod(out methodId, out mthdName, out undecoratedMthd, out className, out namespaceName, lines))
                                {
                                    var stats = CoverageInfo.GetMethodStatistics(coverageBuffer, lines);
                                    string tmpUnitName = namespaceName + "." + className + "." + mthdName;

                                    string mLine = $"Covered/NotCovered => Block - Line : {stats.BlocksCovered} / {stats.BlocksNotCovered} - {stats.LinesCovered} / {stats.LinesNotCovered} => {tmpUnitName} ";
                                    WriteDetail(mLine);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteDetail($"Coverage calculation failed : {ex}");
            }

            return false;
        }

        private VstestConsoleParser RunMsUnitTestExe(string parameters, string dllName)
        {
            var exePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                          @"\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";
            if (!File.Exists(exePath))
            {
                WriteDetail("Unit test run exe could not found : " + exePath);
                return new VstestConsoleParser();
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = parameters,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                var proc = new Process
                {
                    StartInfo = processInfo
                };
                bool isStarted = proc.Start();
                var sb = new StringBuilder();
                while (!proc.StandardOutput.EndOfStream)
                {
                    var tmp = proc.StandardOutput.ReadLine();
                    sb.AppendLine(tmp);
                }
                var line = sb.ToString();
                WriteDetail("Exe Output : \n" + line);
                return new VstestConsoleParser(proc.ExitCode, line, dllName);
            }
            catch (Exception ex)
            {
                WriteDetail($"Unit test exe run failed : {ex.ToString()}");
            }

            return new VstestConsoleParser();
        }

        private List<UnitTestDetail> CheckforUnitTest(PartialUnitTestSetting setting, Solution solution, TfsVariable tfsVariables, params MethodUnitTestCollection[] bags)
        {
            int totalFailCount = 0;
            List<UnitTestDetail> lst = new List<UnitTestDetail>();
            foreach (var queuDepo in bags)
            {
                foreach (var itmDoc in queuDepo)
                {
                    int tmpFail = 0;
                    foreach (var itmMethod in itmDoc.Methods)
                    {
                        IMethodSymbol method = itmDoc.Doc.GetSemanticModelAsync().Result.GetDeclaredSymbol(itmMethod);
                        var unitTest = FindUnitTestReferences(solution, method, tfsVariables);
                        if ((unitTest == null) || !unitTest.Any())
                        {
                            WriteDetail(string.Format("[{0}] method test is not found!!!", method.Name));
                            ++tmpFail;
                            if (totalFailCount > setting.MaxLogCount)
                                return null;
                        }
                        else
                        {
                            lst.AddRange(unitTest);
                        }
                    }
                    totalFailCount += tmpFail;
                    if (tmpFail == 0)
                        WriteDetail(string.Format("[{0}] project's all test are found", itmDoc.Doc.Project.Name));
                }
            }

            return totalFailCount == 0 ? lst : null;
        }

        private MethodUnitTestCollection GetChangeforChangedAdded(List<TFSFileState> codesAdded, TfsVariable tfsVariables, Solution solution)
        {
            var depo = new MethodUnitTestCollection();
            if (!codesAdded.Any())
                return depo;

            foreach (var itmChanged in codesAdded)
            {
                var webPath = itmChanged.FilePath.Replace("/", "\\").TrimStart('\\');
                var localVersionFullPath = Path.Combine(tfsVariables.BuildSourceDirectory, webPath);
                Document doc = null;
                foreach (var tmp in solution.Projects)
                {
                    doc = tmp.Documents.FirstOrDefault(x => x.FilePath == localVersionFullPath);
                    if (doc != null)
                        break;
                }
                if (doc == null)
                {
                    WriteDetail(string.Format("(Added) No project document found for : {0}", localVersionFullPath));
                    return null;
                }

                var tmpMethods = GetAllPublicMethods(doc);
                if (tmpMethods != null)
                    depo.Add(doc, tmpMethods);
            }

            return depo;
        }

        private MethodUnitTestCollection GetChangeforChangedFiles(List<TFSFileState> changedFiles, TfsVariable tfsVariables, Solution solution)
        {
            var depo = new MethodUnitTestCollection();
            if (!changedFiles.Any())
                return depo;

            foreach (var itmChanged in changedFiles)
            {
                var webPath = itmChanged.FilePath.Substring(1).Replace("/", "\\");
                var serverVersion = TFSHelper.DownloadFile(webPath);
                serverVersion = string.IsNullOrEmpty(serverVersion) ? serverVersion : serverVersion.Replace("\r\n", "\n");
                var localVersionFullPath = Path.Combine(tfsVariables.BuildSourceDirectory, webPath);
                var report = GetFileChanges(serverVersion, localVersionFullPath);
                Document doc = null;
                foreach (var tmp in solution.Projects)
                {
                    doc = tmp.Documents.FirstOrDefault(x => x.FilePath == localVersionFullPath);
                    if (doc != null)
                        break;
                }
                if (doc == null)
                {
                    WriteDetail(string.Format("(Changed) No project document found for : {0}", localVersionFullPath));
                    return null;
                }

                var tmpMethods = GetChangedPublicMethods(doc, report);
                if (tmpMethods != null)
                    depo.Add(doc, tmpMethods);
            }

            return depo;
        }

        private Solution GetSolutionInfo(TfsVariable tfsVariables, PartialUnitTestSetting setting, TFSFileState itmChanged)
        {
            bool isSolutionSelected = !string.IsNullOrEmpty(tfsVariables.SolutiontoBuild);
            string slnPath = null;
            string[] slnFiles = isSolutionSelected
                ? SolutionHelper.FindSolutionFiles(tfsVariables.BuildSourceDirectory, tfsVariables.SolutiontoBuild)
                : SolutionHelper.FindSolutionFiles(tfsVariables.BuildSourceDirectory);

            if (isSolutionSelected)
                slnPath = slnFiles.FirstOrDefault();
            else
            {
                foreach (var slnFullPath in slnFiles)
                {
                    string mainFolderName = Path.GetDirectoryName(slnFullPath);
                    var mainFolder = new DirectoryInfo(mainFolderName);
                    var repoFolder = mainFolder.Parent.FullName;
                    string tmpFile = repoFolder + itmChanged.FilePath;
                    if (File.Exists(tmpFile))
                    {
                        slnPath = slnFullPath;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(slnPath))
                    return null;
            }
            MSBuildWorkspace workSpace = MSBuildWorkspace.Create();
            var solution = workSpace.OpenSolutionAsync(slnPath).Result;
            return solution;
        }

        private static ArrayList GetFileChanges(string source, string destinationPath)
        {
            DiffList_TextFile sourceFile = new DiffList_TextFile(source.Split('\n'));
            DiffList_TextFile destinationFile = new DiffList_TextFile(destinationPath);
            DiffEngine diffEngine = new DiffEngine();
            diffEngine.ProcessDiff(sourceFile, destinationFile, DiffEngineLevel.FastImperfect);

            ArrayList differenceList = diffEngine.DiffReport();
            return differenceList;
        }

        private static List<MethodDeclarationSyntax> GetAllPublicMethods(Document doc)
        {
            var syntaxTree = doc.GetSemanticModelAsync().Result.SyntaxTree;
            bool isTestClass = IsThisTestClass(syntaxTree);
            if (isTestClass)
                return null;
            var root = syntaxTree.GetRoot();

            var mdsList = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(md => md.Modifiers.Any(SyntaxKind.PublicKeyword));

            return mdsList.ToList();
        }

        private static bool IsThisTestClass(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();
            var classInfo = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            if (HasAttibute(classInfo.AttributeLists, "TestClass"))
                return true;

            return false;
        }

        private static List<MethodDeclarationSyntax> GetChangedPublicMethods(Document doc, ArrayList changes)
        {
            List<MethodDeclarationSyntax> mdsList = new List<MethodDeclarationSyntax>();
            var syntaxTree = doc.GetSemanticModelAsync().Result.SyntaxTree;
            bool isTestClass = IsThisTestClass(syntaxTree);
            if (isTestClass)
                return null;

            DiffResultSpanStatus changeType = DiffResultSpanStatus.NoChange;
            int sourceIndex = 0;
            int destIndex = 0;
            int changeLength = 0;
            int changedLineStart = 0;
            int changedLineEnd = 0;

            foreach (DiffResultSpan change in changes)
            {
                changeType = change.Status;
                sourceIndex = change.SourceIndex + 1;
                destIndex = change.DestIndex + 1;
                changeLength = change.Length;
                changedLineStart = changeType.Equals(DiffResultSpanStatus.DeleteSource) ? sourceIndex : destIndex;
                changedLineEnd = changedLineStart + changeLength - 1;

                var mds = GetMethodBetweenLines(syntaxTree, changedLineStart, changedLineEnd);

                if (mds != null)
                {
                    mdsList.AddRange(mds);
                }
            }


            return mdsList;
        }

        private static MethodDeclarationSyntax GetMethodFromLine(SyntaxTree syntaxTree, int lineNumber)
        {
            var line = syntaxTree.GetText().Lines
                .FirstOrDefault(l => l.LineNumber == lineNumber);
            int spnStart = line.Span.Start;
            int spnEnd = line.Span.End;

            var root = syntaxTree.GetRoot();
            var mds = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(md => md.Modifiers.Any(SyntaxKind.PublicKeyword)
                                                && md.FullSpan.Start <= spnStart
                                                && spnEnd <= md.FullSpan.End);

            return mds;

        }

        private static List<MethodDeclarationSyntax> GetMethodBetweenLines(SyntaxTree syntaxTree, int lineNumberStart, int lineNumberEnd)
        {
            var lineStart = syntaxTree.GetText().Lines
                .FirstOrDefault(l => l.LineNumber == lineNumberStart);

            var lineEnd = syntaxTree.GetText().Lines
               .FirstOrDefault(l => l.LineNumber == lineNumberEnd);

            int spnStart = lineEnd.Span.Start;
            int spnEnd = lineStart.Span.End;

            var root = syntaxTree.GetRoot();
            var mds = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(md => md.Modifiers.Any(SyntaxKind.PublicKeyword)
                                                && md.FullSpan.Start <= spnStart
                                                && spnEnd <= md.FullSpan.End);

            return mds.ToList();

        }

        private static List<UnitTestDetail> FindUnitTestReferences(Solution solution, IMethodSymbol method, TfsVariable tfsVariables)
        {
            var methodReferences = SymbolFinder.FindReferencesAsync(method, solution).Result.ToList();

            if (!methodReferences.Any())
                return null;

            var depo = new List<UnitTestDetail>();

            foreach (ReferencedSymbol methodReference in methodReferences)
            {
                IEnumerable<ReferenceLocation> locations = methodReference.Locations;
                foreach (ReferenceLocation location in locations)
                {
                    MethodDeclarationSyntax referenceMds = GetMethodFromLine(location.Document.GetSemanticModelAsync().Result.SyntaxTree, location.Location.GetMappedLineSpan().StartLinePosition.Line);

                    if (referenceMds != null)
                    {
                        ClassDeclarationSyntax referenceCds = referenceMds.Parent as ClassDeclarationSyntax;
                        bool isTestClass = HasAttibute(referenceCds.AttributeLists, "TestClass");
                        if (isTestClass)
                        {
                            bool isTestMethod = HasAttibute(referenceMds.AttributeLists, "TestMethod");
                            if (isTestMethod)
                            {
                                string rightDll = FindRightOutputDll(location.Document.Project.OutputFilePath, tfsVariables);
                                depo.Add(
                                    new UnitTestDetail(referenceMds.Identifier.Text, rightDll)
                                    );
                            }
                        }
                    }

                }
            }

            return depo;
        }

        private static string FindRightOutputDll(string outputFilePath, TfsVariable tfsVariables)
        {
            string buildMode = string.IsNullOrEmpty(tfsVariables.BuildConfiguration)
                ? ""
                : tfsVariables.BuildConfiguration.ToLowerInvariant();

            if (buildMode == "release")
            {
                return outputFilePath.Replace(@"\bin\Debug\", @"\bin\Release\");
            }

            return outputFilePath;
        }

        private static bool HasAttibute(SyntaxList<AttributeListSyntax> attributeLists, string attibuteName)
        {
            return attributeLists
                .Any(al => al.Attributes
                        .Any(a => a.Name is IdentifierNameSyntax && (((IdentifierNameSyntax)a.Name).Identifier.Text == attibuteName)));
        }
    }
}
