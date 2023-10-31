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
        public Camera Camera { get; private set; } = new Camera();

        public CameraComponent()
        {
            Context.RegisterOnce(Init);
            
        }

        /// <summary>
        /// Removing the camera
        /// </summary>
        protected override void OnDestroy()
        {
            if (Renderer.Current == Camera)
                Renderer.Current.Camera = null;
        }

        /// <summary>
        /// Init
        /// </summary>
        private void Init()
        {
            //Parent.OnLocationChanged += Transform_OnLocationChanged;
            //Parent.OnRotationChanged += Transform_OnRotationChanged;
            //Parent.OnScaleChanged += Transform_OnScaleChanged;

            //We change the current camera...
            Renderer.Current.Camera = this.Camera;

            //And we attache the same Transform then the parent
            this.Camera.Transform = this.Parent.Transform;
        }


        //private void Transform_OnLocationChanged(Vector3 oldLocation, Vector3 newLocation)
        //{
        //    Camera.Location = newLocation;
        //}

        //private void Transform_OnRotationChanged(Rotator3 oldRotation, Rotator3 newRotation)
        //{
        //    Camera.Rotation = newRotation;
        //}

        //private void Transform_OnScaleChanged(Vector3 oldLocation, Vector3 newLocation)
        //{
        //    Camera.Location = newLocation;
        //}
    }
}
