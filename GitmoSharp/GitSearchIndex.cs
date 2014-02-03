using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitmoSharp {
    public class GitHistoryIndex : Dictionary<string, GitNode> {
        bool isInitialized;

        public void AddCommit(string path, Commit commit)
        {
            if (!this.ContainsKey(path)) {
                this.Add(path, new GitNode());
            }
            Console.WriteLine("Adding to index: " + path);
            this[path].AddCommit(commit);
        }

        public void Initialize(Repository rep)
        {
            var allCommits = rep.Head.Commits;
            GitHistoryIndex index = this;

            Commit previous = null;
            foreach (var c in allCommits.Reverse()) {
                if (previous != null) {

                    TreeChanges changes = rep.Diff.Compare<TreeChanges>(previous.Tree, c.Tree);

                    foreach (var change in changes) {
                        index.AddCommit(change.Path, c);
                    }
                }
                else {
                    var tree = c.Tree;
                    ProcessTree(c, tree, index);
                }
                previous = c;
            }

            isInitialized = true;
        }

        private static void ProcessTree(Commit c, Tree tree, GitHistoryIndex index)
        {
            foreach (var entry in tree) {
                if (entry.TargetType == TreeEntryTargetType.Tree) {
                    ProcessTree(c, entry.Target as Tree, index);
                }
                else {
                    index.AddCommit(entry.Path, c);
                }
            }
        }
    }

    public class GitNode {

        public GitNode()
        {
            this.History = new List<Commit>();
            this.EarliestCommit = DateTimeOffset.MaxValue;
            this.LatestCommit = DateTime.MinValue;
        }
        public string Path { get; set; }
        public DateTimeOffset EarliestCommit { get; set; }
        public DateTimeOffset LatestCommit { get; set; }
        public List<Commit> History { get; set; }



        public void AddCommit(Commit commit)
        {
            if (EarliestCommit > commit.Author.When) {
                EarliestCommit = commit.Author.When;
            }
            if (LatestCommit < commit.Author.When) {
                LatestCommit = commit.Author.When;
            }
            History.Add(commit);
        }
    }
}
