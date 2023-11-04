using System;
using System.Diagnostics;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Cube
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {

            Context.LockCursor();


            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -1f);

            _currentMesh = PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(0f, 0f, 0f));
                                           //.AddMaterial(Context.Renderer.CreateMaterial(new()
                                           //             {
                                           //                 DiffuseTexture = BaseTextures.White,
                                           //                 Shader = shader
                                           //             }));


            Scene.Add(_currentMesh);


            //Mesh mesh2 = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            //{
            //    Scale = 3f,
            //    ResetMaterialAmbientColor = true
            //});
            //mesh2.Location = new Vector3(2f, 2f, 4f);
            //Context.Add(mesh2);


            //Context.AmbientLight.Intensity = 0.1f;

            //Context.DirectionalLight = new DirectionalLight()
            //{
            //    Rotation = Rotator3.FromDegrees(45, 90, 0)
            //};

            //Context.Add(new PointLight()
            //{
            //    Location = new Vector3(-8.0f, 0f, 0f),
            //    AttenuationLinear = 0.2f
            //});


            //var terrainMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\box_terrain.obj", new MeshImportationParameters()
            //{
            //    Scale = 1f,
            //    InverseFaces = false,
            //    SmoothNormals = false
            //});
            //terrainMesh.Location = new Vector3(0f, -1f, 0.0f);
            //Context.Add(terrainMesh);
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);


            //_currentMesh.RotateY(0.01f);

            System.Threading.Thread.Sleep(3);

        }

    }
}
