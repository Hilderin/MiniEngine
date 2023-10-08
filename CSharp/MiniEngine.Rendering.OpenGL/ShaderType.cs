using MiniEngine.Rendering.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Shader type
    /// </summary>
    public enum ShaderType : uint
    {
        /// <summary>
        /// Vertex shader
        /// </summary>
        Vertex = GL.GL_VERTEX_SHADER,

        /// <summary>
        /// Fragment shader
        /// </summary>
        Fragment = GL.GL_FRAGMENT_SHADER
    }

}
