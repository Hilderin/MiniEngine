using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests
{
    /// <summary>
    /// Helpers for testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Get the path for a test file result
        /// </summary>
        public static string GetPathTestResultFile(string name)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                     .Replace("bin\\Debug\\net7.0", String.Empty)
                                     .Replace("bin\\Release\\net7.0", String.Empty);

            return Path.Combine(currentPath, "TestFiles", name);

        }
    }
}
