using System;
using IO = System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitmoSharp;

namespace GitmoSharp.Test
{
    [TestClass]
    public class GitmoTest
    {
        [TestInitialize]
        public void TestInit()
        {
            IO.Directory.CreateDirectory("Test/NotInitialized");


        }

        [TestMethod]
        public void TestIsValid()
        {
            bool result = Gitmo.IsValid("Tests/NotInitialized");

            Assert.IsFalse(result);
        }
    }
}
