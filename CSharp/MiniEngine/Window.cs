using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.GLFW;

namespace MiniEngine
{
    /// <summary>
    /// Window where to render
    /// </summary>
    public class Window : IDisposable
    {
        /// <summary>
        /// Context
        /// </summary>
        private Context _context;

        /// <summary>
        /// Window GLFW
        /// </summary>
        private GLFW.Window _glfwWindow;

        /// <summary>
        /// Titre de la fenêtre
        /// </summary>
        private string _title;

        /// <summary>
        /// Internal variable to callbacks to be sure the garbage collector will not clear it
        /// </summary>
        private KeyCallback _onKeyCallback;
        private MouseCallback _onMouseCallback;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NativeWindow" /> class.
        /// </summary>
        /// <param name="width">The desired width, in screen coordinates, of the window. This must be greater than zero.</param>
        /// <param name="height">The desired height, in screen coordinates, of the window. This must be greater than zero.</param>
        /// <param name="title">The initial window title.</param>
        public Window(int width, int height, string title, Context context)
        {
            _title = title;
            _context = context;

            _glfwWindow = Glfw.CreateWindow(width, height, title ?? string.Empty, Monitor.None, GLFW.Window.None);

            if (Glfw.GetClientApi(_glfwWindow) != ClientApi.None)
                MakeCurrent();

            //Binds the events...
            BindCallbacks();            

        }


        #region Properties


        /// <summary>
        ///     Gets or sets the width of the client area of the window, in screen coordinates.
        /// </summary>
        /// <exception cref="Exception">Thrown when specified value is less than 1.</exception>
        public int ClientWidth
        {
            get
            {
                Glfw.GetWindowSize(_glfwWindow, out var width, out var dummy);
                return width;
            }
            set
            {
                if (value < 1)
                    throw new Exception("Window width muts be greater than 0.");
                Glfw.GetWindowSize(_glfwWindow, out var dummy, out var height);
                Glfw.SetWindowSize(_glfwWindow, value, height);
            }
        }

        /// <summary>
        ///     Gets or sets the height of the client area of the window, in screen coordinates.
        /// </summary>
        /// <exception cref="Exception">Thrown when specified value is less than 1.</exception>
        public int ClientHeight
        {
            get
            {
                Glfw.GetWindowSize(_glfwWindow, out var dummy, out var height);
                return height;
            }
            set
            {
                if (value < 1)
                    throw new Exception("Window height muts be greater than 0.");
                Glfw.GetWindowSize(_glfwWindow, out var width, out var dummy);
                Glfw.SetWindowSize(_glfwWindow, width, value);
            }
        }

        /// <summary>
        ///     Gets or sets the size of the client area of the window, in screen coordinates.
        /// </summary>
        /// <value>
        ///     A <see cref="System.Drawing.Size" /> in screen coordinates that represents the size of the window's client area.
        /// </value>
        public Vector2 ClientSize
        {
            get
            {
                Glfw.GetWindowSize(_glfwWindow, out var width, out var height);
                return new Vector2(width, height);
            }
            set => Glfw.SetWindowSize(_glfwWindow, (int)value.X, (int)value.Y);
        }


        /// <summary>
        ///     Gets the monitor this window is fullscreen on.
        ///     <para>Returns <see cref="GLFW.Monitor.None" /> if window is not fullscreen.</para>
        /// </summary>
        /// <value>
        ///     The monitor.
        /// </value>
        public Monitor Monitor => Glfw.GetWindowMonitor(_glfwWindow);

        /// <summary>
        ///     Gets or sets the position of the window in screen coordinates, including border, titlebar, etc..
        /// </summary>
        /// <value>
        ///     The position.
        /// </value>
        public Vector2 Position
        {
            get
            {
                Glfw.GetWindowPosition(_glfwWindow, out var x, out var y);
                Glfw.GetWindowFrameSize(_glfwWindow, out var l, out var t, out var dummy1, out var dummy2);
                return new Vector2(x - l, y - t);
            }
            set
            {
                Glfw.GetWindowFrameSize(_glfwWindow, out var l, out var t, out var dummy1, out var dummy2);
                Glfw.SetWindowPosition(_glfwWindow, (int)value.X + l, (int)value.Y + t);
            }
        }

