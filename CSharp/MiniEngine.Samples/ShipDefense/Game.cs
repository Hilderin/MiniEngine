using MiniEngine;
using System;

namespace ShipDefense
{
    /// <summary>
    /// Main Game Class
    /// </summary>
    internal class Game
    {
        public Context Context;
        public Scene Scene;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game(Context context)
        {
            Context = context;
            Scene = context.Scene;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public void Init()
        {
            Scene.Camera.MoveBackward(3f);

            Scene.Add(new MeshObject()
            {
                Mesh = Primitives.CreateCubeMesh()
            });
        }


        /// <summary>
        /// Update each frame
        /// </summary>
        public void Update()
        {

            if (Context.Input.IsKeyDown(Keys.Escape))
                Context.Quit();
            if (Context.Input.IsKeyDown(Keys.F5))
                Reload();
        }

        /// <summary>
        /// Reload the scene
        /// </summary>
        public void Reload()
        {
            Scene.Clear();
            Init();
        }
    }
}
