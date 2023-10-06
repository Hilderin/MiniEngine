using System;
using System.Collections.Generic;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Mesh
    /// </summary>
    public unsafe class Mesh2: IDisposable
    {
        private const int SHADER_POSITION_LOCATION = 0;
        private const int SHADER_TEX_COORD_LOCATION = 1;
        private const int SHADER_NORMAL_LOCATION = 2;
        private static uint SHADER_COLOR_TEXTURE_UNIT = GL.GL_TEXTURE0;
        private static uint SHADER_SPECULAR_EXPONENT_UNIT = GL.GL_TEXTURE6;

        private const int INDEX_BUFFER = 0;
        private const int POS_VB = 1;
        private const int TEXCOORD_VB = 2;
        private const int NORMAL_VB = 3;
        private const int WVP_MAT_VB = 4;  // required only for instancing
        private const int WORLD_MAT_VB = 5;  // required only for instancing
        private const int NB_BUFFERS = 6;

        /// <summary>
        /// Vertex Array Object
        /// </summary>
        private uint _vao = uint.MaxValue;

        /// <summary>
        /// Buffers
        /// </summary>
        private uint[] _buffers = new uint[NB_BUFFERS];

        ///// <summary>
        ///// Indices buffer
        ///// </summary>
        //private uint _ibo = uint.MaxValue;

        /// <summary>
        /// Meshes
        /// </summary>
        //private MeshInfo[] _meshes;
        private int _nbMesh;
        private BasicMeshEntry[] _meshDatas;

        private Vector3[] _positions = new Vector3[0];
        private Vector3[] _normals = new Vector3[0];
        private Vector2[] _texCoords = new Vector2[0];
        private int[] _indices = new int[0];

        /// <summary>
        /// Materials
        /// </summary>
        private Material[] _materials;

        /// <summary>
        /// Shader
        /// </summary>
        private Shader _shader;

        /// <summary>
        /// Last index that was set
        /// </summary>
        private int _lastIndexSetData = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Mesh2(int nbMesh, int nbMaterial)
        {
            //_meshes = new MeshInfo[nbMesh];
            _nbMesh = nbMesh;
            _meshDatas = new BasicMeshEntry[nbMesh];
            _materials = new Material[nbMaterial];

            CreateBuffers();
        }

        /// <summary>
        /// Set the material
        /// </summary>
        public void SetMaterial(Material material, int index)
        {
            _materials[index] = material;
        }

        /// <summary>
        /// Set the mesh data...
        /// </summary>
        public void SetMeshData(Vector3[] positions,
                            Vector2[] texCoords,
                            Vector3[] normals,
                            int[] indices,
                            int materialIndex,
                            int index)
        {
            if (index != _lastIndexSetData + 1)
                throw new ArgumentException($"Invalid index {index}, expected {_lastIndexSetData + 1}");

            _meshDatas[index].MaterialIndex = materialIndex;
            _meshDatas[index].NumVertex = positions.Length;
            _meshDatas[index].NumIndices = indices.Length;

            if (index > 0)
            {
                _meshDatas[index].BaseVertex = _meshDatas[index - 1].BaseVertex + _meshDatas[index - 1].NumVertex;
                _meshDatas[index].BaseIndex = _meshDatas[index - 1].BaseIndex + _meshDatas[index - 1].NumIndices;

                //Growing arrays...
                _positions = AppendArray(positions, _positions);
                _texCoords = AppendArray(texCoords, _texCoords);
                _normals = AppendArray(normals, _normals);
                _indices = AppendArray(indices, _indices);

            }
            else
            {
                _positions = positions;
                _texCoords = texCoords;
                _normals = normals;
                _indices = indices;
            }

            _lastIndexSetData = index;
        }

        /// <summary>
        /// Append an array
        /// </summary>
        private T[] AppendArray<T>(T[] source, T[] dest)
        {
            if (dest.Length == 0)
                return source;

            T[] newArray = new T[dest.Length + source.Length];
            Array.Copy(dest, 0, newArray, 0, dest.Length);
            Array.Copy(source, 0, newArray, dest.Length, source.Length);

            return newArray;
        }



        /// <summary>
        /// Init the mesh
        /// </summary>
        public void Init()
        {

            //Creation of the shader...
            InitShader();

            //Activate the vao...
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _buffers[POS_VB]);
            GL.CheckError();

            GL.glBufferData(GL.GL_ARRAY_BUFFER, _positions, GL.GL_STATIC_DRAW);
            GL.CheckError();

            GL.glEnableVertexAttribArray(SHADER_POSITION_LOCATION);
            GL.CheckError();

            GL.glVertexAttribPointer(SHADER_POSITION_LOCATION, 3, GL.GL_FLOAT, false, 0, 0);
            GL.CheckError();

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _buffers[TEXCOORD_VB]);
            GL.CheckError();

            GL.glBufferData(GL.GL_ARRAY_BUFFER, _texCoords, GL.GL_STATIC_DRAW);
            GL.CheckError();

            GL.glEnableVertexAttribArray(SHADER_TEX_COORD_LOCATION);
            GL.CheckError();

            GL.glVertexAttribPointer(SHADER_TEX_COORD_LOCATION, 2, GL.GL_FLOAT, false, 0, 0);
            GL.CheckError();

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, _buffers[NORMAL_VB]);
            GL.CheckError();

            GL.glBufferData(GL.GL_ARRAY_BUFFER, _normals, GL.GL_STATIC_DRAW);
            GL.CheckError();

            GL.glEnableVertexAttribArray(SHADER_NORMAL_LOCATION);
            GL.CheckError();

            GL.glVertexAttribPointer(SHADER_NORMAL_LOCATION, 3, GL.GL_FLOAT, false, 0, 0);
            GL.CheckError();


            GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _buffers[INDEX_BUFFER]);
            GL.CheckError();

            GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _indices, GL.GL_STATIC_DRAW);
            GL.CheckError();

            //Reset binding...
            GL.glBindVertexArray(0);
            GL.CheckError();


        }

        /// <summary>
        /// Init the shader
        /// </summary>
        private void InitShader()
        {
            _shader = new Shader(
@"#version 330

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoord;

uniform mat4 gWVP;

out vec2 TexCoord0;

void main()
{
    gl_Position = gWVP * vec4(Position, 1.0);
    TexCoord0 = TexCoord;
}

"
, @"#version 330

in vec2 TexCoord0;

out vec4 FragColor;

uniform sampler2D gSampler;

void main()
{
    FragColor = texture2D(gSampler, TexCoord0);
}

");
        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render(Matrix4 wvpMatrix)
        {
            //Activate the vao...
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            for (int i = 0; i < _nbMesh; i++)
            {

                Material mat = _materials[_meshDatas[i].MaterialIndex];

                if (mat.Diffuse != null)
                    mat.Diffuse.Bind(SHADER_COLOR_TEXTURE_UNIT);
                if (mat.Specular != null)
                    mat.Specular.Bind(SHADER_SPECULAR_EXPONENT_UNIT);


                _shader.SetUniform("gWVP", wvpMatrix);
                _shader.SetUniform("gSampler", 0);


                GL.glDrawElementsBaseVertex(GL.GL_TRIANGLES,
                                             _meshDatas[i].NumIndices,
                                             GL.GL_UNSIGNED_INT,
                                             sizeof(int) * _meshDatas[i].BaseIndex,
                                             _meshDatas[i].BaseVertex);
                GL.CheckError();
            }
        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Clear the mesh data and buffers
        /// </summary>
        public void Clear()
        {
            if (_vao != uint.MaxValue)
            {
                GL.glDeleteVertexArrays(_vao);
                GL.CheckError();
                _vao = uint.MaxValue;


                //Reset buffers...
                GL.glDeleteBuffers(_buffers);
                GL.CheckError();

            }
        }

        /// <summary>
        /// Create the buffer for our mesh
        /// </summary>
        private void CreateBuffers()
        {
            Clear();

            _vao = GL.glGenVertexArrays();
            GL.CheckError();


            GL.glBindVertexArray(_vao);
            GL.CheckError();


            //Registering the buffers array...
            GL.glGenBuffers(_buffers);
            GL.CheckError();



            //Reset binding...
            GL.glBindVertexArray(0);
            GL.CheckError();

            if (_shader != null)
            {
                _shader.Dispose();
                _shader = null;
            }
        }


        ///// <summary>
        ///// Structure for the mesh info
        ///// </summary>
        //private struct MeshInfo
        //{
        //    Vector3[] Positions;
        //    Vector3[] TexCoords;
        //    Vector3[] Normals;
        //}


        private struct BasicMeshEntry
        {
            public int NumVertex;
            public int NumIndices;
            public int BaseVertex;
            public int BaseIndex;
            public int MaterialIndex;

            public BasicMeshEntry()
            {
                NumIndices = 0;
                BaseVertex = 0;
                BaseIndex = 0;
                MaterialIndex = int.MaxValue;
            }


        };


    }
}
