using System;
using Xunit;
using SisakCla.Core;
using System.IO;

namespace SisakCla.Core.Test
{
    public class UnitTest1
    {
        class FunctionClass
        {
            public bool a = false;
            public string b = String.Empty;
            public string c1 = String.Empty;
            public double c2 = 0.0;

            [CliOption("-a")]
            public void TestA()
            {
                a = true;
            }

            [CliOption("-b", LongOption = "--beta")]
            public void TestB(string bParam)
            {
                b = bParam;
            }

            [CliOption("-c", LongOption = "--charlie")]
            public void TestC(string cParam1, double cParam2)
            {
                c1 = cParam1;
                c2 = cParam2;
            }
        }

        [Fact]
        public void TestShortOption()
        {
            FunctionClass f = new FunctionClass();
            string bParam = "test from b";
            Cli cli = new Cli(new string[] { "-a", "-b", bParam });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.True(f.a);
            Assert.Equal(f.b, bParam);
        }

        [Fact]
        public void TestLongOption()
        {
            FunctionClass f = new FunctionClass();
            string bParam = "test from b";
            Cli cli = new Cli(new string[] { "--beta", bParam });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(f.b, bParam);
        }

        [Fact]
        public void TestMultipleParameter()
        {
            FunctionClass f = new FunctionClass();
            string cParam1 = "test";
            double cParam2 = 12.0;
            Cli cli = new Cli(new string[] { "--charlie", cParam1, cParam2.ToString() });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(f.c1, cParam1);
            Assert.Equal(f.c2, cParam2);
        }

        [Fact]
        public void TestNullParameter()
        {
            FunctionClass f = new FunctionClass();
            string cParam1 = "test";
            double cParam2 = f.c2;
            Cli cli = new Cli(new string[] { "--charlie", cParam1 });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(f.c1, cParam1);
            Assert.Equal(f.c2, cParam2);
        }

        [Fact]
        public void TestTooManyParameter()
        {
            FunctionClass f = new FunctionClass();
            string cParam1 = "test";
            double cParam2 = f.c2;
            Cli cli = new Cli(new string[] { "--charlie", cParam1, cParam2.ToString(), "2", "3" });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(f.c1, cParam1);
            Assert.Equal(f.c2, cParam2);
        }

        [Fact]
        public void TestTypeException()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "--charlie", "", "a" });
            cli.AddFunctionClass(f);
            Assert.Throws<InvalidCastException>(() =>
            {
                cli.Parse();
            });
        }

        [Fact]
        public void TestHelp()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "--charlie", "", "a" });
            cli.AddFunctionClass(f);
            TextWriter tw = new StringWriter();
            cli.DefaultTextWriter = tw;
            cli.PrintHelp();
            Assert.True(tw.ToString().Length > 0);
        }
    }
}
