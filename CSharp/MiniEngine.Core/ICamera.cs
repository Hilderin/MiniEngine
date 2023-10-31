using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Camera
    /// </summary>
    public interface ICamera
    {
        Matrix4 GetViewMatrix();

        Matrix4 GetProjectionMatrixVulkan(int clientSizeX, int clientSizeY);
    }
}
