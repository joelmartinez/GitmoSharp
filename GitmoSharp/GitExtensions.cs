using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitmoSharp {

    public static class CommitLogExtension {
        public static IEnumerable<Commit> PathFilter(this IEnumerable<Commit> log, string path)
        {
            if (string.IsNullOrEmpty(path))
                return log;

            return log.Where(s => {
                var pathEntry = s[path];
                var parent = s.Parents.FirstOrDefault();
                if (parent == null)
                    return pathEntry != null;

                var parentPathEntry = parent[path];
                if (pathEntry == null && parentPathEntry == null)
                    return false;
                else if (pathEntry != null && parentPathEntry != null)
                    return pathEntry.Target.Sha != parentPathEntry.Target.Sha;
                else // pathEntry!=null ^ parentPathEntry!=null
                    return true;
            });
        }

        /// <summary>Get's the latest commit for this path</summary>
        public static Commit LatestCommitFor(this IEnumerable<Commit> log, string path)
        {
            if (log == null) return null;

            if (string.IsNullOrEmpty(path))
                return log.FirstOrDefault();

            return log.FirstOrDefault(s => {
                var pathEntry = s[path];
                var parent = s.Parents.FirstOrDefault();
                if (parent == null)
                    return pathEntry != null;

                var parentPathEntry = parent[path];
                if (pathEntry == null && parentPathEntry == null)
                    return false;
                else if (pathEntry != null && parentPathEntry != null)
                    return pathEntry.Target.Sha != parentPathEntry.Target.Sha;
                else // pathEntry!=null ^ parentPathEntry!=null
                    return true;
            });
        }

        /// <summary>Get's the latest commit for this path</summary>
        public static Commit EarliestCommitFor(this IEnumerable<Commit> log, string path)
        {
            if (string.IsNullOrEmpty(path))
                return log.First();

            return log.Last(s => {
                var pathEntry = s[path];
                var parent = s.Parents.FirstOrDefault();
                if (parent == null)
                    return pathEntry != null;

                var parentPathEntry = parent[path];
                if (pathEntry == null && parentPathEntry == null)
                    return false;
                else if (pathEntry != null && parentPathEntry != null)
                    return pathEntry.Target.Sha != parentPathEntry.Target.Sha;
                else // pathEntry!=null ^ parentPathEntry!=null
                    return true;
            });
        }
    }
}
