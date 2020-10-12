using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

namespace SisakCla.Core
{
    public class Cli
    {
        private string[] _args;
        private List<CliOption> _options;

        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }


        public bool PrintParameters { get; set; } = true;
        public bool PrintDefaultValue { get; set; } = true;

        public TextWriter DefaultTextWriter { get; set; } = Console.Out;

        public Cli(string[] args)
        {
            _args = args;
            _options = new List<CliOption>();
            AddFunctionClass(this);
        }

        public void AddFunctionClass<T>(T t)
        {
            var methods = ReflectionHelper.GetMethodsWithAttribute(typeof(T), typeof(CliOption));
            foreach (var method in methods)
            {
                var cliOption = method.GetCustomAttribute<CliOption>();
                cliOption.Base = t;
                cliOption.Method = method;
                _options.Add(cliOption);
            }
        }

        [CliOption("-h", LongOption = "--help", Description = "Prints the help text.")]
        public void PrintHelp()
        {
            var textWriter = DefaultTextWriter;
            _options.Sort((x, y) => x.Option.CompareTo(y.Option));
            textWriter.WriteLine(Description);
            textWriter.WriteLine(Version);
            textWriter.WriteLine(Copyright);
            textWriter.WriteLine();
            if (_options.Count > 0)
            {
                textWriter.WriteLine("Parameters:");
            }
            foreach (var option in _options)
            {
                textWriter.Write($"{option.Option}");
                if (!String.IsNullOrEmpty(option.LongOption))
                    textWriter.Write($" ({option.LongOption})");
                if (PrintParameters)
                {
                    PrintParameterHelp(textWriter, option.Method.GetParameters());
                }
                textWriter.WriteLine();
                textWriter.WriteLine($"\t{option.Description}");
                textWriter.WriteLine();
            }
        }

        private void PrintParameterHelp(TextWriter textWriter, ParameterInfo[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.HasDefaultValue)
                {
                    textWriter.Write($" [{parameter.Name}");
                    if (PrintDefaultValue) 
                    {
                        textWriter.Write($"={parameter.DefaultValue}");
                    }
                    textWriter.Write("]");
                }
                else
                {
                    textWriter.Write($" <{parameter.Name}>");
                }
            }
        }

        public void Parse()
        {
            Queue<string> arguments = new Queue<string>();
            CliOption currentOption = null;

            for (int i = 0; i < _args.Length; i++)
            {
                var argument = _args[i];
                if (argument.StartsWith("-"))
                {
                    CheckAndInvoke(currentOption, arguments);
                    foreach (var option in _options)
                    {
                        if (option.Option == argument || option.LongOption == argument)
                        {
                            currentOption = option;
                            break;
                        }
                    }
                }
                else
                {
                    arguments.Enqueue(argument);
                }
            }

            CheckAndInvoke(currentOption, arguments);
        }

        private void CheckAndInvoke(CliOption option, Queue<string> parameters)
        {
            if (option != null)
            {
                InvokeOption(option, parameters);
                option = null;
                parameters.Clear();
            }
        }

        const string CAST_ERROR_MESSAGE = "CLI-Option {0} expected parameter {1} of type {2}";

        private string CreateErrorMessage(CliOption option, int i, Type parameter)
        {
            return string.Format(CAST_ERROR_MESSAGE,
                $"{option.Name}({option.Option})",
                i + 1,
                parameter);
        }

        private void InvokeOption(CliOption option, IEnumerable<string> parameters)
        {
            string[] strParameters = parameters.ToArray();
            ParameterInfo[] methodParameters = option.Method.GetParameters();

            if (methodParameters != null && methodParameters.Length > 0)
            {
                object[] parsedParameters = new object[methodParameters.Length];

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    Type parameterType = methodParameters[i].ParameterType;
                    string parameter = strParameters.Length > i ? strParameters[i] : null;

                    switch (Type.GetTypeCode(parameterType))
                    {
                        case TypeCode.Object:
                        case TypeCode.String:
                            parsedParameters[i] = parameter;
                            break;
                        case TypeCode.Int16:
                            try
                            {
                                parsedParameters[i] = Int16.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Int32:
                            try
                            {
                                parsedParameters[i] = Int32.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Int64:
                            try
                            {
                                parsedParameters[i] = Int64.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Double:
                            try
                            {
                                parsedParameters[i] = Double.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Boolean:
                            try
                            {
                                parsedParameters[i] = Boolean.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Decimal:
                            try
                            {
                                parsedParameters[i] = Decimal.Parse(parameter ?? "0");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.Char:
                            try
                            {
                                parsedParameters[i] = Char.Parse(parameter ?? "");
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        case TypeCode.DateTime:
                            try
                            {
                                parsedParameters[i] = DateTime.Parse(parameter);
                            }
                            catch (Exception)
                            {
                                throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                            }
                            break;
                        default:
                            throw new InvalidCastException($"Type {Type.GetTypeCode(parameterType)} not supported");
                    }
                }

                option.Method?.Invoke(option.Base, parsedParameters);
            }
            else
            {
                option.Method?.Invoke(option.Base, new object[] { });
            }
        }
    }
}