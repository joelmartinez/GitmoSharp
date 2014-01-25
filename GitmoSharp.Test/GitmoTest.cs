using System;
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
                                      "Test/Initialized"    //1
                                 };
        [TestInitialize]
        public void TestInit()
        {
            foreach (var path in paths) {
                IO.Directory.CreateDirectory(path);
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
        public void TestIsValid()
        {
            bool result = Gitmo.IsValid(paths[0]);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestIsValid_True()
        {
            string path = paths[1];
            Gitmo.Init(path);
            Assert.IsTrue(Gitmo.IsValid(path));
        }
    }
}
