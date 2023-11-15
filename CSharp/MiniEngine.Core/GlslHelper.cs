using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MiniEngine
{
    /// <summary>
    /// Helper for GLSL files
    /// </summary>
    public static class GlslHelper
    {
        /// <summary>
        /// NEW LINE CHAR ARRAY
        /// </summary>
        private static readonly char[] NEW_LINE = new[] { '\n' };

        /// <summary>
        /// Matches everything inside #include "(.)" except " so we get shortest ".+" match into a group
        /// </summary>
        /// <value>
        /// Regular expression.
        /// </value>
        //public static string Include => @"#include\s+""([^""]+)"""; //
        private static readonly Regex _includeRegEx = new Regex(@"#include\s+""([^""]+)""", RegexOptions.Compiled);

        /// <summary>
        /// Expand the shader code with includes and manage comments //! and //?
        /// </summary>
        public static string Expand(string code, string workingFolder)
        {
            //Manage the includes and the //!
            HashSet<string> includedFiles = new HashSet<string>();
            return ExpandCode(code, workingFolder, includedFiles);
        }


        /// <summary>
        /// Expand the shader code with includes and removing comments //!
        /// </summary>
        private static string ExpandCode(string shaderCode, string workingFolder, HashSet<string> includedFiles)
        {
            //Just to be sure to always only have \n for new lines
            shaderCode = shaderCode.Replace("\r\n", "\n");


            shaderCode = SpecialCommentReplacement(shaderCode, "//!");
            if (includedFiles.Count == 0)
            {
                shaderCode = SpecialCommentReplacement(shaderCode, "//?");
            }
            
            return ExpandIncludes(shaderCode, includeName => GetIncludeCode(includeName, workingFolder, includedFiles));
        }

        /// <summary>
        /// Remove line
        /// </summary>
        private static string SpecialCommentReplacement(string code, string specialComment)
        {
            var lines = code.Split(NEW_LINE, StringSplitOptions.None); //if UNIX style line endings still working so do not use Envirnoment.NewLine
            for (int i = 0; i < lines.Length; ++i)
            {
                var index = lines[i].IndexOf(specialComment); // search for special comment
                if (-1 != index)
                {
                    lines[i] = lines[i].Substring(index + specialComment.Length); // remove everything before special comment
                }
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Returns the code for an include file
        /// </summary>
        private static string GetIncludeCode(string includeName, string workingFolder, HashSet<string> includedFiles)
        {
            
            string includeCode;

            string includeFileName = Path.GetFullPath(Path.Combine(workingFolder, includeName));
            if (!File.Exists(includeFileName))
            {
                //We will try in the resource...
                string resouceName = null;
                bool found = false;
                string ajustedName = includeName.Replace("\\", ".").Replace("/", ".");

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!ResourceUtils.IsAssemblyUsable(assembly))
                        continue;

                    string namespaceResouce = assembly.GetName().Name;
                    if (namespaceResouce.Equals("MiniEngine.Core", StringComparison.OrdinalIgnoreCase))
                        namespaceResouce = "MiniEngine";

                    

                    resouceName = namespaceResouce + "." + ajustedName;
                    if (ResourceUtils.IsResourceExists(resouceName))
                    {
                        found = true;
                        break;
                    }

                    resouceName = namespaceResouce + ".Resources." + ajustedName;
                    if (ResourceUtils.IsResourceExists(resouceName))
                    {
                        found = true;
                        break;
                    }

                    resouceName = namespaceResouce + ".Resources.Shaders." + ajustedName;
                    if (ResourceUtils.IsResourceExists(resouceName))
                    {
                        found = true;
                        break;
                    }


                }

                if(!found)
                    throw new FileNotFoundException("Include file not found: {includeFileName}");

                includeCode = ResourceUtils­.GetString(resouceName);
            }
            else
            {
                //Directly from file...
                includeCode = File.ReadAllText(includeFileName);
            }

            if (includedFiles.Contains(includeFileName))
            {
                return includeCode;
            }
            includedFiles.Add(includeFileName);

            return ExpandCode(includeCode, Path.GetDirectoryName(includeFileName), includedFiles: includedFiles);
            
        }

        /// <summary>
		/// Searches for #include statements in the shader code and replaces them by the code in the include resource.
		/// </summary>
		/// <param name="shaderCode">The shader code.</param>
		/// <param name="GetIncludeCode">Functor that will be called with the include path as parameter and returns the include shader code.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">GetIncludeCode</exception>
		private static string ExpandIncludes(string shaderCode, Func<string, string> GetIncludeCode)
        {
            var lines = shaderCode.Split(NEW_LINE, StringSplitOptions.None); //if UNIX style line endings still working so do not use Envirnoment.NewLine
            int lineNr = 1;
            foreach (var line in lines)
            {
                // Search for include pattern (e.g. #include raycast.glsl) (nested not supported)
                var match = _includeRegEx.Match(line);
                if (match.Success)
                {
                    var sFullMatch = match.Value;
                    var includeName = match.Groups[1].ToString(); // get the include
                    var includeCode = GetIncludeCode(includeName);
                    var lineNumberCorrection = $"\n#line {lineNr}\n";
                    shaderCode = shaderCode.Replace(sFullMatch, includeCode + lineNumberCorrection); // replace #include with actual include code
                }
                ++lineNr;
            }
            return shaderCode;
        }
    }
}
