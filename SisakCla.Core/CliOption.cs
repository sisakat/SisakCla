using System;
using System.Reflection;

namespace SisakCla.Core
{
    [System.AttributeUsage(System.AttributeTargets.Method
        | System.AttributeTargets.Property
        | System.AttributeTargets.Field)]
    public class CliOption : System.Attribute
    {
        /// <summary>
        /// Short option signature (e.g. -f)
        /// </summary>
        public string Option { get; private set; }

        /// <summary>
        /// Short name for the option
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Long option signature (e.g. --force)
        /// </summary>
        public string LongOption { get; set; }

        /// <summary>
        /// Description of the option, displayed in the help text.
        /// </summary>
        public string Description { get; set; }
        public bool Required { get; set; }

        /// <summary>
        /// Methods, fields or properties are initialized ordered by their priority.
        /// </summary>
        public int Priority { get; set; }
        internal object Base { get; set; }
        internal MethodInfo Method { get; set; }
        internal FieldInfo Field { get; set; }
        internal PropertyInfo Property { get; set; }

        public CliOption(string option)
        {
            Option = option;
        }
    }
}
