using System;

namespace SisakCla.Core
{
    public class MissingRequiredOptionException : Exception
    {
        public CliOption RequiredOption { get; private set; }
        public MissingRequiredOptionException()
        {
        }

        public MissingRequiredOptionException(string message) : base(message)
        {
        }

        public MissingRequiredOptionException(CliOption option)
            : base($"{option.Option} is required but was not specified")
        {
            RequiredOption = option;
        }
    }
}