        /// <summary>
        ///     Gets or sets the size of the window, in screen coordinates, including border, titlebar, etc.
        /// </summary>
        /// <value>
        ///     A <see cref="System.Drawing.Size" /> in screen coordinates that represents the size of the window.
        /// </value>
        public Vector2 Size
        {
            get
            {
                Glfw.GetWindowSize(_glfwWindow, out var width, out var height);
                Glfw.GetWindowFrameSize(_glfwWindow, out var l, out var t, out var r, out var b);
                return new Vector2(width + l + r, height + t + b);
            }
            set
            {
                Glfw.GetWindowFrameSize(_glfwWindow, out var l, out var t, out var r, out var b);
                Glfw.SetWindowSize(_glfwWindow, (int)value.X - l - r, (int)value.Y - t - b);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is closing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is closing; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosing => Glfw.WindowShouldClose(_glfwWindow);

        /// <summary>
        ///     Gets or sets a string to the system clipboard.
        /// </summary>
        /// <value>
        ///     The clipboard string.
        /// </value>
        public string Clipboard
        {
            get => Glfw.GetClipboardString(_glfwWindow);
            set => Glfw.SetClipboardString(_glfwWindow, value);
        }

        /// <summary>
        ///     Gets or sets the behavior of the mouse cursor.
        /// </summary>
        /// <value>
        ///     The cursor mode.
        /// </value>
        public CursorMode CursorMode
        {
            get => (CursorMode)Glfw.GetInputMode(_glfwWindow, InputMode.Cursor);
            set => Glfw.SetInputMode(_glfwWindow, InputMode.Cursor, (int)value);
        }


        /// <summary>
        ///     Gets a value indicating whether this instance is decorated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is decorated; otherwise, <c>false</c>.
        /// </value>
        public bool IsDecorated => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Decorated);

        /// <summary>
        ///     Gets a value indicating whether this instance is floating (top-most, always-on-top).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is floating; otherwise, <c>false</c>.
        /// </value>
        public bool IsFloating => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Floating);

        /// <summary>
        ///     Gets a value indicating whether this instance is focused.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is focused; otherwise, <c>false</c>.
        /// </value>
        public bool IsFocused => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Focused);

