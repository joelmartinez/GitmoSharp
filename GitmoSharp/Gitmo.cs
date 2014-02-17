using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

using LibGit2Sharp;

namespace GitmoSharp {
    /// <summary>This library includes methods for an automated process to work with updating a remote repository. This includes making archives of 
    /// certain folders.</summary>
    public class Gitmo {
        private string rootPath;
        private Repository repository;

        public bool HasChanges
        {
            get
            {
                RepositoryStatus status = repository.Index.RetrieveStatus();
                return status.IsDirty;
            }
        }

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
        public bool Zip(string id, string relativePathToZip, string outPath)
        {
            string pathToZip = IO.Path.Combine(rootPath, relativePathToZip);

            Commit latestCommit = repository.Commits.LatestCommitFor(relativePathToZip);
            DateTimeOffset lastUpdated = DateTimeOffset.Now;
            if (latestCommit != null) {
                lastUpdated = latestCommit.Author.When;
            }

            Zipper z = new Zipper(id, outPath);
            if (z.DoesArchiveRequireRebuilding(lastUpdated)) {
                z.WriteArchive(pathToZip);
                return true;
            }

            return false;
        }

        /// <summary>Delete the zip file's configuration file. This ensures a rebuild next time Zip is called.</summary>
        /// <returns>The path to the configuration file that was deleted. It shouldn't exist at this point.</returns>
        public string ResetZipConfig(string id, string outpath)
        {
            Zipper z = new Zipper(id, outpath);
            if (IO.File.Exists(z.ConfigFilePath)) {
                IO.File.Delete(z.ConfigFilePath);
            }

            return z.ConfigFilePath;
        }

        /// <summary>Fetches the latest from the remote, and checks out the remote branch (ie. does not attempt to merge).</summary>
        /// <param name="remoteName">'origin' by default.</param>
        /// <param name="branch">'master' by default</param>
        /// <param name="username">null by default</param>
        /// <param name="password">null by default</param>
        public void FetchLatest(string remoteName="origin", string branch = "master", string username=null, string password=null)
        {
            if (HasChanges) {
                // uh oh ... something has gone awry. We should reset before attempting to fetch changes
                Reset();
            }

            var remote = repository.Network.Remotes[remoteName];

            if (!string.IsNullOrWhiteSpace(username)) {
                repository.Network.Fetch(remote, tagFetchMode: TagFetchMode.None, credentials: new Credentials { Username = username, Password = password });
            }
            else {
                repository.Network.Fetch(remote, tagFetchMode: TagFetchMode.None);
            }

            var remoteBranch = repository.Branches.Single(b => b.Name == string.Format("{0}/{1}", remoteName, branch));

            repository.Checkout(remoteBranch);
        }

        /// <summary>Resets the repository to the HEAD commit, and also deletes untracked files</summary>
        public void Reset()
        {
            repository.Reset(ResetMode.Hard);
            var status = repository.Index.RetrieveStatus();
            foreach (var file in status.Untracked) {
                string fpath = IO.Path.Combine(rootPath, file.FilePath);
                IO.File.Delete(fpath);
            }
        }

        /// <summary>Fetches the latest from the remote, and checks out the remote branch (ie. does not attempt to merge).</summary>
        /// <param name="remoteName">'origin' by default.</param>
        /// <param name="branch">'master' by default</param>
        /// <param name="username">null by default</param>
        /// <param name="password">null by default</param>
        public Task FetchLatestAsync(string remoteName = "origin", string branch = "master", string username = null, string password = null)
        {
            return Task.Factory.StartNew(() => FetchLatest(remoteName, branch, username, password));
        }

        /// <summary>Stages and Commits all pending changes.</summary>
        /// <param name="message">The comment message to include.</param>
        public void CommitChanges(string message)
        {
            repository.Index.Stage("*");
            repository.Commit(message);
        }

        /// <summary>Adds a remote if it is missing. No-op if it's already there.</summary>
        public void AddRemote(string remoteName, string remoteLocation)
        {
            if (!repository.Network.Remotes.Any(r => r.Name == remoteName)) {
                repository.Network.Remotes.Add(remoteName, remoteLocation);
            }
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
