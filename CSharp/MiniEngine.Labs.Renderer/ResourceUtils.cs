using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MiniEngine.Labs.Renderer
{
    /// <summary>
    /// Class to access embedded resources
    /// </summary>
    internal static class ResourceUtils
    {
        private static Assembly _assembly;
        private static string _namespace;

        /// <summary>
        /// Constructor
        /// </summary>
        static ResourceUtils()
        {
            _assembly = typeof(ResourceUtils).Assembly;
            _namespace = typeof(ResourceUtils).Namespace;
        }

        /// <summary>
        /// Get string data from resource
        /// </summary>
        public static string GetString(string name)
        {
            string resourceName = $"{_namespace}.{name}";
            using (Stream resource = _assembly.GetManifestResourceStream(resourceName))
            using (StreamReader sr = new StreamReader(resource))
            {
                if (resource == null)
                    throw new FileNotFoundException($"Resource '{resourceName}' not found. Available resources: {String.Join(", ", _assembly.GetManifestResourceNames())}");

                return sr.ReadToEnd();
            }
        }


        /// <summary>
        /// Get bytes data from resource
        /// </summary>
        public static byte[] GetBytes(string name)
        {
            byte[] data;
            string resourceName = $"{_namespace}.{name}";
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
