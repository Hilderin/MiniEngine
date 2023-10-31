using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A camera object
    /// </summary>
    public class CameraObject : GameObject
    {
        public CameraComponent CameraComponent;

        public CameraObject()
        {
            CameraComponent = AddComponent<CameraComponent>();
        }

    }
}
