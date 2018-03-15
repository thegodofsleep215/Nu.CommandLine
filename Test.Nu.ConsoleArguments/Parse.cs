using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Nu.ConsoleArguments;

namespace Test.Nu.ConsoleArgumentsTest
{
    [TestClass]
    public class ParseMethod
    {
        [TestMethod]
        public void EmptyArgs()
        {
            var args = new string[0];
            var actual = ConsoleArguments.Parse(args);
            Assert.AreEqual(0, actual.NamedArguments.Count);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void OneUnnamedArgument()
        {
            var args = new[] {"foo"};
            var actual = ConsoleArguments.Parse(args);
            Assert.AreEqual(0, actual.NamedArguments.Count);
            Assert.AreEqual(1, actual.UnnamedArguments.Length);
            Assert.AreEqual("foo", actual.UnnamedArguments[0]);
        }

        [TestMethod]
        public void TwoUnnamedArguments()
        {
            var args = new[] {"foo", "bar"};
            var actual = ConsoleArguments.Parse(args);
            Assert.AreEqual(0, actual.NamedArguments.Count);
            Assert.AreEqual(2, actual.UnnamedArguments.Length);
            Assert.AreEqual("foo", actual.UnnamedArguments[0]);
            Assert.AreEqual("bar", actual.UnnamedArguments[1]);
        }

        [TestMethod]
        public void OneNamedArgument()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] {"-foo", "bar"};
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(1, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }   

        [TestMethod]
        public void OneNamedArgumentSlashStartsName()
        {
            var denote = '\\';
            var delimiter = ' ';
            var args = new[] {"\\foo", "bar"};
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(1, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void OneNamedArgumentEqualDelimits()
        {
            var denote = '-';
            var delimiter = '=';
            var args = new[] {"-foo=bar"};
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(1, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void TwoNamedArguments()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "-foo", "bar", "-fee", "beer" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(2, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
             Assert.AreEqual("fee", actual.NamedArguments.ElementAt(1).Key);
            Assert.AreEqual("beer", actual.NamedArguments.ElementAt(1).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void TwoNamedArgumentsWithEqualDelimiter()
        {
            var denote = '-';
            var delimiter = '=';
            var args = new[] { "-foo=bar", "-fee=beer" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(2, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
             Assert.AreEqual("fee", actual.NamedArguments.ElementAt(1).Key);
            Assert.AreEqual("beer", actual.NamedArguments.ElementAt(1).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void OneNamedArgumentTheUnnamed()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "-foo", "bar", "beer" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(1, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar", actual.NamedArguments.ElementAt(0).Value);
            Assert.AreEqual(1, actual.UnnamedArguments.Length);
            Assert.AreEqual("beer", actual.UnnamedArguments[0]);
        }

        [TestMethod]
        public void OneNamedArgumentWithQuotedString()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] {"-foo", "\"bar foo\""};
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            Assert.AreEqual(1, actual.NamedArguments.Count);
            Assert.AreEqual("foo", actual.NamedArguments.ElementAt(0).Key);
            Assert.AreEqual("bar foo", actual.NamedArguments.ElementAt(0).Value);
            Assert.AreEqual(0, actual.UnnamedArguments.Length);
        }

        [TestMethod]
        public void OneFlag()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "-flag" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
        }

        [TestMethod]
        public void OneFlagBeforeNamedArgument()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "-flag", "-foo", "\"bar foo\"" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
            actual.NamedArguments.ContainsKey("foo").ShouldBeTrue();
            actual.NamedArguments["foo"].ShouldEqual("bar foo");
        }

        [TestMethod]
        public void OneFlagBeforeNamedArgumentEqualDelimiter()
        {
            var denote = '-';
            var delimiter = '=';
            var args = new[] { "-flag", "-foo=\"bar foo\"" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
            actual.NamedArguments.ContainsKey("foo").ShouldBeTrue();
            actual.NamedArguments["foo"].ShouldEqual("bar foo");
        }

        [TestMethod]
        public void OneFlagAfterNamedArgument()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "-foo", "\"bar foo\"", "-flag" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
            actual.NamedArguments.ContainsKey("foo").ShouldBeTrue();
            actual.NamedArguments["foo"].ShouldEqual("bar foo");
        }

        [TestMethod]
        public void OneFlagAfterUnnamedArgument()
        {
            var denote = '-';
            var delimiter = ' ';
            var args = new[] { "foo", "-flag" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
            actual.UnnamedArguments.Length.ShouldEqual(1);
            actual.UnnamedArguments.ShouldContain("foo");
        }

        [TestMethod]
        public void OneFlagAfterBeforeNamedArgumentEqualIsDelimiter()
        {
            var denote = '-';
            var delimiter = '=';
            var args = new[] { "-flag", "foo" };
            var actual = ConsoleArguments.Parse(args, denote, delimiter);
            actual.Flags.Count.ShouldEqual(1);
            actual.Flags.ShouldContain("flag");
            actual.UnnamedArguments.Length.ShouldEqual(1);
            actual.UnnamedArguments.ShouldContain("foo");
        }




    }
}
