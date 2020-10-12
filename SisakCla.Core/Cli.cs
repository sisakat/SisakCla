using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.ComponentModel;

namespace SisakCla.Core
{
    public class Cli
    {
        private string[] _args;
        private List<CliOption> _options;

        /// <summary>
        /// Brief description or name of the application. Displayed in the help text.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Version of the application. Displayed in the help text.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Copyright notice displayed in the help text.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Wether parameters are printed in the help text or not.
        /// </summary>
        public bool PrintParameters { get; set; } = true;

        /// <summary>
        /// Wether default values of parameters are printed in the help text or not.
        /// </summary>
        public bool PrintDefaultValue { get; set; } = true;

        /// <summary>
        /// TextWriter used for printing the help text.
        /// </summary>
        public TextWriter DefaultTextWriter { get; set; } = Console.Out;

        public Cli(string[] args)
        {
            _args = args;
            _options = new List<CliOption>();
            AddFunctionClass(this);
        }

        /// <summary>
        /// Function class used for extracting options.
        /// </summary>
        public void AddFunctionClass<T>(T t)
        {
            AddMethods(t);
            AddFields(t);
            AddProperties(t);
        }

        private void AddMethods<T>(T t)
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

        private void AddFields<T>(T t)
        {
            var fields = ReflectionHelper.GetFieldsWithAttribute(typeof(T), typeof(CliOption));
            foreach (var field in fields)
            {
                var cliOption = field.GetCustomAttribute<CliOption>();
                cliOption.Base = t;
                cliOption.Field = field;
                _options.Add(cliOption);
            }
        }

        private void AddProperties<T>(T t)
        {
            var properties = ReflectionHelper.GetPropertiesWithAttribute(typeof(T), typeof(CliOption));
            foreach (var property in properties)
            {
                var cliOption = property.GetCustomAttribute<CliOption>();
                cliOption.Base = t;
                cliOption.Property = property;
                _options.Add(cliOption);
            }
        }

