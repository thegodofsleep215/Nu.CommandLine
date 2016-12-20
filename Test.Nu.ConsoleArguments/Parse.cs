using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nu.ConsoleArguments;

namespace Test.NuConoleArguments
{
    [TestClass]
    public class Parse
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


    }
}
