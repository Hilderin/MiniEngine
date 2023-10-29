
namespace MiniEngine.Components
{
    /// <summary>
    /// Basic movement
    /// </summary>
    public class BasicMovementComponent: GameComponent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BasicMovementComponent()
        {
            Context.RegisterUpdate(Update);
        }

        public void Update()
        {
            Parent.MoveInDirections(0.1f, Context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            //if (Context.Input.IsKeyDown(Keys.NumpadAdd))
            //    Context.Scene.AmbientLight.Intensity += 0.01f;
            //if (Context.Input.IsKeyDown(Keys.NumpadSubtract))
            //    Context.Scene.AmbientLight.Intensity -= 0.01f;
            if (Context.Input.IsKeyDown(Keys.Z))
                Parent.RotateYaw(-0.1f);
            if (Context.Input.IsKeyDown(Keys.X))
                Parent.RotateYaw(0.1f);
            if (Context.Input.IsKeyDown(Keys.C))
                Parent.RotatePitch(-0.1f);
            if (Context.Input.IsKeyDown(Keys.V))
                Parent.RotatePitch(0.1f);
            if (Context.Input.IsKeyDown(Keys.R))
                Parent.RotateRoll(-0.1f);
            if (Context.Input.IsKeyDown(Keys.F))
                Parent.RotateRoll(0.1f);

            //if (Context.Input.IsKeyDown(Keys.PageUp))
            //{
            //    if (Scene.DirectionalLight != null)
            //        Scene.DirectionalLight.Intensity += 0.01f;
            //}
            //if (Context.Input.IsKeyDown(Keys.PageDown))
            //{
            //    if (Scene.DirectionalLight != null)
            //        Scene.DirectionalLight.Intensity -= 0.01f;
            //}
            //Scene.AmbientLight.Intensity = Math.Clamp(Scene.AmbientLight.Intensity, 0f, 1f);
            //if (Scene.DirectionalLight != null)
            //    Scene.DirectionalLight.Intensity = Math.Clamp(Scene.DirectionalLight.Intensity, 0f, 1f);

            //if (Context.Input.IsJustMouseMoved)
            //{
            //    Vector2 mouseMovement = Context.Input.MouseMovement;
            //    Camera.RotatePitch(mouseMovement.Y * -0.1f);
            //    //Camera.RotateYaw(mouseMovement.X * 0.1f);
            //    Debug.Print(mouseMovement.ToString());
            //}
        }
    }
}
