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
    public static class ResourceUtils
    {

        /// <summary>
        /// Get string data from resource
        /// </summary>
        public static string GetString(string name)
        {
            using (Stream resource = GetStream(name))
            {
                if (resource == null)
                    throw new FileNotFoundException($"Resource '{name}' not found.");

                using (StreamReader sr = new StreamReader(resource))
                {
                    return sr.ReadToEnd();
                }
            }
        }


        /// <summary>
        /// Get bytes data from resource
        /// </summary>
        public static byte[] GetBytes(string name)
        {
            byte[] data;
            using (Stream resource = GetStream(name))
            {
                if (resource == null)
                    throw new FileNotFoundException($"Resource '{name}' not found.");

                data = new byte[resource.Length];
                resource.Read(data, 0, (int)resource.Length);
            }
            return data;
        }

        
        /// <summary>
        /// Check if an assembly could be used for resources
        /// </summary>
        public static bool IsAssemblyUsable(Assembly assembly)
        {
            if (assembly.FullName.StartsWith("System.") || assembly.FullName.StartsWith("Microsoft.") || assembly.FullName.StartsWith("netstandard,"))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get the stream for a resource in all referenced assemblies
        /// </summary>
        public static Stream GetStream(string name)
        {

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!IsAssemblyUsable(assembly))
                    continue;

                Stream resource = assembly.GetManifestResourceStream(name);
                if (resource != null)
                    return resource;
            }

            return null;
        }

    }
}
