# SisakCla
A small and simple library for dealing with Command-Line-Arguments.

## Usage

You can annotate methods, fields and properties so that they are used to reflect options. Parameters and types are parsed accordingly.

```csharp
class Program
{
    static void Main(string[] args)
    {
        SisakCla.Core.Cli cli = new SisakCla.Core.Cli(args);
        cli.Description = "Sample Program";
        cli.Version = "1.0";
        cli.Copyright = "(c) Sisak";
        
        cli.AddFunctionClass(new FunctionClass());

        try 
        {
            cli.Parse();
        } 
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }
}

class FunctionClass
{
    [CliOption("-f", LongOption = "--flag", Description = "Flag that does something")]
    public bool Flag { get; set; }

    [CliOption("-a", LongOption = "--add", Description = "Add two numbers")]
    public void Add(int num1, double num2 = 2.5)
    {
        Console.WriteLine($"{param1} + {param2} = {param1 + param2}");
    }
}
```

Calling the program:

```
> dotnet SampleProgram.dll
Sample Program
1.0
(c) Sisak

Parameters:
-a (--add) <num1> [num2=2.5]
        Add two numbers

-f (--flag)
        Flag that does something

-h (--help)
        Prints the help text.

> dotnet SampleProgram.dll -a 3 4.5 -f
3 + 4.5 = 7.5
```