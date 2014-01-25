using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

using LibGit2Sharp;

namespace GitmoSharp {
    public class Gitmo {
        private string rootPath;
        private Repository repository;

        public Gitmo(string path)
        {
            rootPath = path;

            ValidatePath();

            repository = new Repository(path);
        }

        public void Zip(string relativePath, string zipPath)
        {

        }

        public static bool IsValid(string path)
        {
            return Repository.IsValid(path);
        }

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
