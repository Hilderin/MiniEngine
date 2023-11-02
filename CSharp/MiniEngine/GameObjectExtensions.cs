using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Extensions for GameObjects
    /// </summary>
    public static class GameObjectExtensions
    {

        /// <summary>
        /// Add a component to the game object
        /// </summary>
        public static T AddComponents<T>(this T gameObject, params GameComponent[] components) where T : GameObject
        {
            for (int i = 0; i < components.Length; i++)
            {
                gameObject.AddComponent(components[i]);
            }

            return gameObject;

        }


        /// <summary>
        /// Rotate on X axis
        /// </summary>
        public static T RotatePitch<T>(this T gameObject, float angleRad) where T : GameObject
        {
            gameObject.Transform.RotatePitch(angleRad);
            return gameObject;
        }

        /// <summary>
        /// Rotate on Y axis
        /// </summary>
        public static T RotateYaw<T>(this T gameObject, float angleRad) where T : GameObject
        {
            gameObject.Transform.RotateYaw(angleRad);
            return gameObject;
        }

        /// <summary>
        /// Rotate on Z axis
        /// </summary>
        public static T RotateRoll<T>(this T gameObject, float angleRad) where T : GameObject
        {
            gameObject.Transform.RotateRoll(angleRad);
            return gameObject;
        }

        /// <summary>
        /// Move to a specific location
        /// </summary>
        public static T MoveTo<T>(this T gameObject, Vector3 location) where T : GameObject
        {
            gameObject.Transform.MoveTo(location);
            return gameObject;
        }

        /// <summary>
        /// Move forward considering the rotation of the world transform
        /// </summary>
        public static T MoveForward<T>(this T gameObject, float distance) where T : GameObject
        {
            gameObject.Transform.MoveForward(distance);
            return gameObject;
        }

        /// <summary>
        /// Move forward considering the rotation of the world transform
        /// Z < 0 = Forward
        /// Z < 0 = Backward
        /// X > 0 = Right
        /// X < 0 = Left
        /// Y > 0 = Up
        /// Y < 0 = Down
        /// </summary>
        public static T MoveInDirections<T>(this T gameObject, float distance, Vector3 directions) where T : GameObject
        {
            gameObject.Transform.MoveInDirections(distance, directions);
            return gameObject;
        }

        /// <summary>
        /// Move backward considering the rotation of the world transform
        /// </summary>
        public static T MoveBackward<T>(this T gameObject, float distance) where T : GameObject
        {
            gameObject.Transform.MoveBackward(distance);
            return gameObject;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public static T MoveLeft<T>(this T gameObject, float distance) where T : GameObject
        {
            gameObject.Transform.MoveLeft(distance);
            return gameObject;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public static T MoveRight<T>(this T gameObject, float distance) where T : GameObject
        {
            gameObject.Transform.MoveRight(distance);
            return gameObject;
        }

        /// <summary>
        /// Add scale
        /// </summary>
        public static T AddScale<T>(this T gameObject, float scale) where T : GameObject
        {
            gameObject.Transform.AddScale(scale);
            return gameObject;
        }

    }
}
