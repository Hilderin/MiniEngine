using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Information on an instance on the instance buffer
    /// </summary>
    internal struct VkInstanceStruct
    {
        public Vector3 Location;
        public Vector3 Rotation;
        public Vector3 Scale;
        public uint TextureIndex;

    }
}
