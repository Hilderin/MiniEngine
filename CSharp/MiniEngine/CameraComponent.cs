using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A camera component
    /// </summary>
    public class CameraComponent : GameComponent
    {
        public Camera Camera;

        public CameraComponent()
        {
            Camera = new Camera();
        }

        /// <summary>
        /// Removing the camera
        /// </summary>
        protected override void OnDestroy()
        {
            //if (Context.Scene != null && Context.Scene.Camera == Camera)
            //    Context.Scene.Camera = null;
        }
    }
}
