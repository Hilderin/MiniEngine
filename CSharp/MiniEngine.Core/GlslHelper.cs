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
        /// Find the shader stage from file extension
        /// </summary>
        public static ShaderStage GetShaderStageFromPath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".vert": return ShaderStage.Vertex;
                case ".frag": return ShaderStage.Fragment;
                case ".comp": return ShaderStage.Compute;
                case ".tesc": return ShaderStage.TessellationControl;
                case ".tese": return ShaderStage.TessellationEvaluation;
                case ".geom": return ShaderStage.Geometry;
                default: throw new InvalidOperationException($"No shader stage corresponding to extension: {extension}");
            }
        }

        /// <summary>
        /// Get file extension for shader stage
        /// </summary>
        public static string GetFileExtensionShaderStage(ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Vertex: return ".vert";
                case ShaderStage.Fragment: return ".frag";
                case ShaderStage.TessellationControl: return ".tesc";
                case ShaderStage.TessellationEvaluation: return ".tese";
                case ShaderStage.Geometry: return ".geom";
                case ShaderStage.Compute: return ".comp";
                default: throw new NotSupportedException($"Stage shader not supported: {stage}");
            }
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
            if (!AssetManager.Current.TryFindAssetUri(includeName, workingFolder, out string includeAssetUri))
                throw new FileNotFoundException($"GLSL include file not found: {includeName}");

            string includeCode = AssetManager.Current.GetString(includeAssetUri);


            if (includedFiles.Contains(includeAssetUri))
            {
                return includeCode;
            }
            includedFiles.Add(includeAssetUri);

            return ExpandCode(includeCode, Path.GetDirectoryName(includeAssetUri), includedFiles: includedFiles);
            
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
