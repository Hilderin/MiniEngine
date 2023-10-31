using MiniEngine;
using MiniEngine.Components;
using System;
using Math = MiniEngine.Math;

namespace ShipDefense
{
    /// <summary>
    /// Main Game Class
    /// </summary>
    internal class Game
    {
        public Context Context;
        public Scene Scene;
        public CameraComponent CameraComponent;

        private bool _mustReload = false;
        private Mesh _mesh;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game(Context context)
        {
            Context = context;
            Scene = new Scene();
            Context.Asset.OnAssetChanged += Asset_OnAssetChanged;
        }


        /// <summary>
        /// Initialisation
        /// </summary>
        public void Init()
        {
            //CameraComponent = new CameraComponent();
            //CameraComponent.Camera = 
            //Camera.Current.Location = new Vector3(0f, 0, -3f);
            //Scene.Camera.RotatePitch(Math.DegToRad(15));
            //Scene.Camera.AddComponent<BasicMovementComponent>();

            var camera = Scene.Add(new CameraObject());
            camera.MoveBackward(3f);
            camera.AddComponent<BasicMovementComponent>();

            //if (Context.Scene != null)
            //    Context.Scene.Camera = Camera;
            Scene.Add(new MeshObject()
                        .SetMesh(Primitives.CreateCubeMesh())
                        .AddMaterial(Context.Asset.Get<Material>("materials/test"))
                      );
        }


        /// <summary>
        /// Update each frame
        /// </summary>
        public void Update()
        {

            if (Context.Input.IsKeyDown(Keys.Escape))
                Context.Quit();
            if (Context.Input.IsKeyDown(Keys.F5))
                _mustReload = true;


            if (_mustReload)
            {
                Reload();
                _mustReload = false;
            }
        }

        /// <summary>
        /// Reload the scene
        /// </summary>
        public void Reload()
        {
            Scene.Clear();
            Init();
        }


        /// <summary>
        /// Just reloading everything on asset change
        /// </summary>
        private void Asset_OnAssetChanged()
        {
            //Not on the same thread so we will wait the next frame
            _mustReload = true;
        }
    }
}
