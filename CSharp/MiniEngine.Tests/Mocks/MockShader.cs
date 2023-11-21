using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests.Mocks
{
    internal class MockShader : Shader
    {
        public override Shader Load(ShaderDefinition shaderDef)
        {
            return this;
        }

        protected override void Destroy()
        {
            
        }
    }
}
