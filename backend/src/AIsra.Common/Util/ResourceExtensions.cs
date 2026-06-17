using System.Reflection;

namespace AIsra.Common.Util;

public static class ResourceExtensions
{
    extension(Assembly asm)
    {
        public string ReadEmbeddedTextFile(string resourceName)
        {
            using var stream = asm.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                throw new FileNotFoundException($"Could not find embedded resource: '{resourceName}'");
            }

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
