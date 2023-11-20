using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests.Mocks
{
    internal class MockMesh : Mesh
    {
        public MeshDefinition MeshDefinition;

        public override Mesh Load(MeshDefinition meshDef)
        {
            MeshDefinition = meshDef;

            return this;
        }

        protected override void Destroy()
        {
            
        }
    }
}
