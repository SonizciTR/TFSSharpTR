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

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    public class PartialUnitTestTask : BaseTask<PartialUnitTestSetting>
    {
        private int failCount = 0;

        public override TaskStatu Job(TfsVariable tfsVariables, UserVariable<PartialUnitTestSetting> usrVariables)
        {
            var setting = usrVariables.SettingFileData;
            if (setting == null)
                return new TaskStatu("PUT01", "No setting found.");

            var changedAll = TFSHelper.ChangedFiles();
            var codes = changedAll.Where(x => x.FilePath.EndsWith(".cs"));
            var codesChanged = codes.Where(x => x.State == SourceControlFileState.Changed).ToList();
            var codesAdded = codes.Where(x => x.State == SourceControlFileState.Added).ToList();
            if (codesChanged.Count + codesAdded.Count < 1)
                return new TaskStatu("No changes found to check for partial unit test");

            var tmpFile = codesChanged.Any() ? codesChanged[0] : codesAdded[0];
            Solution gSolution = GetSolutionInfo(tfsVariables, setting, tmpFile);
            if (gSolution == null)
                return new TaskStatu("PUT03", "No suitable solution found");

            var tmpMethodsforChanged = GetChangeforChangedFiles(codesChanged, tfsVariables, gSolution);
            if (tmpMethodsforChanged == null)
                return new TaskStatu("PUT04", "No suitable project document found for changed list");
            var tmpMethodsforAdded = GetChangeforChangedAdded(codesChanged, tfsVariables, gSolution);
            if (tmpMethodsforAdded == null)
                return new TaskStatu("PUT05", "No suitable project document found for added list");

            bool isUnitTestOk = CheckforUnitTest(setting, gSolution, tmpMethodsforChanged, tmpMethodsforAdded);

            return isUnitTestOk ? new TaskStatu("Partial Unit Test check successful") : new TaskStatu("PUT06", "Partial Unit Test  failed");
        }

        private bool CheckforUnitTest(PartialUnitTestSetting setting, Solution solution, params MethodUnitTestCollection[] bags)
        {
            foreach (var queuDepo in bags)
            {
                foreach (var itmDoc in queuDepo)
                {
                    foreach (var itmMethod in itmDoc.Methods)
                    {
                        IMethodSymbol method = itmDoc.Doc.GetSemanticModelAsync().Result.GetDeclaredSymbol(itmMethod);
                        bool unitTestExist = FindUnitTestReferences(solution, method);
                        if (!unitTestExist)
                        {
                            WriteDetail(string.Format("[{0}] method test is not found!!!", method.Name));
                            ++failCount;
                            if (failCount > setting.MaxLogCount)
                                return false;
                        }
                    }
                    WriteDetail(string.Format("[{0}] project's all test are found", itmDoc.Doc.Project.Name));
                }
            }

            return failCount == 0;
        }

        private MethodUnitTestCollection GetChangeforChangedAdded(List<TFSFileState> codesAdded, TfsVariable tfsVariables, Solution solution)
        {
            var depo = new MethodUnitTestCollection();
            if (!codesAdded.Any())
                return depo;

            foreach (var itmChanged in codesAdded)
            {
                var webPath = itmChanged.FilePath.Replace("/", "\\");
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
                var webPath = itmChanged.FilePath.Replace("/", "\\");
                var serverVersion = TFSHelper.DownloadFile(webPath);
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
                depo.Add(doc, tmpMethods);
            }

            return depo;
        }

        private Solution GetSolutionInfo(TfsVariable tfsVariables, PartialUnitTestSetting setting, TFSFileState itmChanged)
        {
            bool isSolutionSelected = !string.IsNullOrEmpty(tfsVariables.SolutiontoBuild);

            string[] slnFiles = isSolutionSelected ? new string[] { tfsVariables.SolutiontoBuild }
            : SolutionHelper.FindSolutionFiles(tfsVariables.BuildSourceDirectory);

            foreach (var slnFullPath in slnFiles)
            {
                string slnPath = Directory.GetFiles(slnFullPath, tfsVariables.SolutiontoBuild, SearchOption.AllDirectories).FirstOrDefault();
                if (slnPath == null)
                    continue;

                MSBuildWorkspace workSpace = MSBuildWorkspace.Create();
                var solution = workSpace.OpenSolutionAsync(slnPath).Result;
                return solution;
            }
            return null;
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
            var root = syntaxTree.GetRoot();

            var mdsList = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(md => md.Modifiers.Any(SyntaxKind.PublicKeyword));

            return mdsList.ToList();
        }

        private static List<MethodDeclarationSyntax> GetChangedPublicMethods(Document doc, ArrayList changes)
        {
            List<MethodDeclarationSyntax> mdsList = new List<MethodDeclarationSyntax>();
            var syntaxTree = doc.GetSemanticModelAsync().Result.SyntaxTree;

            DiffResultSpanStatus changeType = DiffResultSpanStatus.NoChange;
            int sourceIndex = 0;
            int destIndex = 0;
            int changeLength = 0;
            int changedLine = 0;

            foreach (DiffResultSpan change in changes)
            {
                changeType = change.Status;
                sourceIndex = change.SourceIndex + 1;
                destIndex = change.DestIndex + 1;
                changeLength = change.Length;
                changedLine = changeType.Equals(DiffResultSpanStatus.DeleteSource) ? sourceIndex : destIndex;

                var mds = GetMethodFromLine(syntaxTree, changedLine);

                if (mds != null)
                {
                    mdsList.Add(mds);
                }
            }


            return mdsList;
        }

        private static MethodDeclarationSyntax GetMethodFromLine(SyntaxTree syntaxTree, int lineNumber)
        {
            Microsoft.CodeAnalysis.Text.TextLine line = syntaxTree.GetText().Lines
                .Where(l => l.LineNumber == lineNumber)
                .FirstOrDefault();
            int spnStart = line.Span.Start;
            int spnEnd = line.Span.End;

            var root = syntaxTree.GetRoot();
            var mds = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(md => md.Modifiers.Any(SyntaxKind.PublicKeyword))
                        .Where(md => md.FullSpan.Start <= spnStart && spnEnd <= md.FullSpan.End)
                        .FirstOrDefault();

            return mds;

        }

        private static bool FindUnitTestReferences(Solution solution, IMethodSymbol method)
        {
            IEnumerable<ReferencedSymbol> methodReferences = SymbolFinder.FindReferencesAsync(method, solution).Result;

            //no reference
            if (methodReferences.Count().Equals(0))
                return false;

            ReferencedSymbol methodReference = methodReferences.Single();
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
                            return true;
                    }
                }

            }

            return false;
        }

        private static bool HasAttibute(SyntaxList<AttributeListSyntax> attributeLists, string attibuteName)
        {
            return attributeLists
                .Any(al => al.Attributes
                        .Any(a => a.Name is IdentifierNameSyntax && (((IdentifierNameSyntax)a.Name).Identifier.Text == attibuteName)));
        }
    }
}