        /// <summary>
        ///     Gets a value indicating whether this instance is resizable.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is resizable; otherwise, <c>false</c>.
        /// </value>
        public bool IsResizable => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Resizable);

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="NativeWindow" /> is maximized.
        ///     <para>Has no effect on fullscreen windows.</para>
        /// </summary>
        /// <value>
        ///     <c>true</c> if maximized; otherwise, <c>false</c>.
        /// </value>
        public bool Maximized
        {
            get => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Maximized);
            set
            {
                if (value)
                    Glfw.MaximizeWindow(_glfwWindow);
                else
                    Glfw.RestoreWindow(_glfwWindow);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="NativeWindow" /> is minimized.
        ///     <para>If window is already minimized, does nothing.</para>
        /// </summary>
        /// <value>
        ///     <c>true</c> if minimized; otherwise, <c>false</c>.
        /// </value>
        public bool Minimized
        {
            get => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.AutoIconify);
            set
            {
                if (value)
                    Glfw.IconifyWindow(_glfwWindow);
                else
                    Glfw.RestoreWindow(_glfwWindow);
            }
        }


        /// <summary>
        ///     Gets or sets the mouse position in screen-coordinates relative to the client area of the window.
        /// </summary>
        /// <value>
        ///     The mouse position.
        /// </value>
        public Vector2 MousePosition
        {
            get
            {
                Glfw.GetCursorPosition(_glfwWindow, out var x, out var y);
                return new Vector2(Convert.ToSingle(x), Convert.ToSingle(y));
            }
            set => Glfw.SetCursorPosition(_glfwWindow, value.X, value.Y);
        }

        /// <summary>
        ///     Sets the sticky keys input mode.
        ///     <para>
        ///         Set to <c>true</c> to enable sticky keys, or <c>false</c> to disable it. If sticky keys are enabled, a key
        ///         press will ensure that <see cref="Glfw.GetKey" /> returns <see cref="InputState.Press" /> the next time it is
        ///         called even if the key had been released before the call. This is useful when you are only interested in
        ///         whether keys have been pressed but not when or in which order.
        ///     </para>
        /// </summary>
        public bool StickyKeys
        {
            get => Glfw.GetInputMode(_glfwWindow, InputMode.StickyKeys) == (int)Constants.True;
            set =>
                Glfw.SetInputMode(_glfwWindow, InputMode.StickyKeys, value ? (int)Constants.True : (int)Constants.False);
        }

        /// <summary>
        ///     Gets or sets the sticky mouse button input mode.
        ///     <para>
        ///         Set to <c>true</c> to enable sticky mouse buttons, or <c>false</c> to disable it. If sticky mouse buttons are
        ///         enabled, a mouse button press will ensure that <see cref="Glfw.GetMouseButton" /> returns
        ///         <see cref="InputState.Press" /> the next time it is called even if the mouse button had been released before
        ///         the call. This is useful when you are only interested in whether mouse buttons have been pressed but not when
        ///         or in which order.
        ///     </para>
        /// </summary>
        public bool StickyMouseButtons
        {
            get => Glfw.GetInputMode(_glfwWindow, InputMode.StickyMouseButton) == (int)Constants.True;
            set =>
                Glfw.SetInputMode(_glfwWindow, InputMode.StickyMouseButton,
                    value ? (int)Constants.True : (int)Constants.False);
        }

        /// <summary>
        ///     Gets or sets the window title or caption.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Glfw.SetWindowTitle(_glfwWindow, value ?? string.Empty);
            }
        }

        /// <summary>
        ///     Gets the video mode for the monitor this window is fullscreen on.
        ///     <para>If window is not fullscreen, returns the <see cref="GLFW.VideoMode" /> for the primary monitor.</para>
        /// </summary>
        /// <value>
        ///     The video mode.
        /// </value>
        public VideoMode VideoMode
        {
            get
            {
                var monitor = Monitor;
                return Glfw.GetVideoMode(monitor == Monitor.None ? Glfw.PrimaryMonitor : monitor);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="NativeWindow" /> is visible.
        /// </summary>
        /// <value>
        ///     <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible
        {
            get => Glfw.GetWindowAttribute(_glfwWindow, WindowAttribute.Visible);
            set
            {
                if (value)
                    Glfw.ShowWindow(_glfwWindow);
                else
                    Glfw.HideWindow(_glfwWindow);
            }
        }

        #endregion


        #region Public methods




        /// <summary>
        ///     Sets the window fullscreen on the primary monitor.
        /// </summary>
        public void Fullscreen()
        {
            Fullscreen(Glfw.PrimaryMonitor);
        }

        /// <summary>
        ///     Sets the window fullscreen on the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to display the window fullscreen.</param>
        public void Fullscreen(Monitor monitor)
        {
            Glfw.SetWindowMonitor(_glfwWindow, monitor, 0, 0, 0, 0, -1);
        }


        /// <summary>
        ///     Centers the on window on the screen.
        ///     <para>Has no effect on fullscreen or maximized windows.</para>
        /// </summary>
        public void CenterOnScreen()
        {
            var monitor = Monitor == Monitor.None ? Glfw.PrimaryMonitor : Monitor;
            var videoMode = Glfw.GetVideoMode(monitor);
            var size = Size;
            Position = new Vector2((videoMode.Width - size.X) / 2, (videoMode.Height - size.Y) / 2);
        }

        /// <summary>
        ///     Closes this instance.
        ///     <para>This invalidates the window, but does not free its resources.</para>
        /// </summary>
        public void Close()
        {
            Glfw.SetWindowShouldClose(_glfwWindow, true);
        }

        /// <summary>
        ///     Swaps the front and back buffers when rendering with OpenGL or OpenGL ES.
        ///     <para>
        ///         This should not be called on a window that is not using an OpenGL or OpenGL ES context (.i.e. Vulkan).
        ///     </para>
        /// </summary>
        public void SwapBuffers()
        {
            Glfw.SwapBuffers(_glfwWindow);
        }

        /// <summary>
        ///     Focuses this form to receive input and events.
        /// </summary>
        public void Focus()
        {
            Glfw.FocusWindow(_glfwWindow);
        }

        /// <summary>
        ///     Makes window and its context the current.
        /// </summary>
        public void MakeCurrent()
        {
            Glfw.MakeContextCurrent(_glfwWindow);
        }

        /// <summary>
        ///     Maximizes this window to fill the screen.
        ///     <para>Has no effect if window is already maximized.</para>
        /// </summary>
        public void Maximize()
        {
            Glfw.MaximizeWindow(_glfwWindow);
        }


        /// <summary>
        ///     Minimizes this window.
        ///     <para>Has no effect if window is already minimized.</para>
        /// </summary>
        public void Minimize()
        {
            Glfw.IconifyWindow(_glfwWindow);
        }

        /// <summary>
        ///     Restores a minimized window to its previous state.
        ///     <para>Has no effect if window was already restored.</para>
        /// </summary>
        public void Restore()
        {
            Glfw.RestoreWindow(_glfwWindow);
        }

        /// <summary>
        ///     Sets the aspect ratio to maintain for the window.
        ///     <para>This function is ignored for fullscreen windows.</para>
        /// </summary>
        /// <param name="numerator">The numerator of the desired aspect ratio.</param>
        /// <param name="denominator">The denominator of the desired aspect ratio.</param>
        public void SetAspectRatio(int numerator, int denominator)
        {
            Glfw.SetWindowAspectRatio(_glfwWindow, numerator, denominator);
        }

        /// <summary>
        ///     Sets the icon(s) used for the titlebar, taskbar, etc.
        ///     <para>Standard sizes are 16x16, 32x32, and 48x48.</para>
        /// </summary>
        /// <param name="images">One or more images to set as an icon.</param>
        public void SetIcons(params Image[] images)
        {
            Glfw.SetWindowIcon(_glfwWindow, images.Length, images);
        }

        /// <summary>
        ///     Sets the window monitor.
        ///     <para>
        ///         If <paramref name="monitor" /> is not <see cref="GLFW.Monitor.None" />, the window will be full-screened and
        ///         dimensions ignored.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The desired monitor, or <see cref="GLFW.Monitor.None" /> to set windowed mode.</param>
        /// <param name="x">The desired x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="y">The desired y-coordinate of the upper-left corner of the client area.</param>
        /// <param name="width">The desired width, in screen coordinates, of the client area or video mode.</param>
        /// <param name="height">The desired height, in screen coordinates, of the client area or video mode.</param>
        /// <param name="refreshRate">The desired refresh rate, in Hz, of the video mode, or <see cref="Constants.Default" />.</param>
        public void SetMonitor(Monitor monitor, int x, int y, int width, int height,
            int refreshRate = (int)Constants.Default)
        {
            Glfw.SetWindowMonitor(_glfwWindow, monitor, x, y, width, height, refreshRate);
        }

        /// <summary>
        ///     Sets the limits of the client size  area of the window.
        /// </summary>
        /// <param name="minSize">The minimum size of the client area.</param>
        /// <param name="maxSize">The maximum size of the client area.</param>
        public void SetSizeLimits(Vector2 minSize, Vector2 maxSize)
        {
            SetSizeLimits((int)minSize.X, (int)minSize.Y, (int)maxSize.X, (int)maxSize.Y);
        }

        /// <summary>
        ///     Sets the limits of the client size  area of the window.
        /// </summary>
        /// <param name="minWidth">The minimum width of the client area.</param>
        /// <param name="minHeight">The minimum height of the client area.</param>
        /// <param name="maxWidth">The maximum width of the client area.</param>
        /// <param name="maxHeight">The maximum height of the client area.</param>
        public void SetSizeLimits(int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            Glfw.SetWindowSizeLimits(_glfwWindow, minWidth, minHeight, maxWidth, maxHeight);
        }


        /// <summary>
        /// Create a surface
        /// </summary>
        public void CreateSurface(IntPtr instance, IntPtr surface)
        {
            int result = Glfw.CreateWindowSurface(instance, _glfwWindow, IntPtr.Zero, surface);
            if (result != 0)
                throw new Exception($"Impossible to create surface: {result}");
        }


        /// <summary>
        /// Dispose the Window
        /// </summary>
        public void Dispose()
        {
            try
            {
                Glfw.DestroyWindow(_glfwWindow);
            }
            catch
            { }
        }


        #endregion

        #region Private methods


        /// <summary>
        /// Bind callbacks
        /// </summary>
        private void BindCallbacks()
        {
            _onKeyCallback = OnKey;
            _onMouseCallback = OnCursorPosition;

            Glfw.SetKeyCallback(_glfwWindow, _onKeyCallback);
            Glfw.SetCursorPositionCallback(_glfwWindow, _onMouseCallback);
        }

        /// <summary>
        /// OnKeypress
        /// </summary>
        public void OnKey(GLFW.Window window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            _context.Input.SetKeyState(key, (state != InputState.Release));
        }

        /// <summary>
        /// OnCursorPosition (mouse mouved)
        /// </summary>
        public void OnCursorPosition(GLFW.Window window, double x, double y)
        {
            _context.Input.SetMousePosition(new Vector2((float)x, (float)y));
        }

        #endregion

    }
}
