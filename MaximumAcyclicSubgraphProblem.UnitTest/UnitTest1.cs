using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaximumAcyclicSubgraphProblem.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var s = new List<string> {"1", "2", "3", "4"};
            var d = new List<string> {"2", "3", "1", "1"};
            Assert.IsFalse(MaximumAcyclicSubgraphProblemForm.IsAcyclic(s, d));
        }

        [TestMethod]
        public void TestMethod2()
        {
            var s = new List<string> { "1", "2", "3" };
            var d = new List<string> { "2", "3", "4" };
            Assert.IsTrue(MaximumAcyclicSubgraphProblemForm.IsAcyclic(s, d));
        }

        [TestMethod]
        public void TestMethod3()
        {
            var s = new List<string> { "1", "2", "3", "4", "4" };
            var d = new List<string> { "2", "3", "4", "1", "2" };
            Assert.IsFalse(MaximumAcyclicSubgraphProblemForm.IsAcyclic(s, d));
        }
        [TestMethod]
        public void TestMethod4()
        {
            var s = new List<string> { "3", "2"};
            var d = new List<string> { "1", "3" };
            Assert.IsTrue(MaximumAcyclicSubgraphProblemForm.IsAcyclic(s, d));
        }
    }
}