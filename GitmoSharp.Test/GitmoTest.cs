using System;
using IO = System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitmoSharp;
using System.Threading.Tasks;

namespace GitmoSharp.Test
{
    [TestClass]
    public class GitmoTest
    {
        private string[] paths = { 
                                     "Test/NotInitialized", //0
                                      "Test/Initialized",   //1
                                      "Test/InitializedWithCommit", //2
                                      "Test/InitializedA", //3
                                      "Test/InitializedB", //4
                                      "Test/InitializedA2", //5
                                      "Test/InitializedB2", //6
                                      "Test"
                                 };
        [TestInitialize]
        public void TestInit()
        {
            foreach (var path in paths) {
                IO.Directory.CreateDirectory(path);

                if (path.StartsWith("Test/Initialized")) {
                    Gitmo.Init(path);
                }
            }
        }

        [TestCleanup]
        public void TestClean()
        {
            foreach (var path in paths) {
                if (IO.Directory.Exists(path)) {
                    try {
                        IO.Directory.Delete(path, true);
                    }
                    catch (Exception ex) { }
                }
            }
        }

        [TestMethod]
        public void TestHasChanges()
        {
            string rep = paths[4];
            Gitmo git = new Gitmo(rep);
            Write(rep, "dirty.txt", "drrrty");

            Assert.IsTrue(git.HasChanges);
        }

        [TestMethod]
        public void TestFetchWithUntrackedChanges()
        {
            string repoA = paths[3];
            string repoB = paths[4];

            // first, we set up the repositories
            Gitmo gitA = new Gitmo(repoA);
            Write(repoA, "first.txt", "first Content");
            gitA.CommitChanges("initial"); // first commit, to ensure that the master branch exists

            Gitmo gitB = new Gitmo(repoB);
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


        [TestMethod]
        public void TestFetchWithUncommitedModifiedChanges()
        {
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
            Assert.IsTrue(IO.File.Exists(IO.Path.Combine(repositoryPath, filename)));
        }
        private static void AssertFileContent(string repositoryPath, string filename, string expectedContent)
        {
            string path = IO.Path.Combine(repositoryPath, filename);
            string actualContent = IO.File.ReadAllText(path);
            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public void TestZipDirectory()
        {
            string repositoryPath = paths[1];

            Write(repositoryPath, "somefile.txt", "somecontent");

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            Assert.IsTrue(IO.File.Exists("Test/theid.zip"));
        }

        [TestMethod]
        public void TestResetZipConfig()
        {
            string repositoryPath = paths[1];

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "someOtherfile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid_clean";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            string configFile = g.ResetZipConfig(archiveID, outPath);

            Assert.IsFalse(IO.File.Exists(configFile));
        }


        [TestMethod]
        public void TestResetZipConfig_withRebuild()
        {
            string repositoryPath = paths[1];

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "someOtherfile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);

            string archiveID = "theid_clean_withrebuild";
            string relativePathInRepository = ""; // whole thing
            string outPath = "Test";

            g.Zip(archiveID, relativePathInRepository, outPath);

            string configFile = g.ResetZipConfig(archiveID, outPath);


            bool wasRebuilt = g.Zip(archiveID, relativePathInRepository, outPath);

            Assert.IsTrue(wasRebuilt, "Archive wasn't rebuilt after resetting the config");
            Assert.IsTrue(IO.File.Exists(configFile), "Config File wasn't recreated");
        }

        [TestMethod]
        public void TestZipDirectory_WithCommit()
        {
            string repositoryPath = paths[2];
            IO.Directory.CreateDirectory(IO.Path.Combine(repositoryPath, "somedir"));

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "somedir", "afile.txt"), DateTime.Now.ToString());

            Gitmo g = new Gitmo(repositoryPath);
            g.CommitChanges("a file commit");
            
            string archiveID = "theid";
            string relativePathInRepository = "somedir"; // whole thing
            string outPath = "Test/out";

            bool didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.IsTrue(didCreateZip, "first zip not made");

            didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.IsFalse(didCreateZip, "second zip attempt still made a zip");

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "somedir", "asecondfile.txt"), DateTime.Now.ToString());
            Task.Delay(1000).Wait(); // required delay to make sure the second commit has a different timestamp;
            g.CommitChanges("second commit");
            didCreateZip = g.Zip(archiveID, relativePathInRepository, outPath);
            Assert.IsTrue(didCreateZip, "did not create the zip after the second try");

            Assert.IsTrue(IO.File.Exists("Test/out/theid.zip"));
        }

        [TestMethod]
        public void TestIsValid()
        {
            bool result = Gitmo.IsValid(paths[0]);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestIsValid_True()
        {
            string path = paths[1];
            Assert.IsTrue(Gitmo.IsValid(path));
        }
    }
}
