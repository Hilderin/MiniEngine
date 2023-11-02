using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Track inputs between each frames
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Keys that are down at a fixed position
        /// </summary>
        private bool[] _keyDownsFixedIndex = new bool[349];

        /// <summary>
        /// Keys that are down in the list
        /// </summary>
        private List<Keys> _keyDowns = new List<Keys>();

        /// <summary>
        /// List of newly down keys
        /// </summary>
        private List<Keys> _newlyKeyDowns = new List<Keys>();

        /// <summary>
        /// List of newly up keys
        /// </summary>
        private List<Keys> _newlyKeyUps = new List<Keys>();


        /// <summary>
        /// Mouse buttons that are down at a fixed position
        /// </summary>
        private bool[] _mouseDownsFixedIndex = new bool[8];

        /// <summary>
        /// MouseButton that are down in the list
        /// </summary>
        private List<MouseButton> _mouseDowns = new List<MouseButton>();

        /// <summary>
        /// List of newly down mouseButtons
        /// </summary>
        private List<MouseButton> _newlyMouseDowns = new List<MouseButton>();

        /// <summary>
        /// List of newly up mouseButtons
        /// </summary>
        private List<MouseButton> _newlyMouseUps = new List<MouseButton>();

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
        /// Get the mouse scroll position
        /// </summary>
        public Vector2 MouseScroll { get; private set; }

        /// <summary>
        /// Get the mouse scroll position delta
        /// </summary>
        public Vector2 MouseScrollDelta { get; private set; }



        /// <summary>
        /// List of keys that are down
        /// </summary>
        public List<Keys> KeyDowns { get { return _keyDowns; } }

        /// <summary>
        /// List of keys that are newly down
        /// </summary>
        public List<Keys> NewlyKeyDowns { get { return _newlyKeyDowns; } }

        /// <summary>
        /// List of keys that are newly up
        /// </summary>
        public List<Keys> NewlyKeyUps { get { return _newlyKeyUps; } }




        /// <summary>
        /// Constructor
        /// </summary>
        public InputManager()
        {
        }



        /// <summary>
        /// Set a state for a key
        /// </summary>
        public void SetKeyState(Keys key, bool down)
        {
            Debug.Print("SetKeyState - " + key + " " + (down ? "down" : "up"));
            if ((int)key > 0 && (int)key < 349)
            {
                if (down)
                {
                    //Key down...
                    if (!_keyDownsFixedIndex[(int)key])
                    {
                        //Newly down...
                        _newlyKeyDowns.Add(key);
                        _keyDowns.Add(key);
                        _keyDownsFixedIndex[(int)key] = true;
                    }
                }
                else
                {
                    //Key up...
                    if (_keyDownsFixedIndex[(int)key])
                    {
                        _keyDowns.Remove(key);
                        _newlyKeyUps.Add(key);
                        _keyDownsFixedIndex[(int)key] = false;
                    }
                }
            }

            
        }

        /// <summary>
        /// Set the mouse position
        /// </summary>
        public void SetMousePosition(Vector2 position)
        {
            if (MousePosition != position)
            {
                //Calculate the diff and the deplacement vector...
                //Debug.Print("Old position: " + MousePosition + ", new: " + position.ToString());
                Vector2 movement = (position - MousePosition) / 200;
                //movement.Normalize();
                MouseMovement = movement;
                //Debug.Print(movement.ToString());

                MousePosition = position;
                IsJustMouseMoved = true;
            }
        }

        /// <summary>
        /// Set the mouse scroll
        /// </summary>
        public void SetMouseScroll(Vector2 scroll)
        {
            if (MouseScroll != scroll)
            {
                //Calculate the diff and the deplacement vector...
                Debug.Print("Old scroll: " + MouseScroll + ", new: " + scroll.ToString());
                Vector2 movement = (scroll - MouseScroll) / 200;
                //movement.Normalize();
                MouseScrollDelta = movement;
                //Debug.Print(movement.ToString());

                MouseScroll = scroll;
                IsJustMouseMoved = true;
            }
        }

        /// <summary>
        /// Set a state for a mouse button
        /// </summary>
        public void SetMouseButton(MouseButton mouseButton, bool down)
        {
            if (down)
            {
                //mouseButton down...
                if (!_mouseDownsFixedIndex[(int)mouseButton])
                {
                    //Newly down...
                    _newlyMouseDowns.Add(mouseButton);
                    _mouseDowns.Add(mouseButton);
                    _mouseDownsFixedIndex[(int)mouseButton] = true;
                }
            }
            else
            {
                //mouseButton up...
                if (_mouseDownsFixedIndex[(int)mouseButton])
                {
                    _mouseDowns.Remove(mouseButton);
                    _newlyMouseUps.Add(mouseButton);
                    _mouseDownsFixedIndex[(int)mouseButton] = false;
                }
            }

            
        }



        /// <summary>
        /// Check if a key is down
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return _keyDownsFixedIndex[(int)key];
        }

        /// <summary>
        /// Check if the key down is new since the last frame
        /// </summary>
        public bool IsJustKeyDown(Keys key)
        {
            if (IsKeyDown(key))
                return _newlyKeyDowns.Contains(key);
            else
                return false;
        }

        /// <summary>
        /// Check if a mouse button is down
        /// </summary>
        public bool IsMouseDown(MouseButton mouseDown)
        {
            return _mouseDownsFixedIndex[(int)mouseDown];
        }

        /// <summary>
        /// Check if the mouseDown down is new since the last frame
        /// </summary>
        public bool IsJustMouseDown(MouseButton mouseDown)
        {
            if (IsMouseDown(mouseDown))
                return _newlyMouseDowns.Contains(mouseDown);
            else
                return false;
        }

        /// <summary>
        /// Get movement vector
        /// </summary>
        public Vector3 GetMovementVector(Keys forwardKey, Keys backwardKey, Keys leftKey, Keys rightKey, Keys upKey = Keys.None, Keys downKey = Keys.None)
        {
            Vector3 movement = Vector3.Zero;

            if (IsKeyDown(forwardKey))
                movement += Vector3.Forward;
            if (IsKeyDown(backwardKey))
                movement += Vector3.Backward;
            if (IsKeyDown(leftKey))
                movement += Vector3.Left;
            if (IsKeyDown(rightKey))
                movement += Vector3.Right;
            if (IsKeyDown(upKey))
                movement += Vector3.Up;
            if (IsKeyDown(downKey))
                movement += Vector3.Down;

            return movement;
        }

        /// <summary>
        /// Indicate a new frame
        /// </summary>
        internal void OnNewFrame()
        {
            _newlyKeyDowns.Clear();
            _newlyKeyUps.Clear();
            _newlyMouseDowns.Clear();
            _newlyMouseUps.Clear();
            IsJustMouseMoved = false;
            MouseMovement = Vector2.Zero;
        }

    }
}
