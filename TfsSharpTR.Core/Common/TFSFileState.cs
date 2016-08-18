using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Core.Common
{
    public enum SourceControlFileState
    {
        Added = 0,
        Changed = 1,
        Deleted = 2
    }

    public class TFSFileState
    {
        public string FilePath { get; set; }
        public SourceControlFileState State { get; set; }

        public TFSFileState()
        {
        }

        public TFSFileState(string path, SourceControlFileState state)
        {
            FilePath = path;
            State = state;
        }
    }

    internal static class FileStateExtension
    {
        public static SourceControlFileState ToSourceControlFileState(this VersionControlChangeType source)
        {
            if (source == VersionControlChangeType.Add)
                return SourceControlFileState.Added;

            if (source == VersionControlChangeType.Delete)
                return SourceControlFileState.Deleted;

            if (source == (VersionControlChangeType.SourceRename | VersionControlChangeType.Delete))
                return SourceControlFileState.Deleted;

            return SourceControlFileState.Changed;
        }

        public static SourceControlFileState ToSourceControlFileState(this PendingChange source)
        {
            if (source.IsAdd)
                return SourceControlFileState.Added;
            if (source.IsDelete)
                return SourceControlFileState.Deleted;

            return SourceControlFileState.Changed;
        }
    }
}
