using System.IO;

namespace SisakCla.Core
{
    internal static class TextWriterExtensions
    {
        public static void WriteLineIfNotEmpty(this TextWriter textWriter, string value) 
        {
            if (!string.IsNullOrEmpty(value))
                textWriter.WriteLine(value);
        }
    }
}