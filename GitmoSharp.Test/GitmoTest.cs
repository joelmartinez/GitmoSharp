﻿using System;
using IO = System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitmoSharp;

namespace GitmoSharp.Test
{
    [TestClass]
    public class GitmoTest
    {
        private string[] paths = { 
                                     "Test/NotInitialized", //0
                                      "Test/Initialized",   //1
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
                IO.Directory.Delete(path, true);
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