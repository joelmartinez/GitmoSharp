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
        public void TestZipDirectory()
        {
            string repositoryPath = paths[1];

            IO.File.WriteAllText(IO.Path.Combine(repositoryPath, "somefile.txt"), "somecontent");

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
