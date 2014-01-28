using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

using LibGit2Sharp;

namespace GitmoSharp {
    /// <summary>Helper methods for Git repositories.</summary>
    public class Gitmo {
        private string rootPath;
        private Repository repository;

        /// <summary>Initializes Gitmo to be able to operate on a git repository. </summary>
        /// <param name="path">The root path to the git repository in question. Must be a valid git repo.</param>
        public Gitmo(string path)
        {
            rootPath = path;

            ValidatePath();

            repository = new Repository(path);
        }

        /// <summary>Creates, or updates a zip archive of the folder in a git repository.</summary>
        /// <param name="id">A unique identifier for this archive.</param>
        /// <param name="relativePathToZip">The relative path inside the git repo that you want to create an archive for.</param>
        /// <param name="outPath">The output path for the zip file (and a related meta file). This should not be a directory in 
        /// the git repository.</param>
        public void Zip(string id, string relativePathToZip, string outPath)
        {
            string pathToZip = IO.Path.Combine(rootPath, relativePathToZip);

            DateTimeOffset lastUpdated = DateTimeOffset.MinValue; // TODO: this should come from git history

            Zipper z = new Zipper(id, outPath);
            if (z.DoesArchiveRequireRebuilding(lastUpdated)) {
                z.WriteArchive(pathToZip);
            }
        }

        public void FetchLatest()
        {

        }

        /// <summary>Checks to see whether the path is a valid git repository.</summary>
        public static bool IsValid(string path)
        {
            return Repository.IsValid(path);
        }

        /// <summary>Initializes the path into a git repository.</summary>
        public static void Init(string path)
        {
            Repository.Init(path);
        }

        private void ValidatePath()
        {
            if (string.IsNullOrWhiteSpace(rootPath)) {
                throw new ArgumentNullException("path");
            }
            if (!IO.Directory.Exists(rootPath)) {
                throw new ArgumentException(string.Format("path doesn't exist: {0}", rootPath));
            }
            if (!IsValid(rootPath)) {
                throw new ArgumentException(string.Format("path is not a valid git repository: {0}", rootPath));
            }
        }
    }
}
