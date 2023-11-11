using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Helper for the shader
    /// </summary>
    public static class ShaderCompiler
    {
        /// <summary>
        /// Compile a shader
        /// </summary>
        public static byte[] Compile(string code, ShaderStageFlags stageFlags)
        {

            string rootSDK = Environment.GetEnvironmentVariable("VULKAN_SDK");

            if (String.IsNullOrEmpty(rootSDK))
                throw new Exception("Vulkan SDK not found, environment variable VULKAN_SDK not set.");

            if(!Directory.Exists(rootSDK))
                throw new Exception($"Vulkan SDK not found, path not found: {rootSDK} (from VULKAN_SDK environment variable)");
            
            string pathglslcexe = Path.Combine(rootSDK, "bin\\glslc.exe");
            if (!File.Exists(pathglslcexe))
                throw new Exception($"Vulkan glslc.exe not found in: {pathglslcexe}");

            string extension;
            switch (stageFlags)
            {
                case ShaderStageFlags.Vertex: extension = ".vert"; break;
                case ShaderStageFlags.Fragment: extension = ".frag"; break;
                case ShaderStageFlags.TessellationControl: extension = ".tesc"; break;
                case ShaderStageFlags.TessellationEvaluation: extension = ".tese"; break;
                case ShaderStageFlags.Geometry: extension = ".geom"; break;
                case ShaderStageFlags.Compute: extension = ".comp"; break;
                default: throw new NotSupportedException($"Stage shader not supported: {stageFlags}");
            }


            string tempFileCode = Path.GetTempFileName() + extension;
            string tempFileSpv = Path.GetTempFileName();
            try
            {
                //Writing the code on disk...
                File.WriteAllText(tempFileCode, code);

                using (Process p = new Process())
                {
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = pathglslcexe;
                    p.StartInfo.Arguments = $"\"{tempFileCode}\" -o \"{tempFileSpv}\"";

                    p.Start();

                    string stdoutx = p.StandardOutput.ReadToEnd();
                    string stderrx = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        throw new Exception("Shader compilation error: " + stderrx);
                    }
                }

                if (!File.Exists(tempFileSpv))
                    throw new Exception("Shader compilating ended without error but the spv file was not created.");

                return File.ReadAllBytes(tempFileSpv);

            }
            catch
            {
                try
                {
                    if (File.Exists(tempFileCode))
                        File.Delete(tempFileCode);
                }
                catch
                { }

                try
                {
                    if (File.Exists(tempFileSpv))
                        File.Delete(tempFileSpv);
                }
                catch
                { }

                throw;

            }


        }
    }
}
