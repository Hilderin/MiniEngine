using System;
using System.Collections.Generic;

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

        public MeshObject SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;
            return this;
        }


        public MeshObject AddMaterial(Material mat)
        {
            Materials.Add(mat);
            return this;
        }


    }




}
