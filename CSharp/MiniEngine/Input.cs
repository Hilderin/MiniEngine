using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public class Input
    {
        /// <summary>
        /// Context
        /// </summary>
        private Context _context;

        /// <summary>
        /// Keys that are down
        /// </summary>
        private bool[] _keyDown = new bool[349];

        /// <summary>
        /// List of newly pressed keys
        /// </summary>
        private List<Keys> _newlyPressedKeys = new List<Keys>();

        /// <summary>
        /// Indicate if the mouse just moved
        /// </summary>
        public bool IsJustMouseMoved { get; private set; }

        /// <summary>
        /// Get the mouse position
        /// </summary>
        public Vector2 MousePosition { get; private set; }

        /// <summary>
        /// Get the mouse movement vector normalized
        /// </summary>
        public Vector2 MouseMovement { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Input(Context context)
        {
            _context = context;
        }


        /// <summary>
        /// Set a state for a key
        /// </summary>
        public void SetKeyState(Keys key, bool pressed)
        {
            if(pressed && !_keyDown[(int)key])
                _newlyPressedKeys.Add(key);

            _keyDown[(int)key] = pressed;
        }

        /// <summary>
        /// Set the mouse position
        /// </summary>
        public void SetMousePosition(Vector2 position)
        {
            if (MousePosition != position)
            {
                //Calculate the diff and the deplacement vector...
                //Debug.Print(position.ToString());
                Vector2 movement = (position - MousePosition) / 200;
                //movement.Normalize();
                MouseMovement = movement;
                //Debug.Print(movement.ToString());

                MousePosition = position;
                IsJustMouseMoved = true;
            }
        }


        /// <summary>
        /// Check if a key is pressed
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return _keyDown[(int)key];
        }

        /// <summary>
        /// Check if the key pressed is new since the last frame
        /// </summary>
        public bool IsJustKeyPressed(Keys key)
        {
            if (IsKeyPressed(key))
                return _newlyPressedKeys.Contains(key);
            else
                return false;
        }

        /// <summary>
        /// Get movement vector
        /// </summary>
        public Vector3 GetMovementVector(Keys forwardKey, Keys backwardKey, Keys leftKey, Keys rightKey, Keys upKey = Keys.None, Keys downKey = Keys.None)
        {
            Vector3 movement = Vector3.Zero;

            if (IsKeyPressed(forwardKey))
                movement += Vector3.Forward;
            if (IsKeyPressed(backwardKey))
                movement += Vector3.Backward;
            if (IsKeyPressed(leftKey))
                movement += Vector3.Left;
            if (IsKeyPressed(rightKey))
                movement += Vector3.Right;
            if (IsKeyPressed(upKey))
                movement += Vector3.Up;
            if (IsKeyPressed(downKey))
                movement += Vector3.Down;

            return movement;
        }

        /// <summary>
        /// Indicate a new frame
        /// </summary>
        internal void OnNewFrame()
        {
            _newlyPressedKeys.Clear();
            IsJustMouseMoved = false;
            MouseMovement = Vector2.Zero;
        }

    }
}
