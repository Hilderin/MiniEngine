using System;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.OpenGL
{
    internal unsafe static class GLExtensions
    {
        /// <summary>
        ///     Generate buffer object names.
        /// </summary>
        /// <param name="n">Specifies the number of buffer object names to be generated.</param>
        /// <param name="buffers">Specifies an array in which the generated buffer object names are stored.</param>
        public static readonly glGenBuffersHandler glGenBuffers = GetDelegate<glGenBuffersHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGenBuffersHandler(int n, uint* buffers);

        /// <summary>
        ///     Bind a named buffer object.
        /// </summary>
        /// <param name="target">Specifies the target to which the buffer object is bound.</param>
        /// <param name="buffer">Specifies the name of a buffer object.</param>
        public static readonly glBindBufferHandler glBindBuffer = GetDelegate<glBindBufferHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glBindBufferHandler(uint target, uint buffer);

        /// <summary>
        ///     Deletes a single buffer object.
        /// </summary>
        /// <param name="buffer">A buffer to be deleted.</param>
        public static readonly glDeleteBuffersHandler glDeleteBuffers = GetDelegate<glDeleteBuffersHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDeleteBuffersHandler(int n, uint* buffers);

        /// <summary>
        ///     Creates and initializes a buffer object's data store.
        /// </summary>
        /// <param name="target">Specifies the target to which the buffer object is bound.</param>
        /// <param name="size">Specifies the size in bytes of the buffer object's new data store.</param>
        /// <param name="data">
        ///     Specifies a pointer to data that will be copied into the data store for initialization, or NULL if
        ///     no data is to be copied.
        /// </param>
        /// <param name="usage">
        ///     Specifies the expected usage pattern of the data store.
        ///     <para>
        ///         Must be GL_STREAM_DRAW, GL_STREAM_READ, GL_STREAM_COPY, GL_STATIC_DRAW, GL_STATIC_READ, GL_STATIC_COPY,
        ///         GL_DYNAMIC_DRAW, GL_DYNAMIC_READ, or GL_DYNAMIC_COPY.
        ///     </para>
        ///     .
        /// </param>
        public static readonly glBufferDataHandler glBufferData = GetDelegate<glBufferDataHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glBufferDataHandler(uint target, int size, void* data, uint usage);

        /// <summary>
        ///     Enable a generic vertex attribute array.
        /// </summary>
        /// <param name="index">Specifies the index of the generic vertex attribute to be disabled.</param>
        public static readonly glEnableVertexAttribArrayHandler glEnableVertexAttribArray = GetDelegate<glEnableVertexAttribArrayHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glEnableVertexAttribArrayHandler(uint index);

        /// <summary>
        ///     Define an array of generic vertex attribute data
        /// </summary>
        /// <param name="index">Specifies the index of the generic vertex attribute to be modified.</param>
        /// <param name="size">
        ///     Specifies the number of components per generic vertex attribute.
        ///     <para>Must be 1, 2, 3, 4, or <see cref="GL_BGRA" />.</para>
        /// </param>
        /// <param name="type">Specifies the data type of each component in the array.</param>
        /// <param name="normalized">
        ///     Specifies whether fixed-point data values should be normalized (true) or converted directly as
        ///     fixed-point values (false) when they are accessed.
        /// </param>
        /// <param name="stride">
        ///     Specifies the byte offset between consecutive generic vertex attributes.
        ///     <para>If stride is 0, the generic vertex attributes are understood to be tightly packed in the array.</para>
        /// </param>
        /// <param name="pointer">
        ///     Specifies a offset of the first component of the first generic vertex attribute in the array in
        ///     the data store of the buffer currently bound to the GL_ARRAY_BUFFER target.
        /// </param>
        public static readonly glVertexAttribPointerHandler glVertexAttribPointer = GetDelegate<glVertexAttribPointerHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glVertexAttribPointerHandler(uint index, int size, uint type, bool normalized, int stride, uint pointer);

        /// <summary>
        ///     Disable a generic vertex attribute array.
        /// </summary>
        /// <param name="index">Specifies the index of the generic vertex attribute to be disabled.</param>
        public static readonly glDisableVertexAttribArrayHandler glDisableVertexAttribArray = GetDelegate<glDisableVertexAttribArrayHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDisableVertexAttribArrayHandler(uint index);

        /// <summary>
        /// Permet d'obtenir les messages de debug
        /// Doit caller glEnable
        /// </summary>
        public static readonly glDebugMessageCallbackHandler glDebugMessageCallback = GetDelegate<glDebugMessageCallbackHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDebugMessageCallbackHandler(GL.DebugProc callback, void* userParam);

        /// <summary>
        ///     Generate vertex array object names.
        /// </summary>
        /// <param name="n">Specifies the number of vertex array object names to generate.</param>
        /// <param name="arrays">Specifies an array in which the generated vertex array object names are stored.</param>
        public static readonly glGenVertexArraysHandler glGenVertexArrays = GetDelegate<glGenVertexArraysHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGenVertexArraysHandler(int n, uint* arrays);

        /// <summary>
        ///     Delete vertex array objects.
        /// </summary>
        /// <param name="n">Specifies the number of vertex array objects to be deleted.</param>
        /// <param name="arrays">Specifies the address of an array containing the n names of the objects to be deleted.</param>
        public static readonly glDeleteVertexArraysHandler glDeleteVertexArrays = GetDelegate<glDeleteVertexArraysHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDeleteVertexArraysHandler(int n, uint* arrays);

        /// <summary>
        ///     Bind a vertex array object.
        /// </summary>
        /// <param name="array">Specifies the name of the vertex array to bind.</param>
        public static readonly glBindVertexArrayHandler glBindVertexArray = GetDelegate<glBindVertexArrayHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glBindVertexArrayHandler(uint array);

        /// <summary>
        ///     Compiles a shader object.
        /// </summary>
        /// <param name="shader">Specifies the shader object to be compiled.</param>
        public static readonly glCompileShaderHandler glCompileShader = GetDelegate<glCompileShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glCompileShaderHandler(uint shader);

        /// <summary>
        ///     Creates a shader program object.
        /// </summary>
        /// <returns>An empty program object, a non-zero value by which it can be referenced.</returns>
        public static readonly glCreateProgramHandler glCreateProgram = GetDelegate<glCreateProgramHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint glCreateProgramHandler();

        /// <summary>
        ///     Creates a shader object.
        /// </summary>
        /// <param name="type">Specifies the type of shader to be created.<para>Must be one of GL_VERTEX_SHADER, GL_GEOMETRY_SHADER, or GL_FRAGMENT_SHADER.</para></param>
        /// <returns>An empty shader object, a non-zero value by which it can be referenced.</returns>
        public static readonly glCreateShaderHandler glCreateShader = GetDelegate<glCreateShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint glCreateShaderHandler(uint type);

        /// <summary>
        ///     Determines if a name corresponds to a program object.
        /// </summary>
        /// <param name="program">The potential program object to check.</param>
        /// <returns><c>true</c> if object is a program, otherwise <c>false</c>.</returns>
        public static readonly glIsProgramHanlder glIsProgram = GetDelegate<glIsProgramHanlder>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool glIsProgramHanlder(uint program);

        /// <summary>
        ///     Determines if a name corresponds to a shader object.
        /// </summary>
        /// <param name="shader">The potential program object to check.</param>
        /// <returns><c>true</c> if object is a shader, otherwise <c>false</c>.</returns>
        public static readonly glIsShaderHandler glIsShader = GetDelegate<glIsShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool glIsShaderHandler(uint shader);

        /// <summary>
        ///     Deletes a program object.
        /// </summary>
        /// <param name="program">Specifies the program object to be deleted.</param>
        public static readonly glDeleteProgramHandler glDeleteProgram = GetDelegate<glDeleteProgramHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDeleteProgramHandler(uint program);

        /// <summary>
        ///     Deletes a shader object.
        /// </summary>
        /// <param name="shader">Specifies the shader object to be deleted.</param>
        public static readonly glDeleteShaderHandler glDeleteShader = GetDelegate<glDeleteShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDeleteShaderHandler(uint shader);

        /// <summary>
        ///     Detaches a shader object from a program object to which it is attached.
        /// </summary>
        /// <param name="program">Specifies the program object from which to detach the shader object.</param>
        /// <param name="shader">Specifies the shader object to be detached.</param>
        public static readonly glDetachShaderHandler glDetachShader = GetDelegate<glDetachShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDetachShaderHandler(uint program, uint shader);


        /// <summary>
        ///      Replaces the source code in a shader object.
        /// </summary>
        /// <param name="shader">Specifies the handle of the shader object whose source code is to be replaced.</param>
        /// <param name="count">Specifies the number of elements in the string and length arrays.</param>
        /// <param name="str">Specifies an array of pointers to strings containing the source code to be loaded into the shader.</param>
        /// <param name="length">Specifies an array of string lengths.</param>
        public static readonly glShaderSourceHandler glShaderSource = GetDelegate<glShaderSourceHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glShaderSourceHandler(uint shader, int count, /*const*/ byte** str, /*const*/ int* length);


        /// <summary>
        ///      Replaces the source code in a shader object.
        /// </summary>
        /// <param name="shader">Specifies the handle of the shader object whose source code is to be replaced.</param>
        /// <param name="count">Specifies the number of elements in the string and length arrays.</param>
        /// <param name="str">Specifies an array of pointers to strings containing the source code to be loaded into the shader.</param>
        /// <param name="length">Specifies an array of string lengths.</param>
        public static readonly glAttachShaderHandler glAttachShader = GetDelegate<glAttachShaderHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glAttachShaderHandler(uint program, uint shader);

        /// <summary>
        ///     Links a program object.
        /// </summary>
        /// <param name="program">Specifies the handle of the program object to be linked.</param>
        public static readonly glLinkProgramHandler glLinkProgram = GetDelegate<glLinkProgramHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glLinkProgramHandler(uint program);



        /// <summary>
        ///     Installs a program object as part of current rendering state.
        /// </summary>
        /// <param name="program">Specifies the handle of the program object whose executables are to be used as part of current rendering state.</param>
        public static readonly glUseProgramHandler glUseProgram = GetDelegate<glUseProgramHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUseProgramHandler(uint program);



        /// <summary>
        /// Return a parameter from a program object.
        /// </summary>
        /// <param name="program">Specifies the program object to be queried.</param>
        /// <param name="pname">Specifies the object parameter.</param>
        /// <param name="args">Returns the requested object parameter.</param>
        public static readonly glGetProgramivHandler glGetProgramiv = GetDelegate<glGetProgramivHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGetProgramivHandler(uint program, uint pname, int* args);

        /// <summary>
        ///     Returns the information log for a program object.
        /// </summary>
        /// <param name="program">Specifies the program object whose information log is to be queried.</param>
        /// <param name="bufSize">Specifies the size of the character buffer for storing the returned information log.</param>
        /// <returns>The info log, or <c>null</c> if an error occured.</returns>
        public static readonly glGetProgramInfoLogHandler glGetProgramInfoLog = GetDelegate<glGetProgramInfoLogHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGetProgramInfoLogHandler(uint program, int bufSize, int* length, byte* infoLog);

        /// <summary>
        ///     Validates a program object.
        /// </summary>
        /// <param name="program">Specifies the handle of the program object to be validated.</param>
        /// <seealso cref="glGetProgramInfoLog"/>
        public static readonly glValidateProgramHandler glValidateProgram = GetDelegate<glValidateProgramHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glValidateProgramHandler(uint program);

        /// <summary>
        /// Return a parameter from a shader object.
        /// </summary>
        /// <param name="shader">Specifies the shader object to be queried.</param>
        /// <param name="pname">Specifies the object parameter.<para>Must be GL_SHADER_TYPE, GL_DELETE_STATUS, GL_COMPILE_STATUS, GL_INFO_LOG_LENGTH, or GL_SHADER_SOURCE_LENGTH.</para></param>
        /// <param name="args">Returns the requested object parameter.</param>
        public static readonly glGetShaderivHandler glGetShaderiv = GetDelegate<glGetShaderivHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGetShaderivHandler(uint shader, uint pname, int* args);

        /// <summary>
        ///     Returns the information log for a program object.
        /// </summary>
        /// <param name="program">Specifies the program object whose information log is to be queried.</param>
        /// <param name="bufSize">Specifies the size of the character buffer for storing the returned information log.</param>
        /// <returns>The info log, or <c>null</c> if an error occured.</returns>
        public static readonly glGetShaderInfoLogHandler glGetShaderInfoLog = GetDelegate<glGetShaderInfoLogHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glGetShaderInfoLogHandler(uint shader, int bufSize, int* length, byte* infoLog);


        /// <summary>
        ///      Returns the location of a uniform variable.
        /// </summary>
        /// <param name="program">Specifies the program object to be queried.</param>
        /// <param name="name">Points to a null terminated string containing the name of the uniform variable whose location is to be queried.</param>
        /// <returns>An integer that represents the location of a specific uniform variable within a program object.</returns>
        public static readonly glGetUniformLocationHandler glGetUniformLocation = GetDelegate<glGetUniformLocationHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int glGetUniformLocationHandler(uint program, /*const*/ byte* name);

        /// <summary>
        ///     Specify the value of a uniform variable for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform value to be modified.</param>
        /// <param name="v0">The value.</param>
        public static readonly glUniform1fHandler glUniform1f = GetDelegate<glUniform1fHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUniform1fHandler(int location, float v0);

        /// <summary>
        ///     Specify the value of 2 uniform variables for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform value to be modified.</param>
        /// <param name="v0">The first value.</param>
        /// <param name="v1">The second value.</param>
        public static readonly glUniform2fHandler glUniform2f = GetDelegate<glUniform2fHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUniform2fHandler(int location, float v0, float v1);

        // <summary>
        ///     Specify the value of 3 uniform variables for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform value to be modified.</param>
        /// <param name="v0">The first value.</param>
        /// <param name="v1">The second value.</param>
        /// <param name="v1">The third value.</param>
        public static readonly glUniform3fHandler glUniform3f = GetDelegate<glUniform3fHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUniform3fHandler(int location, float v0, float v1, float v3);


        /// <summary>
        ///     Specify the value of a uniform variable for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform value to be modified.</param>
        /// <param name="v0">The value.</param>
        public static readonly glUniform1iHandler glUniform1i = GetDelegate<glUniform1iHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUniform1iHandler(int location, int v0);

        /// <summary>
        ///     Specify the value of a uniform variable for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform value to be modified.</param>
        /// <param name="v0">The value.</param>
        public static readonly glUniformMatrix4fvHandler glUniformMatrix4fv = GetDelegate<glUniformMatrix4fvHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glUniformMatrix4fvHandler(int location, int count, bool transpose, /*const*/ float* value);


        /// <summary>
        ///     Select active texture unit.
        /// </summary>
        /// <param name="texture">Specifies which texture unit to make active.</param>
        public static readonly glActiveTextureHandler glActiveTexture = GetDelegate<glActiveTextureHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glActiveTextureHandler(uint textureUnit);

        /// <summary>
        ///     Select active texture unit.
        /// </summary>
        /// <param name="texture">Specifies which texture unit to make active.</param>
        public static readonly glDrawElementsBaseVertexHandler glDrawElementsBaseVertex = GetDelegate<glDrawElementsBaseVertexHandler>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void glDrawElementsBaseVertexHandler(uint mode, int count, uint type, int indices, int baseVertex);



        /// <summary>
        /// Find the method and create a delegate
        /// </summary>
        private static T GetDelegate<T>() where T : Delegate
        {
            string name = typeof(T).Name;

            //Removing "Handler..."
            name = name.Substring(0, name.Length - "Handler".Length);

            nint proc = GL.wglGetProcAddress(name);
            if (proc == nint.Zero)
                throw new Exception("Extension function " + name + " not supported or method GL called before initializing OpenGL.");

            //  Get the delegate for the function pointer.
            Delegate del = Marshal.GetDelegateForFunctionPointer(proc, typeof(T));
            if (del == null)
                throw new Exception("Extension function " + name + " marshalled incorrectly");

            return (T)del;
        }


    }
}
