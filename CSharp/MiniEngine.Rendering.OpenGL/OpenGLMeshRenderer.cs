using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiniEngine.Mesh;

namespace MiniEngine.Rendering.OpenGL
{
    /// <summary>
    /// Renderer for the meshes
    /// </summary>
    internal class OpenGLMeshRenderer: IMeshRenderer
    {
        private const int SHADER_POSITION_LOCATION = 0;
        private const int SHADER_TEX_COORD_LOCATION = 1;
        private const int SHADER_NORMAL_LOCATION = 2;
        private static uint SHADER_COLOR_TEXTURE_UNIT = GL.GL_TEXTURE0;
        private static int SHADER_COLOR_TEXTURE_UNIT_INDEX = 0;
        private static uint SHADER_SPECULAR_EXPONENT_UNIT = GL.GL_TEXTURE6;
        private static int SHADER_SPECULAR_EXPONENT_UNIT_INDEX = 6;

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
        /// Data on meshes
        /// </summary>
        private BasicMeshEntry[] _meshEntries;

        /// <summary>
        /// Materials
        /// </summary>
        private Material[] _materials;

        /// <summary>
        /// Datas for the sub meshes
        /// </summary>
        private List<SubMeshData> _subMeshes = new List<SubMeshData>();

        /// <summary>
        /// Shader
        /// </summary>
        private PhongShader _shader = new PhongShader();

        /// <summary>
        /// Number of meshes to draw
        /// </summary>
        private int _nbMesh = 0;

        /// <summary>
        /// List of materials
        /// </summary>
        public Material[] Materials { get { return _materials; } }


        /// <summary>
        /// Constructor
        /// </summary>
        public OpenGLMeshRenderer(Mesh mesh)
        {

            MeshData meshData = mesh.GetMeshData();

            _nbMesh = meshData.SubMeshes.Count;
            _subMeshes = meshData.SubMeshes;

            _meshEntries = new BasicMeshEntry[_nbMesh];

            List<Material> materials = new List<Material>(meshData.Materials);


            for (int i = 0; i < _nbMesh; i++)
            {
                AddMeshData(_subMeshes[i].Positions, _subMeshes[i].TexCoords, _subMeshes[i].Normals, _subMeshes[i].Indices, _subMeshes[i].MaterialIndex, i);

                //Check to be sure we have a material...
                while (materials.Count <= _subMeshes[i].MaterialIndex)
                    materials.Add(Material.NotFound);

            }

            //Creation of the array of material... (faster to acces!)
            _materials = meshData.Materials.ToArray();



            //Initialization of OpenGL...
            Init();

        }

        /// <summary>
        /// Init the mesh
        /// </summary>
        public void Init()
        {

            _vao = GL.glGenVertexArrays();
            GL.CheckError();


            GL.glBindVertexArray(_vao);
            GL.CheckError();


            //Registering the buffers array...
            GL.glGenBuffers(_buffers);
            GL.CheckError();

            ////Activate the vao...
            //GL.glBindVertexArray(_vao);
            //GL.CheckError();

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
        /// Render the mesh
        /// </summary>
        public void Render(OpenGLRenderer renderer)
        {
            
            //Activate the vao...
            GL.glBindVertexArray(_vao);
            GL.CheckError();

            for (int i = 0; i < _nbMesh; i++)
            {
                Material mat = _materials[_meshEntries[i].MaterialIndex];

                if (mat.Diffuse != null)
                    ((OpenGLTextureBinder)mat.Diffuse.Binder).Bind(SHADER_COLOR_TEXTURE_UNIT);
                if (mat.Specular != null)
                    ((OpenGLTextureBinder)mat.Specular.Binder).Bind(SHADER_SPECULAR_EXPONENT_UNIT);


                _shader.Enable();

                _shader.SetWVP(ref renderer.WVPMatrix);
                _shader.SetSampler(SHADER_COLOR_TEXTURE_UNIT_INDEX);
                _shader.SetSamplerSpecular(SHADER_SPECULAR_EXPONENT_UNIT_INDEX);

                _shader.SetAmbientColor(ref renderer.AmbientColor);
                _shader.SetAmbientIntensity(renderer.AmbientIntensity);
                _shader.SetMaterialAmbientColor(ref mat.AmbientColor);

                _shader.SetDiffuseColor(ref renderer.DiffuseColor);
                _shader.SetDiffuseIntensity(renderer.DiffuseIntensity);
                _shader.SetDiffuseDirection(ref renderer.CalculatedDiffuseDirection);
                _shader.SetMaterialDiffuseColor(ref mat.DiffuseColor);

                _shader.SetMaterialSpecularColor(ref mat.SpecularColor);
                _shader.SetCameraLocalPos(ref renderer.CameraLocalPosition);

                _shader.SetPointLights(renderer.PointLights, renderer.PointLightsCalulcatedLocalPositions);
                _shader.SetSpotLights(renderer.SpotLights, renderer.SpotLightsCalulcatedLocalPositions, renderer.SpotLightsCalulcatedLocalDirections);


                GL.glDrawElementsBaseVertex(GL.GL_TRIANGLES,
                                             _meshEntries[i].NumIndices,
                                             GL.GL_UNSIGNED_INT,
                                             sizeof(int) * _meshEntries[i].BaseIndex,
                                             _meshEntries[i].BaseVertex);
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

        ///// <summary>
        ///// Create the buffer for our mesh
        ///// </summary>
        //private void CreateBuffers()
        //{
        //    Clear();

        //    _vao = GL.glGenVertexArrays();
        //    GL.CheckError();


        //    GL.glBindVertexArray(_vao);
        //    GL.CheckError();


        //    //Registering the buffers array...
        //    GL.glGenBuffers(_buffers);
        //    GL.CheckError();



        //    //Reset binding...
        //    GL.glBindVertexArray(0);
        //    GL.CheckError();

        //    //if (_shader != null)
        //    //{
        //    //    __shader.Dispose();
        //    //    _shader = null;
        //    //}
        //}


        /// <summary>
        /// Set the mesh data...
        /// </summary>
        private void AddMeshData(Vector3[] positions,
                                Vector2[] texCoords,
                                Vector3[] normals,
                                int[] indices,
                                int materialIndex,
                                int index)
        {
            
           
            _meshEntries[index].MaterialIndex = materialIndex;
            _meshEntries[index].NumVertex = positions.Length;
            _meshEntries[index].NumIndices = indices.Length;

            if (index > 0)
            {
                _meshEntries[index].BaseVertex = _meshEntries[index - 1].BaseVertex + _meshEntries[index - 1].NumVertex;
                _meshEntries[index].BaseIndex = _meshEntries[index - 1].BaseIndex + _meshEntries[index - 1].NumIndices;

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