        /// <summary>
        /// Prints the help text to the DefaultTextWriter
        /// </summary>
        [CliOption("-h", LongOption = "--help", Description = "Prints the help text.")]
        public void PrintHelp()
        {
            var textWriter = DefaultTextWriter;
            _options.Sort((x, y) => x.Option.CompareTo(y.Option));
            textWriter.WriteLineIfNotEmpty(Description);
            textWriter.WriteLineIfNotEmpty(Version);
            textWriter.WriteLineIfNotEmpty(Copyright);
            textWriter.WriteLine();
            if (_options.Count > 0)
            {
                textWriter.WriteLine("Parameters:");
            }
            foreach (var option in _options.Where(x => !string.IsNullOrEmpty(x.Description)))
            {
                textWriter.Write($"{option.Option}");
                if (!String.IsNullOrEmpty(option.LongOption))
                    textWriter.Write($" ({option.LongOption})");
                if (PrintParameters)
                {
                    if (option.Method != null)
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

        /// <summary>
        /// Parsed the command line arguments
        /// </summary>
        public void Parse()
        {
            List<Tuple<CliOption, IEnumerable<string>>> options = new List<Tuple<CliOption, IEnumerable<string>>>();
            Queue<string> arguments = new Queue<string>();
            CliOption currentOption = null;

            for (int i = 0; i < _args.Length; i++)
            {
                var argument = _args[i];

                bool isOption = false;
                foreach (var option in _options)
                {
                    if (option.Option == argument || option.LongOption == argument)
                    {
                        isOption = true;
                        if (currentOption != null)
                        {
                            options.Add(new Tuple<CliOption, IEnumerable<string>>(currentOption, arguments.ToList()));
                            arguments.Clear();
                        }
                        currentOption = option;
                        break;
                    }
                }
                
                if (currentOption != null && !isOption)
                {
                    arguments.Enqueue(argument);
                    continue;
                }
            }

            if (currentOption != null)
            {
                options.Add(new Tuple<CliOption, IEnumerable<string>>(currentOption, arguments.ToList()));
            }

            InvokeAll(options);
        }

        private void InvokeAll(IEnumerable<Tuple<CliOption, IEnumerable<string>>> options)
        {
            if (options == null || options.Count() == 0)
            {
                PrintHelp();
                return;
            }

            foreach (var item in options.Where(x => x.Item1.Field != null).OrderBy(x => x.Item1.Priority))
            {
                CheckAndInvoke(item.Item1, item.Item2);
            }

            foreach (var item in options.Where(x => x.Item1.Property != null).OrderBy(x => x.Item1.Priority))
            {
                CheckAndInvoke(item.Item1, item.Item2);
            }

            foreach (var item in options.Where(x => x.Item1.Method != null).OrderBy(x => x.Item1.Priority))
            {
                CheckAndInvoke(item.Item1, item.Item2);
            }
        }

        private void CheckAndInvoke(CliOption option, IEnumerable<string> parameters)
        {
            if (option.Method != null)
                InvokeOptionMethod(option, parameters);
            if (option.Field != null)
                InvokeOptionField(option, parameters);
            if (option.Property != null)
                InvokeOptionProperty(option, parameters);
        }

        const string CAST_ERROR_MESSAGE = "CLI-Option {0} expected parameter {1} of type {2}";

        private string CreateErrorMessage(CliOption option, int i, Type parameter)
        {
            return string.Format(CAST_ERROR_MESSAGE,
                $"{option.Name}({option.Option})",
                i + 1,
                parameter);
        }

        private void InvokeOptionMethod(CliOption option, IEnumerable<string> parameters)
        {
            if (option.Method == null) throw new ArgumentException("Method cannot be null");
            string[] strParameters = parameters.ToArray();
            ParameterInfo[] methodParameters = option.Method.GetParameters();

            if (methodParameters != null && methodParameters.Length > 0)
            {
                object[] parsedParameters = new object[methodParameters.Length];

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    Type parameterType = methodParameters[i].ParameterType;
                    if (parameterType == typeof(string[])) 
                    {
                        if (strParameters.Length > i)
                        {
                            List<string> tmp = new List<string>();
                            for (int j = i; j < strParameters.Length; j++)
                            {
                                tmp.Add(strParameters[j]);
                            }
                            parsedParameters[i] = tmp.ToArray();
                        }
                    } 
                    else 
                    {
                        string parameter = strParameters.Length > i ? strParameters[i] : null;
                        if (parameter == null && methodParameters[i].HasDefaultValue) 
                            parsedParameters[i] = methodParameters[i].DefaultValue;
                        else 
                            parsedParameters[i] = ParseParameter(parameterType, parameter, i, option);
                    }
                }

                option.Method?.Invoke(option.Base, parsedParameters);
            }
            else
            {
                option.Method?.Invoke(option.Base, new object[] { });
            }
        }

        private void InvokeOptionField(CliOption option, IEnumerable<string> parameters)
        {
            if (option.Field == null) throw new ArgumentException("Field cannot be null");
            Type fieldType = option.Field.FieldType;
            if (fieldType == typeof(bool))
            {
                option.Field.SetValue(option.Base, true);
            }
            else
            {
                var element = parameters.FirstOrDefault();
                if (element != null) 
                {
                    var result = ParseParameter(fieldType, element, 0, option);
                    option.Field.SetValue(option.Base, result);
                }
            }
        }

        private void InvokeOptionProperty(CliOption option, IEnumerable<string> parameters)
        {
            if (option.Property == null) throw new ArgumentException("Property cannot be null");
            Type propertyType = option.Property.PropertyType;
            if (propertyType == typeof(bool))
            {
                option.Property.SetValue(option.Base, true);
            }
            else
            {
                var element = parameters.FirstOrDefault();
                if (element != null) 
                {
                    var result = ParseParameter(propertyType, element, 0, option);
                    option.Property.SetValue(option.Base, result);
                }
            }
        }

        private object ParseParameter(Type parameterType, string parameter, int i, CliOption option)
        {
            switch (Type.GetTypeCode(parameterType))
            {
                case TypeCode.Object:
                case TypeCode.String:
                    return parameter;
                case TypeCode.Int16:
                    try
                    {
                        return Int16.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Int32:
                    try
                    {
                        return Int32.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Int64:
                    try
                    {
                        return Int64.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Double:
                    try
                    {
                        return Double.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Boolean:
                    try
                    {
                        return Boolean.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Decimal:
                    try
                    {
                        return Decimal.Parse(parameter ?? "0");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.Char:
                    try
                    {
                        return Char.Parse(parameter ?? "");
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                case TypeCode.DateTime:
                    try
                    {
                        return DateTime.Parse(parameter);
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(CreateErrorMessage(option, i, parameterType));
                    }
                default:
                    throw new InvalidCastException($"Type {Type.GetTypeCode(parameterType)} not supported");
            }
        }
    }
}