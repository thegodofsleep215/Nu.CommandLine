using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nu.CommandLine.Commands;
using Shouldly;

namespace Test.Nu.CommandLine
{
    [TestClass]
    public class TestMethodInfoExecution
    {
        public string EmptyParameterMethod()
        {
            return "empty";
        }

        public string OneParameterMethod(int i)
        {
            return "one";
        }

        public string TwoParametersMethod(int i, string s)
        {
            return $"{i} {s}";
        }

        public string TwoWithAnOptional(int i, string s = "")
        {
            return $"{i} {s}";
        }

        protected TestMethodInfoExecution()
        {
            noParameters = new MethodInfoExecution(GetType().GetMethod("EmptyParameterMethod"), this);
            oneParameter = new MethodInfoExecution(GetType().GetMethod("OneParameterMethod"), this);
            twoParameters = new MethodInfoExecution(GetType().GetMethod("TwoParametersMethod"), this);
            twoWithOptional = new MethodInfoExecution(GetType().GetMethod("TwoWithAnOptional"), this);
        }

        protected MethodInfoExecution noParameters;
        protected MethodInfoExecution oneParameter;
        protected MethodInfoExecution twoParameters;
        protected MethodInfoExecution twoWithOptional;

        [TestClass]
        public class Constructor : TestMethodInfoExecution
        {
            [TestMethod]
            public void NoParameters()
            {
                noParameters.Method.ShouldBe(GetType().GetMethod("EmptyParameterMethod"));
                noParameters.CommandObject.ShouldBe(this);
                noParameters.DefaultMethodName.ShouldBe("EmptyParameterMethod");
                noParameters.AllParameterNames.ShouldBeEmpty();
                noParameters.OptionalParameterNames.ShouldBeEmpty();
                noParameters.RequiredParameterNames.ShouldBeEmpty();
            }

            [TestMethod]
            public void OneParameter()
            {
                oneParameter.Method.ShouldBe(GetType().GetMethod("OneParameterMethod"));
                oneParameter.CommandObject.ShouldBe(this);
                oneParameter.DefaultMethodName.ShouldBe("OneParameterMethod");

                oneParameter.AllParameterNames.Length.ShouldBe(1);
                oneParameter.AllParameterNames.ShouldContain("i");

                oneParameter.OptionalParameterNames.ShouldBeEmpty();

                oneParameter.RequiredParameterNames.Length.ShouldBe(1);
                oneParameter.RequiredParameterNames.ShouldContain("i");
            }

            [TestMethod]
            public void TwoParameters()
            {
                twoParameters.Method.ShouldBe(GetType().GetMethod("TwoParametersMethod"));
                twoParameters.CommandObject.ShouldBe(this);
                twoParameters.DefaultMethodName.ShouldBe("TwoParametersMethod");

                twoParameters.AllParameterNames.Length.ShouldBe(2);
                twoParameters.AllParameterNames.ShouldContain("i");
                twoParameters.AllParameterNames.ShouldContain("s");

                twoParameters.OptionalParameterNames.ShouldBeEmpty();

                twoParameters.RequiredParameterNames.Length.ShouldBe(2);
                twoParameters.RequiredParameterNames.ShouldContain("i");
                twoParameters.RequiredParameterNames.ShouldContain("s");
            }

            [TestMethod]
            public void TwoWithAnOptionalTest()
            {
                twoWithOptional.Method.ShouldBe(GetType().GetMethod("TwoWithAnOptional"));
                twoWithOptional.CommandObject.ShouldBe(this);
                twoWithOptional.DefaultMethodName.ShouldBe("TwoWithAnOptional");

                twoWithOptional.AllParameterNames.Length.ShouldBe(2);
                twoWithOptional.AllParameterNames.ShouldContain("i");
                twoWithOptional.AllParameterNames.ShouldContain("s");

                twoWithOptional.RequiredParameterNames.Length.ShouldBe(1);
                twoWithOptional.RequiredParameterNames.ShouldContain("i");

                twoWithOptional.OptionalParameterNames.Length.ShouldBe(1);
                twoWithOptional.OptionalParameterNames.ShouldContain("s");

            }
        }

        [TestClass]
        public class CanExecute : TestMethodInfoExecution
        {
            [TestMethod]
            public void NoParametersWorks()
            {
                noParameters.CanExecute(new Dictionary<string, object>(), out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.ShouldBeEmpty();
                error.ShouldBeEmpty();
            }

            [TestMethod]
            public void OneParametersWorks()
            {
                oneParameter.CanExecute(new Dictionary<string, object>{{"i", "1"}}, out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.Length.ShouldBe(1);
                actualCasted[0].ShouldBe(1);
                error.ShouldBeEmpty();
            }

            [TestMethod]
            public void TwoParametersWorks()
            {
                twoParameters.CanExecute(new Dictionary<string, object> { { "i", "1" }, {"s", "s"} }, out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.Length.ShouldBe(2);
                actualCasted[0].ShouldBe(1);
                actualCasted[1].ShouldBe("s");
                error.ShouldBeEmpty();
            }

            [TestMethod]
            public void TwoParametersWorksOutOfOrder()
            {
                twoParameters.CanExecute(new Dictionary<string, object> { { "s", "s" }, { "i", "1" } }, out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.Length.ShouldBe(2);
                actualCasted[0].ShouldBe(1);
                actualCasted[1].ShouldBe("s");
                error.ShouldBeEmpty();
            }

            [TestMethod]
            public void TwoWithOptionalWorksWithAll()
            {
                twoWithOptional.CanExecute(new Dictionary<string, object> { { "i", "1" }, { "s", "s" } }, out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.Length.ShouldBe(2);
                actualCasted[0].ShouldBe(1);
                actualCasted[1].ShouldBe("s");
                error.ShouldBeEmpty();
            }

            [TestMethod]
            public void TwoWithOptionalWorksWithOne()
            {
                twoWithOptional.CanExecute(new Dictionary<string, object> { { "i", "1" } }, out var actualCasted, out var error).ShouldBeTrue();
                actualCasted.Length.ShouldBe(2);
                actualCasted[0].ShouldBe(1);
                actualCasted[1].ShouldBe("");
                error.ShouldBeEmpty();
            }
        }
    }
}
