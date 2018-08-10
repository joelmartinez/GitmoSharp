using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

using LibGit2Sharp;
using Octokit;

namespace GitmoSharp {
    /// <summary>This library includes methods for an automated process to work with updating a remote repository. This includes making archives of 
    /// certain folders.</summary>
    public class Gitmo : IDisposable {
        private string rootPath;
        private LibGit2Sharp.Repository repository;

        public string Name { get; set; }
        public string Email { get; set; }

        public bool HasChanges
        {
            get
            {
                RepositoryStatus status = repository.RetrieveStatus();
                return status.IsDirty;
            }
        }

        public Gitmo(string path, string name, string email)
        {
            this.Name = name;
            this.Email = email;

            CoreInitializeClass (path);
        }

        /// <summary>Initializes Gitmo to be able to operate on a git repository. </summary>
        /// <param name="path">The root path to the git repository in question. Must be a valid git repo.</param>
        [Obsolete("You should really be using the overload to provide the name/email")]
        public Gitmo(string path)
        {
            CoreInitializeClass (path);
        }

        private void CoreInitializeClass (string path)
        {
            rootPath = path;

            ValidatePath ();
            ValidateSignature ();

            repository = new LibGit2Sharp.Repository (path);
        }

        private void ValidateSignature()
        {
            this.Name = string.IsNullOrWhiteSpace(this.Name) ? "GitmoSharp" : this.Name;
            this.Email = string.IsNullOrWhiteSpace(this.Email) ? "auto_gitmo@fakeemail.com" : this.Email;
        }

        /// <summary>Creates, or updates a zip archive of the folder in a git repository.</summary>
        /// <param name="id">A unique identifier for this archive.</param>
        /// <param name="relativePathToZip">The relative path inside the git repo that you want to create an archive for.</param>
        /// <param name="outPath">The output path for the zip file (and a related meta file). This should not be a directory in 
        /// the git repository.</param>
        public bool Zip(string id, string relativePathToZip, string outPath)
        {
            string pathToZip = IO.Path.Combine(rootPath, relativePathToZip);

            var files = IO.Directory
                .GetFiles(pathToZip, "*", IO.SearchOption.AllDirectories)
                .Select(f => new IO.FileInfo(f).LastWriteTimeUtc)
                .OrderByDescending(t => t);
            var latestFileUpdated = files
                .FirstOrDefault();

            //Commit latestCommit = repository.Commits.LatestCommitFor(relativePathToZip);
            DateTimeOffset lastUpdated = DateTimeOffset.Now;
            if (latestFileUpdated != null) {
                lastUpdated = latestFileUpdated;
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

            var options = new FetchOptions() { TagFetchMode = TagFetchMode.None };
            if (!string.IsNullOrWhiteSpace(username)) {
                var creds = new UsernamePasswordCredentials { Username = username, Password = password };
                options.CredentialsProvider = (_url, _user, _cred) => creds;
                repository.Network.Fetch(remote.Name, refspecs:new string[0], options: options);
            }
            else {
                repository.Network.Fetch(remote.Name, refspecs: new string[0], options:options);
            }

            var remoteBranch = repository.Branches.Single(b => b.FriendlyName == string.Format("{0}/{1}", remoteName, branch));
            CheckoutOptions checkoutoptions = new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.None };
            repository.Checkout(remoteBranch.Commits.First().Tree, new string[] { "*" }, checkoutoptions);
            repository.Refs.UpdateTarget(repository.Refs.Head, remoteBranch.Tip.Id);
        }

        /// <summary>Resets the repository to the HEAD commit, and also deletes untracked files</summary>
        public void Reset()
        {
            repository.Reset(ResetMode.Hard);
            var status = repository.RetrieveStatus();
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
            Commands.Stage(repository, "*");

            this.ValidateSignature();
            LibGit2Sharp.Signature sig = new LibGit2Sharp.Signature (this.Name, this.Email, DateTimeOffset.Now);
            repository.Commit(message, sig, sig);
        }

        /// <summary>Adds a remote if it is missing. No-op if it's already there.</summary>
        public void AddRemote(string remoteName, string remoteLocation)
        {
            if (!repository.Network.Remotes.Any(r => r.Name == remoteName)) {
                repository.Network.Remotes.Add(remoteName, remoteLocation);
            }
        }

        /// <summary>
        /// Opens the github pull request.
        /// </summary>
        /// <returns>The URL to the opened pull request</returns>
        /// <param name="owner">the org or username</param>
        /// <param name="reponame">name of the repository</param>
        /// <param name="branchname">The branch to base the PR on.</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password ... this can (should) be a private access token</param>
        /// <param name="prTitle">The title</param>
        /// <param name="prBody">Markdown for the body of the PR</param>
        public async Task<string> OpenGithubPullRequestAsync(string owner, string reponame, string branchname, string username, string password, string prTitle, string prBody)
        {
            var github = new GitHubClient (new ProductHeaderValue ("GitmoSharp"));
            github.Credentials = new Octokit.Credentials (username, password); ;
            ApiConnection apiConnection = new ApiConnection (github.Connection);

            var repo = await github.Repository.Get (owner, reponame);

            var prDetails = new NewPullRequest (prTitle, branchname, "master");
            prDetails.Body = prBody;

            PullRequestsClient client = new PullRequestsClient (apiConnection);
            var pr = await client.Create (repo.Id, prDetails);
            return pr.Url;
        }

        /// <summary>Checks to see whether the path is a valid git repository.</summary>
        public static bool IsValid(string path)
        {
            return LibGit2Sharp.Repository.IsValid(path);
        }

        /// <summary>Initializes the path into a git repository.</summary>
        public static void Init(string path)
        {
            LibGit2Sharp.Repository.Init(path);
        }

        public static void DeleteRepository(string path)
        {
            DeleteReadOnlyDirectory(path);
        }
        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        /// <remarks>This method sourced originally from: https://stackoverflow.com/a/26372070</remarks>
        private static void DeleteReadOnlyDirectory(string directory)
        {
            foreach (var subdirectory in IO.Directory.EnumerateDirectories(directory))
            {
                DeleteReadOnlyDirectory(subdirectory);
            }
            foreach (var fileName in IO.Directory.EnumerateFiles(directory))
            {
                var fileInfo = new IO.FileInfo(fileName);
                fileInfo.Attributes = IO.FileAttributes.Normal;
                fileInfo.Delete();
            }
            IO.Directory.Delete(directory);
        }

        public void Dispose()
        {
            repository.Dispose();
        }

        private void ValidatePath()
        {
            if (string.IsNullOrWhiteSpace(rootPath)) {
                throw new ArgumentNullException(nameof(rootPath));
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
