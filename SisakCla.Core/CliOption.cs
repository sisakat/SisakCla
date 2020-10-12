using System;
using System.Reflection;

namespace SisakCla.Core
{
    [System.AttributeUsage(System.AttributeTargets.Method
        | System.AttributeTargets.Property
        | System.AttributeTargets.Field)]
    public class CliOption : System.Attribute
    {
        public string Option { get; private set; }
        public string Name { get; set; }
        public string LongOption { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
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
