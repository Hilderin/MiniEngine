using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MiniEngine
{
    /// <summary>
    /// Class to access embedded resources
    /// </summary>
    internal static class ResourceUtils
    {
        private static Assembly _assembly;
        private const string _assemblyName = "MiniEngine";
        private static Encoding _encoding;

        /// <summary>
        /// Constructor
        /// </summary>
        static ResourceUtils()
        {
            _assembly = typeof(ResourceUtils).Assembly;
            _encoding = System.Text.Encoding.UTF8;
        }

        /// <summary>
        /// Get string from resource
        /// </summary>
        public static string GetString(string name)
        {
            return _encoding.GetString(GetBytes(name));
        }


        /// <summary>
        /// Get bytes data from resource
        /// </summary>
        public static byte[] GetBytes(string name)
        {
            byte[] data;
            string resourceName = $"{_assemblyName}.Resources.{name}";
            using (Stream resource = _assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                    throw new FileNotFoundException($"Resource '{resourceName}' not found. Available resources: {String.Join(", ", _assembly.GetManifestResourceNames())}");

                data = new byte[resource.Length];
                resource.Read(data, 0, (int)resource.Length);
            }
            return data;
        }

    }
}
