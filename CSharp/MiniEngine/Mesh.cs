using System;
using System.Collections.Generic;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Mesh
    /// </summary>
    public unsafe class Mesh: IDisposable
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

        /// <summary>
        /// Meshes
        /// </summary>
        private int _nbMesh;

        /// <summary>
        /// Data on meshes
        /// </summary>
        private BasicMeshEntry[] _meshDatas;

        /// <summary>
        /// Array of all the positions of the vertices
        /// </summary>
        private Vector3[] _positions = new Vector3[0];

        /// <summary>
        /// Array of all the normals
        /// </summary>
        private Vector3[] _normals = new Vector3[0];

        /// <summary>
        /// Array of all the texture coords.
        /// </summary>
        private Vector2[] _texCoords = new Vector2[0];

        /// <summary>
        /// Array of all the indexes of the vertices
        /// </summary>
        private int[] _indices = new int[0];

        /// <summary>
        /// Materials
        /// </summary>
        private Material[] _materials;


        /// <summary>
        /// Last index that was set
        /// </summary>
        private int _lastIndexSetData = -1;

        /// <summary>
        /// Indicate if mesh has been initialized
        /// </summary>
        private bool _initialized = false;


        /// <summary>
        /// Shader
        /// </summary>
        public PhongShader Shader = new PhongShader();


        /// <summary>
        /// Materials
        /// </summary>
        public Material[] Materials
        {
            get { return _materials; }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public Mesh(): this(0, 1)
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Mesh(int nbMesh, int nbMaterial)
        {
            if (nbMaterial < 1)
                nbMaterial = 1;

            _nbMesh = nbMesh;
            _meshDatas = new BasicMeshEntry[nbMesh];
            _materials = new Material[nbMaterial];

            CreateBuffers();

            //Default material
            _materials[0] = Material.Empty;
        }

        /// <summary>
        /// Set the material
        /// </summary>
        public void SetMaterial(Material material, int index)
        {
            if (_materials.Length <= index)
                _materials = EnsureCapacityArray(_materials, index + 1);

            _materials[index] = material;
        }

        /// <summary>
        /// Set the mesh data...
        /// </summary>
        public void AddMeshData(Vector3[] positions,
                                Vector2[] texCoords,
                                Vector3[] normals,
                                int[] indices,
                                int materialIndex)
        {
            int index = ++_lastIndexSetData;

            if (_nbMesh <= index)
            {
                //Increase the size of the arrays...
                _nbMesh = index + 1;
                _meshDatas = EnsureCapacityArray(_meshDatas, _nbMesh);
                _positions = EnsureCapacityArray(_positions, _nbMesh);
                _texCoords = EnsureCapacityArray(_texCoords, _nbMesh);
                _normals = EnsureCapacityArray(_normals, _nbMesh);
                _indices = EnsureCapacityArray(_indices, _nbMesh);
            }

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
        /// Append an array
        /// </summary>
        private T[] EnsureCapacityArray<T>(T[] array, int capacity)
        {
            if (array.Length >= capacity)
                return array;

            T[] newArray = new T[capacity];
            Array.Copy(array, 0, newArray, 0, array.Length);

            return newArray;
        }



        /// <summary>
        /// Init the mesh
        /// </summary>
        public void Init()
        {

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

            _initialized = true;

        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render(RenderingContext renderingContext)
        {
            if (!_initialized)
            {
                Init();
                _initialized = true;
            }

            //Activate the vao...
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            for (int i = 0; i < _nbMesh; i++)
            {
                Material mat;
                if (_meshDatas[i].MaterialIndex < _materials.Length)
                    mat = _materials[_meshDatas[i].MaterialIndex];
                else
                    //No material...
                    mat = Material.Empty;

                if (mat.Diffuse != null)
                    mat.Diffuse.Bind(SHADER_COLOR_TEXTURE_UNIT);
                if (mat.Specular != null)
                    mat.Specular.Bind(SHADER_SPECULAR_EXPONENT_UNIT);


                Shader.Enable();

                Shader.SetWVP(ref renderingContext.WVPMatrix);
                Shader.SetAmbiantColor(ref renderingContext.AmbiantColor);
                Shader.SetSampler(0);
                Shader.SetAmbientIntensity(renderingContext.AmbientIntensity);
                Shader.SetMaterialAmbientColor(ref mat.AmbiantColor);

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

            //if (_shader != null)
            //{
            //    _shader.Dispose();
            //    _shader = null;
            //}
        }


        /// <summary>
        /// Informations for each mesh
        /// </summary>
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
