using System;

namespace MiniEngine.Labs.Renderer
{
    public static class LabHelper
    {

        /// <summary>
        /// Process inputs for testing
        /// </summary>
        public static void ProcessInputsTest(Context context)
        {
            context.Scene.Camera.MoveInDirections(0.1f, context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            //if (context.Input.IsKeyDown(Keys.NumpadAdd))
            //    context.Scene.AmbientLight.Intensity += 0.01f;
            //if (context.Input.IsKeyDown(Keys.NumpadSubtract))
            //    context.Scene.AmbientLight.Intensity -= 0.01f;
            if (context.Input.IsKeyDown(Keys.Z))
                context.Scene.Camera.RotateYaw(-0.1f);
            if (context.Input.IsKeyDown(Keys.X))
                context.Scene.Camera.RotateYaw(0.1f);
            if (context.Input.IsKeyDown(Keys.C))
                context.Scene.Camera.RotatePitch(-0.1f);
            if (context.Input.IsKeyDown(Keys.V))
                context.Scene.Camera.RotatePitch(0.1f);
            if (context.Input.IsKeyDown(Keys.R))
                context.Scene.Camera.RotateRoll(-0.1f);
            if (context.Input.IsKeyDown(Keys.F))
                context.Scene.Camera.RotateRoll(0.1f);

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

            //if (context.Input.IsJustMouseMoved)
            //{
            //    Vector2 mouseMovement = context.Input.MouseMovement;
            //    Camera.RotatePitch(mouseMovement.Y * -0.1f);
            //    //Camera.RotateYaw(mouseMovement.X * 0.1f);
            //    Debug.Print(mouseMovement.ToString());
            //}
        }

    }
}
