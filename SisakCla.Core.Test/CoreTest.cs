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

            [CliOption("-d1")]
            public double d1 = -1.0;

            [CliOption("-d2")]
            public bool d2 { get; set; }

            [CliOption("-e3")]
            public int e = 0;

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

            [CliOption("-e1", Priority = 1)]
            public void TestE1() 
            {
                e = 1;
            }

            [CliOption("-e2", Priority = 2)]
            public void TestE2() 
            {
                e = 2;
            }


            public double f1 = 0.0;
            public string[] fArr = null;

            [CliOption("-f")]
            public void TestF(double f1, string[] fArr) 
            {
                this.f1 = f1;
                this.fArr = fArr;
            }

            [CliOption("gustav", LongOption = "gustavu", Description = "test")]
            public void TestG(string test)
            {

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
            Assert.Equal(bParam, f.b);
        }

        [Fact]
        public void TestLongOption()
        {
            FunctionClass f = new FunctionClass();
            string bParam = "test from b";
            Cli cli = new Cli(new string[] { "--beta", bParam });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(bParam, f.b);
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
            Assert.Equal(cParam1, f.c1);
            Assert.Equal(cParam2, f.c2);
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
            Assert.Equal(cParam1, f.c1);
            Assert.Equal(cParam2, f.c2);
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
            Assert.Equal(cParam1, f.c1);
            Assert.Equal(cParam2, f.c2);
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

        [Fact]
        public void TestD()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "-d1", (0.0).ToString() });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(0.0, f.d1);
        }

        [Fact]
        public void TestD2()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "-d2" });
            cli.AddFunctionClass(f);
            Assert.False(f.d2);
            cli.Parse();
            Assert.True(f.d2);
        }

        [Fact]
        public void TestPriority()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "-e2", "-e1", "-e3", "3" });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(2, f.e);
        }

        [Fact]
        public void TestThrowField()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "-e3", "ASDF" });
            cli.AddFunctionClass(f);
            Assert.Throws<InvalidCastException>(() => cli.Parse());
        }

        [Fact]
        public void TestArray()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "-f", (1.0).ToString(), "a", "b", "c" });
            cli.AddFunctionClass(f);
            cli.Parse();
            Assert.Equal(1.0, f.f1);
            Assert.True(f.fArr.Length == 3);
        }

        [Fact]
        public void TestGustav()
        {
            FunctionClass f = new FunctionClass();
            Cli cli = new Cli(new string[] { "gustav" });
            cli.AddFunctionClass(f);
            cli.Parse();
        }
    }
}
