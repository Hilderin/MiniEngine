using System;
using System.Collections.Generic;
using System.Data;

namespace MiniEngine
{
    /// <summary>
    /// MeshObject
    /// </summary>
    public unsafe class MeshObject : GameObject
    {

        public MeshComponent MeshComponent;

        public Mesh Mesh { get { return MeshComponent.Mesh; } set { MeshComponent.Mesh = value; } }
        public List<Material> Materials { get { return MeshComponent.Materials; } }


        public MeshObject()
        {
            MeshComponent = AddComponent<MeshComponent>();
        }

        public MeshObject SetMesh(string assetName)
        {
            return SetMesh(Context.Asset.Get<Mesh>(assetName));
        }

        public MeshObject SetMesh(Mesh mesh)
        {
            MeshComponent.SetMesh(mesh);

            return this;
        }


        public MeshObject SetMaterial(string assetName, int matIndex)
        {
            MeshComponent.SetMaterial(assetName, matIndex);

            return this;
        }

        public MeshObject SetMaterial(Material mat, int matIndex)
        {
            MeshComponent.SetMaterial(mat, matIndex);

            return this;
        }


    }




}
