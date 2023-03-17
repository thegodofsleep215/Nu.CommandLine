using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nu.CommandLine.Commands;

namespace Test.Nu.CommandLine
{
    [TestClass]
    public class TestUsage
    {
        protected Usage emptyParameters;
        protected Usage oneParameter;
        protected Usage twoParameters;
        protected Usage twoWithAnOptional;

        public void EmptyParameterMethod() { }
        public void OneParameterMethod(int i) { }
        public void TwoParametersMethod(int i, string s) { }
        public void TwoWithAnOptional(int i, string s="") { }

        public TestUsage()
        {
            emptyParameters = new Usage(new MethodInfoExecution(GetType().GetMethod("EmptyParameterMethod"), this));
            oneParameter = new Usage(new MethodInfoExecution(GetType().GetMethod("OneParameterMethod"), this));
            twoParameters = new Usage(new MethodInfoExecution(GetType().GetMethod("TwoParametersMethod"), this));
            twoWithAnOptional = new Usage(new MethodInfoExecution(GetType().GetMethod("TwoWithAnOptional"), this));
        }

        [TestClass]
        public class MatchParameters : TestUsage
        {
            [TestMethod]
            public void NoParametersWithEmpty()
            {
                Assert.IsTrue(emptyParameters.MatchesUsage(new string[] { }));
            }

            [TestMethod]
            public void NoParametersWithOne()
            {
                Assert.IsFalse(emptyParameters.MatchesUsage(new string[] { "foo" }));
            }

            [TestMethod]
            public void OneParameterWithOneMatching()
            {
                Assert.IsTrue(oneParameter.MatchesUsage(new string[] { "i" }));
            }

            [TestMethod]
            public void OneParameterWithOneNotMatching()
            {
                Assert.IsFalse(oneParameter.MatchesUsage(new string[] { "x" }));
            }

            [TestMethod]
            public void TwoParametersMatchingInOrder()
            {
                Assert.IsTrue(twoParameters.MatchesUsage(new string[] { "i", "s" }));
            }

            [TestMethod]
            public void TwoParametersMatchingInOutOfOrder()
            {
                Assert.IsTrue(twoParameters.MatchesUsage(new string[] { "s", "i" }));
            }

            [TestMethod]
            public void TwoWithOptionalParametersMatching()
            {
                Assert.IsTrue(twoParameters.MatchesUsage(new string[] { "i", "s" }));
            }

            [TestMethod]
            public void TwoWithOptionalParametersMatchingOnlyRequired()
            {
                Assert.IsTrue(twoWithAnOptional.MatchesUsage(new string[] { "i" }));
            }
        }
    }
}
