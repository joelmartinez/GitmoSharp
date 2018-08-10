using System;
using IO = System.IO;
using GitmoSharp;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GitmoSharp.Test
{
    public class GitmoTest
    {
        private static object _object = new object();

        private string testpath = IO.Path.Combine(IO.Path.GetTempPath(), "GitmoTests");

        private string[] basepaths = { 
                                     "Test\\NotInitialized", //0
                                      "Test\\Initialized",   //1
                                      "Test\\InitializedWithCommit", //2
                                      "Test\\InitializedA", //3
                                      "Test\\InitializedB", //4
                                      "Test\\InitializedA2", //5
                                      "Test\\InitializedB2", //6
                                      "Test"
                                 };
 
        private string[] InitializeGitmoTest([CallerMemberName]string memberName = "")
        {
            char otherSep = IO.Path.DirectorySeparatorChar == '/' ? '\\' : '/';

            //Monitor.Enter(_object);
            var pathsToUse = basepaths
                .Select (p => IO.Path.Combine (testpath, memberName, p))
                .Select (p => p.Replace (otherSep, IO.Path.DirectorySeparatorChar))
                .ToArray();

            try {
                string testdir = IO.Path.Combine(testpath, memberName, "Test");
                if (IO.Directory.Exists(testdir)) Gitmo.DeleteRepository(testdir);
            }
            catch (UnauthorizedAccessException uaex)
            {
                throw new ApplicationException($"error cleaning: {uaex.Message} for Test", uaex);
            }

            foreach (var path in pathsToUse) {

                try
                {
                    IO.Directory.CreateDirectory(path);
                }
                catch (UnauthorizedAccessException uaex)
                {
                    throw new ApplicationException($"error creating: {uaex.Message} for {path}", uaex);
                }
                if (path.StartsWith(IO.Path.Combine(testpath, memberName, "Test\\Initialized".Replace (otherSep, IO.Path.DirectorySeparatorChar)), StringComparison.Ordinal)) {
                    Gitmo.Init(path);
                }
            }
            return pathsToUse;
        }


        [Fact]
        public void TestHasChanges()
        {
            var paths = InitializeGitmoTest();
            string rep = paths[4];
            Gitmo git = new Gitmo(rep);
            Write(rep, "dirty.txt", "drrrty");

            Assert.True(git.HasChanges);
        }

        [Fact]
        public void TestFetchWithUntrackedChanges()
        {
            var paths = InitializeGitmoTest();
            string repoA = paths[3];
            string repoB = paths[4];

            // first, we set up the repositories
            using (Gitmo gitA = new Gitmo(repoA))
            using (Gitmo gitB = new Gitmo(repoB))
            {
                Write(repoA, "first.txt", "first Content");
                gitA.CommitChanges("initial"); // first commit, to ensure that the master branch exists
                
                gitB.AddRemote("repoA", repoA);
                gitB.FetchLatest(remoteName: "repoA");

                // now, we set up a situation for a merge issue
                Write(repoA, "a.txt", "A Content");
                gitA.CommitChanges("changes from A"); // we commit a change in repo A

                Write(repoB, "a.txt", "B Content"); // then write an uncommitted change to repo B
                gitB.FetchLatest(remoteName: "repoA");

                AssertFileExists(repoB, "a.txt");
                AssertFileContent(repoB, "a.txt", "A Content");
            }
        }


        [Fact]
        public void TestFetchWithUncommitedModifiedChanges()
        {
            var paths = InitializeGitmoTest();
            string repoA = paths[5];
            string repoB = paths[6];

            // first, we set up the repositories
            Gitmo gitA = new Gitmo(repoA);
            Write(repoA, "first.txt", "first Content");
            gitA.CommitChanges("initial"); // first commit, to ensure that the master branch exists

            Gitmo gitB = new Gitmo(repoB);
            gitB.AddRemote("repoA", repoA);
            gitB.FetchLatest(remoteName: "repoA");

            // now, we set up a situation for a merge issue

            Write(repoB, "first.txt", "B Content in first"); // then write an uncommitted change to repo B
            gitB.FetchLatest(remoteName: "repoA");

            AssertFileExists(repoB, "first.txt");
            AssertFileContent(repoB, "first.txt", "first Content");
        }

        private static void Write(string repositoryPath, string filename, string content)
        {
            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, filename), content);
        }

        private static void AssertFileExists(string repositoryPath, string filename)
        {
            Assert.True(IO.File.Exists(IO.Path.Combine(repositoryPath, filename)));
        }
        private static void AssertFileContent(string repositoryPath, string filename, string expectedContent)
        {
            string path = IO.Path.Combine(repositoryPath, filename);
            string actualContent = IO.File.ReadAllText(path);
            Assert.Equal(expectedContent, actualContent);
        }

        [Fact]
        public void TestZipDirectory()
        {
            var paths = InitializeGitmoTest();
            string repositoryPath = paths[1];

            Write(repositoryPath, "somefile.txt", "somecontent");

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            Assert.True(IO.File.Exists("Test/theid.zip"));
        }

        [Fact]
        public void TestResetZipConfig()
        {
            var paths = InitializeGitmoTest();
            string repositoryPath = paths[1];

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "someOtherfile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid_clean";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            string configFile = g.ResetZipConfig(archiveID, outPath);

            Assert.False(IO.File.Exists(configFile));
        }


        [Fact]
        public void TestResetZipConfig_withRebuild()
        {
            var paths = InitializeGitmoTest();
            string repositoryPath = paths[1];

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "someOtherfile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid_clean_withrebuild";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            string configFile = g.ResetZipConfig(archiveID, outPath);


            bool wasRebuilt = g.Zip(archiveID, relativePathInRepository, outPath);

            Assert.True(wasRebuilt, "Archive wasn't rebuilt after resetting the config");
            Assert.True(IO.File.Exists(configFile), "Config File wasn't recreated");
        }

        [Fact]
        public void TestZipDirectory_WithCommit()
        {
            var paths = InitializeGitmoTest();
            string repositoryPath = paths[2];
            IO.Directory.CreateDirectory(IO.Path.Combine(repositoryPath, "somedir"));

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "somedir", "afile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);
            g.CommitChanges("a file commit");
            
            string archiveID = "theid";
            string relativePathInRepository = "somedir"; // whole thing
            string outPath = "Test/out";

            bool didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.True(didCreateZip, "first zip not made");

            didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.False(didCreateZip, "second zip attempt still made a zip");

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "somedir", "asecondfile.txt"), DateTime.Now.ToString());
            Task.Delay(1000).Wait(); // required delay to make sure the second commit has a different timestamp;
            g.CommitChanges("second commit");
            didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.True(didCreateZip, "did not create the zip after the second try");

            Assert.True(IO.File.Exists("Test/out/theid.zip"));
        }

        [Fact]
        public void TestIsValid()
        {
            var paths = InitializeGitmoTest();
            bool result = Gitmo.IsValid(paths[0]);

            Assert.False(result);
        }

        [Fact]
        public void TestIsValid_True()
        {
            var paths = InitializeGitmoTest();
            string path = paths[1];
            Assert.True(Gitmo.IsValid(path));
        }
    }
}
