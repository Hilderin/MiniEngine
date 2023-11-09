using System;

namespace MiniEngine.Labs.Renderer
{
    public static class LabHelper
    {

        private const float MOVEMENT_DISTANCE_PER_SEC = 3f;

        /// <summary>
        /// Process inputs for testing
        /// </summary>
        public static void ProcessInputsTest(Context context)
        {
            //Scene scene = SceneManager.Current.CurrentScene;
            Camera camera = MiniEngine.Renderer.Current.Camera;

            camera.Transform.MoveInDirections(MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime, context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            //if (context.Input.IsKeyDown(Keys.NumpadAdd))
            //    scene.AmbientLight.Intensity += 0.01f;
            //if (context.Input.IsKeyDown(Keys.NumpadSubtract))
            //    scene.AmbientLight.Intensity -= 0.01f;
            if (context.Input.IsKeyDown(Keys.Z))
                camera.Transform.RotateYaw(-MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);
            if (context.Input.IsKeyDown(Keys.X))
                camera.Transform.RotateYaw(MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);
            if (context.Input.IsKeyDown(Keys.C))
                camera.Transform.RotatePitch(-MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);
            if (context.Input.IsKeyDown(Keys.V))
                camera.Transform.RotatePitch(MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);
            if (context.Input.IsKeyDown(Keys.R))
                camera.Transform.RotateRoll(-MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);
            if (context.Input.IsKeyDown(Keys.F))
                camera.Transform.RotateRoll(MOVEMENT_DISTANCE_PER_SEC * Time.DeltaTime);

            if (context.Input.IsJustMouseDown(MouseButton.Right))
            {
                context.LockCursor();
            }
            if (context.Input.IsJustMouseUp(MouseButton.Right))
            {
                context.ShowCursor();
            }

            //if (context.Input.IsKeyDown(Keys.PageUp))
            //{
            //    if (Scene.DirectionalLight != null)
            //        Scene.DirectionalLight.Intensity += 0.01f;
            //}
            //if (context.Input.IsKeyDown(Keys.PageDown))
            //{
            //    if (Scene.DirectionalLight != null)
            //        Scene.DirectionalLight.Intensity -= 0.01f;
            //}
            //Scene.AmbientLight.Intensity = Math.Clamp(Scene.AmbientLight.Intensity, 0f, 1f);
            //if (Scene.DirectionalLight != null)
            //    Scene.DirectionalLight.Intensity = Math.Clamp(Scene.DirectionalLight.Intensity, 0f, 1f);

            if (context.Input.IsJustMouseMoved && context.Input.IsMouseDown(MouseButton.Right))
            {
                Vector2 mouseMovement = context.Input.MouseMovement;
                //mouseMovement.Normalize();
                camera.Transform.RotateYaw(mouseMovement.X * 10f * Time.DeltaTime);
                camera.Transform.RotatePitch(mouseMovement.Y * -10f * Time.DeltaTime);
                //Camera.RotateYaw(mouseMovement.X * 0.1f);
                //Debug.Info(context.Input.MousePosition.ToString());

                //context.ShowCursor();
                //context.LockCursor();
            }
//            Debug.Info(context.Input.MouseMovement.ToString());
        }

    }
}
