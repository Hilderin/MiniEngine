using ImGuiNET;
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
        private Context _context;

        /// <summary>
        /// Last cursormode
        /// </summary>
        private CursorMode _lastCursorMode = CursorMode.Normal;

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
        public InputManager(Context context)
        {
            _context = context;
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


        /// <summary>
        /// Set a state for a key
        /// </summary>
        public void SetKeyState(Keys key, bool down)
        {
            //Debug.Info("SetKeyState - " + key + " " + (down ? "down" : "up"));
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
                CursorMode currentCursorMode = _context.Window.CursorMode;

                Vector2 movement;
                if (_lastCursorMode != currentCursorMode)
                {
                    //No movement...
                    _lastCursorMode = currentCursorMode;
                    movement = Vector2.Zero;
                }
                else
                {
                    //Calculate the diff and the deplacement vector...
                    //Debug.Print("Old position: " + MousePosition + ", new: " + position.ToString());
                    movement = (position - MousePosition);
                    movement.X = movement.X * 100f / _context.Window.ClientSize.X;
                    movement.Y = movement.Y * 100f / _context.Window.ClientSize.Y;

                    MouseMovement = movement;
                }

               
                //Debug.Info("position: " + position + ", Movement: " + (position - MousePosition).ToString() + ", Relative mov: " + movement.ToString());

                MousePosition = position;

                if(movement != Vector2.Zero)
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
                //Debug.Info("Old scroll: " + MouseScroll + ", new: " + scroll.ToString());
                Vector2 movement = (scroll - MouseScroll) / 200;
                //movement.Normalize();
                MouseScrollDelta = movement;
                //Debug.Print(movement.ToString());

                MouseScroll = scroll;
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
        /// Check if the key just released since the last frame
        /// </summary>
        public bool IsJustKeyUp(Keys key)
        {
            if (!IsKeyDown(key))
                return _newlyKeyUps.Contains(key);
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
        /// Check if the mouseDown just released since the last frame
        /// </summary>
        public bool IsJustMouseUp(MouseButton mouseDown)
        {
            if (!IsMouseDown(mouseDown))
                return _newlyMouseUps.Contains(mouseDown);
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
        /// Update the input for the mouse and the keyboard to ImGui
        /// </summary>
        public void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            //io.ClearInputKeys();

            io.AddMousePosEvent(MousePosition.X, MousePosition.Y);
            io.AddMouseButtonEvent(0, IsMouseDown(MouseButton.Left));
            io.AddMouseButtonEvent(1, IsMouseDown(MouseButton.Right));
            io.AddMouseButtonEvent(2, IsMouseDown(MouseButton.Middle));
            io.AddMouseButtonEvent(3, IsMouseDown(MouseButton.Button1));
            io.AddMouseButtonEvent(4, IsMouseDown(MouseButton.Button2));
            io.AddMouseWheelEvent(0f, MouseScrollDelta.Y);

            for (int i = 0; i < NewlyKeyDowns.Count; i++)
                ImGuiProcessKeyState(NewlyKeyDowns[i], true, io);

            for (int i = 0; i < NewlyKeyUps.Count; i++)
                ImGuiProcessKeyState(NewlyKeyUps[i], false, io);


        }


        /// <summary>
        /// Process a key state...
        /// </summary>
        private void ImGuiProcessKeyState(Keys key, bool down, ImGuiIOPtr io)
        {
            if (ImGuiTryMapKey(key, out bool isTextInput, out ImGuiKey imguikey))
            {
                io.AddKeyEvent(imguikey, down);
            }

            if (down && isTextInput)
                io.AddInputCharacter((uint)key);
        }


        /// <summary>
        /// Try mapping a key with ImGui keys
        /// </summary>
        private bool ImGuiTryMapKey(Keys key, out bool isTextInput, out ImGuiKey result)
        {
            static ImGuiKey keyToImGuiKeyShortcut(Keys keyToConvert, Keys startKey1, ImGuiKey startKey2)
            {
                int changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            if (key >= Keys.F1 && key <= Keys.F12)
            {
                result = keyToImGuiKeyShortcut(key, Keys.F1, ImGuiKey.F1);
                isTextInput = false;
                return true;
            }
            else if (key >= Keys.Numpad0 && key <= Keys.Numpad9)
            {
                result = keyToImGuiKeyShortcut(key, Keys.Numpad0, ImGuiKey.Keypad0);
                isTextInput = true;
                return true;
            }
            else if (key >= Keys.A && key <= Keys.Z)
            {
                result = keyToImGuiKeyShortcut(key, Keys.A, ImGuiKey.A);
                isTextInput = true;
                return true;
            }
            else if (key >= Keys.Number0 && key <= Keys.Number9)
            {
                result = keyToImGuiKeyShortcut(key, Keys.Number0, ImGuiKey._0);
                isTextInput = true;
                return true;
            }

            switch (key)
            {
                case Keys.ShiftLeft:
                case Keys.ShiftRight:
                    result = ImGuiKey.ModShift;
                    isTextInput = false;
                    return true;
                case Keys.ControlLeft:
                case Keys.ControlRight:
                    result = ImGuiKey.ModCtrl;
                    isTextInput = false;
                    return true;
                case Keys.AltLeft:
                case Keys.AltRight:
                    result = ImGuiKey.ModAlt;
                    isTextInput = false;
                    return true;
                case Keys.LeftSuper:
                case Keys.RightSuper:
                    result = ImGuiKey.ModSuper;
                    isTextInput = false;
                    return true;
                case Keys.Menu:
                    result = ImGuiKey.Menu;
                    isTextInput = false;
                    return true;
                case Keys.Up:
                    result = ImGuiKey.UpArrow;
                    isTextInput = false;
                    return true;
                case Keys.Down:
                    result = ImGuiKey.DownArrow;
                    isTextInput = false;
                    return true;
                case Keys.Left:
                    result = ImGuiKey.LeftArrow;
                    isTextInput = false;
                    return true;
                case Keys.Right:
                    result = ImGuiKey.RightArrow;
                    isTextInput = false;
                    return true;
                case Keys.Enter:
                    result = ImGuiKey.Enter;
                    isTextInput = false;
                    return true;
                case Keys.Escape:
                    result = ImGuiKey.Escape;
                    isTextInput = false;
                    return true;
                case Keys.Space:
                    result = ImGuiKey.Space;
                    isTextInput = true;
                    return true;
                case Keys.Tab:
                    result = ImGuiKey.Tab;
                    isTextInput = false;
                    return true;
                case Keys.Backspace:
                    result = ImGuiKey.Backspace;
                    isTextInput = false;
                    return true;
                case Keys.Insert:
                    result = ImGuiKey.Insert;
                    isTextInput = false;
                    return true;
                case Keys.Delete:
                    result = ImGuiKey.Delete;
                    isTextInput = false;
                    return true;
                case Keys.PageUp:
                    result = ImGuiKey.PageUp;
                    isTextInput = false;
                    return true;
                case Keys.PageDown:
                    result = ImGuiKey.PageDown;
                    isTextInput = false;
                    return true;
                case Keys.Home:
                    result = ImGuiKey.Home;
                    isTextInput = false;
                    return true;
                case Keys.End:
                    result = ImGuiKey.End;
                    isTextInput = false;
                    return true;
                case Keys.CapsLock:
                    result = ImGuiKey.CapsLock;
                    isTextInput = false;
                    return true;
                case Keys.ScrollLock:
                    result = ImGuiKey.ScrollLock;
                    isTextInput = false;
                    return true;
                case Keys.PrintScreen:
                    result = ImGuiKey.PrintScreen;
                    isTextInput = false;
                    return true;
                case Keys.Pause:
                    result = ImGuiKey.Pause;
                    isTextInput = false;
                    return true;
                case Keys.NumLock:
                    result = ImGuiKey.NumLock;
                    isTextInput = false;
                    return true;
                case Keys.NumpadDivide:
                    result = ImGuiKey.KeypadDivide;
                    isTextInput = false;
                    return true;
                case Keys.NumpadMultiply:
                    result = ImGuiKey.KeypadMultiply;
                    isTextInput = true;
                    return true;
                case Keys.NumpadSubtract:
                    result = ImGuiKey.KeypadSubtract;
                    isTextInput = true;
                    return true;
                case Keys.NumpadAdd:
                    result = ImGuiKey.KeypadAdd;
                    isTextInput = true;
                    return true;
                case Keys.NumpadDecimal:
                    result = ImGuiKey.KeypadDecimal;
                    isTextInput = true;
                    return true;
                case Keys.NumpadEnter:
                    result = ImGuiKey.KeypadEnter;
                    isTextInput = false;
                    return true;
                case Keys.GraveAccent:
                    result = ImGuiKey.GraveAccent;
                    isTextInput = true;
                    return true;
                case Keys.Minus:
                    result = ImGuiKey.Minus;
                    isTextInput = true;
                    return true;
                case Keys.Equal:
                    result = ImGuiKey.Equal;
                    isTextInput = true;
                    return true;
                case Keys.BracketLeft:
                    result = ImGuiKey.LeftBracket;
                    isTextInput = true;
                    return true;
                case Keys.BracketRight:
                    result = ImGuiKey.RightBracket;
                    isTextInput = true;
                    return true;
                case Keys.Semicolon:
                    result = ImGuiKey.Semicolon;
                    isTextInput = true;
                    return true;
                case Keys.Apostrophe:
                    result = ImGuiKey.Apostrophe;
                    isTextInput = true;
                    return true;
                case Keys.Comma:
                    result = ImGuiKey.Comma;
                    isTextInput = true;
                    return true;
                case Keys.Period:
                    result = ImGuiKey.Period;
                    isTextInput = true;
                    return true;
                case Keys.Slash:
                    result = ImGuiKey.Slash;
                    isTextInput = true;
                    return true;
                case Keys.Backslash:
                    result = ImGuiKey.Backslash;
                    isTextInput = true;
                    return true;
                default:
                    result = ImGuiKey.None;
                    isTextInput = false;
                    return false;
            }
        }


    }
}
