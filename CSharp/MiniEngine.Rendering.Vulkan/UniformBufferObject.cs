using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    struct UniformBufferObject
    {
        public Matrix4 model;
        public Matrix4 view;
        public Matrix4 proj;
    }
}
