using System;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Mesh
    /// </summary>
    public unsafe class Mesh: IDisposable
    {
        private readonly int VERTEX_SIZE = sizeof(Vertex);

        /// <summary>
        /// Vertex Array Object
        /// </summary>
        private uint _vao = uint.MaxValue;

        /// <summary>
        /// Vertices buffer
        /// </summary>
        private uint _vbo = uint.MaxValue;        

        /// <summary>
        /// Indices buffer
        /// </summary>
        private uint _ibo = uint.MaxValue;

        /// <summary>
        /// Vertices
        /// </summary>
        private Vertex[] _vertices;

        /// <summary>
        /// Indices
        /// </summary>
        private int[] _indices;


        /// <summary>
        /// Constructor
        /// </summary>
        public Mesh()
        {
            CreateBuffers();
        }

        /// <summary>
        /// Set the vertices
        /// </summary>
        public void SetVertices(Vertex[] vertices)
        {
            
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            if (_vbo == uint.MaxValue)
            {
                _vbo = GL.glGenBuffers();
                GL.CheckError();
            }

            _vertices = vertices;

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            GL.CheckError();

            fixed (Vertex* v = &_vertices[0])
            {
                GL.glBufferData(GL.GL_ARRAY_BUFFER, VERTEX_SIZE * _vertices.Length, v, GL.GL_STATIC_DRAW);
                GL.CheckError();
            }

            //Position...
            GL.glEnableVertexAttribArray(0);
            GL.CheckError();
            GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, false, VERTEX_SIZE, 0);
            GL.CheckError();

            //Color...
            GL.glEnableVertexAttribArray(1);
            GL.CheckError();
            GL.glVertexAttribPointer(1, 3, GL.GL_FLOAT, false, VERTEX_SIZE, 3 * sizeof(float));
            GL.CheckError();

            //TexCoord...
            GL.glEnableVertexAttribArray(2);
            GL.CheckError();
            GL.glVertexAttribPointer(2, 2, GL.GL_FLOAT, false, VERTEX_SIZE, 6 * sizeof(float));
            GL.CheckError();

            //Reset binding...
            GL.glBindVertexArray(0);
            GL.CheckError();

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, 0);
            GL.CheckError();

            GL.glDisableVertexAttribArray(0);
            GL.CheckError();
            GL.glDisableVertexAttribArray(1);
            GL.CheckError();
            GL.glDisableVertexAttribArray(3);
            GL.CheckError();

        }

        /// <summary>
        /// Set the indices
        /// </summary>
        public void SetIndices(int[] indices)
        {
            _indices = indices;

            GL.glBindVertexArray(_vao);
            GL.CheckError();

            if (_ibo == uint.MaxValue)
            {
                _ibo = GL.glGenBuffers();
                GL.CheckError();
            }

            GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ibo);
            GL.CheckError();

            fixed (int* pointer = &_indices[0])
            {
                GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, sizeof(int) * _indices.Length, pointer, GL.GL_STATIC_DRAW);
            }

            //Reset binding...
            GL.glBindVertexArray(0);
            GL.CheckError();

            GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, 0);
            GL.CheckError();

        }

        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render()
        {
            //Activate the vao...
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            //Rendering triangles...
            GL.glDrawElements(GL.GL_TRIANGLES, _indices.Length, GL.GL_UNSIGNED_INT, IntPtr.Zero);
            GL.CheckError();

            //Needed?
            //GL.glBindVertexArray(0);
            //GL.CheckError();

        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            if (_vao != uint.MaxValue)
            {
                GL.glDeleteVertexArrays(_vao);
                GL.CheckError();
                _vao = uint.MaxValue;
            }

            if (_vbo != uint.MaxValue)
            {
                GL.glDeleteBuffers(_vbo);
                GL.CheckError();
                _vbo = uint.MaxValue;
            }

            if (_ibo != uint.MaxValue)
            {
                GL.glDeleteBuffers(_ibo);
                GL.CheckError();
                _ibo = uint.MaxValue;
            }
        }

        /// <summary>
        /// Create the buffer for our mesh
        /// </summary>
        private void CreateBuffers()
        {
            GL.glBindVertexArray(0);

            _vao = GL.glGenVertexArrays();
            GL.CheckError();

            //GL.glBindVertexArray(_vao);
            //GL.CheckError();

            





            ////Reset binding...
            //GL.glBindVertexArray(0);
            //GL.glDisableVertexAttribArray(0);
            //GL.glDisableVertexAttribArray(1);
            //GL.glDisableVertexAttribArray(3);
        }


    }
}
