using System;
using MiniEngine.GLFW;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Callback for the Init method
    /// </summary>
    public delegate void InitHandler();

    /// <summary>
    /// Callback for the Run method
    /// </summary>
    public delegate void RunHandler();

    /// <summary>
    /// Context for a game instance
    /// </summary>
    public class Context : IDisposable
    {
        #region Private members

        /// <summary>
        /// Indicate if initialized
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Indicate if disposed
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// Current context
        /// </summary>
        [ThreadStatic]
        private static Context _current;

        /// <summary>
        /// Window
        /// </summary>
        private Window _window;

        #endregion

        #region Public properties

        /// <summary>
        /// ClientSize
        /// </summary>
        public Vector2 ClientSize = new Vector2(1200, 800);


        /// <summary>
        /// Current context
        /// </summary>
        public static Context Current => _current;

        /// <summary>
        /// Renderer
        /// </summary>
        public readonly Renderer Renderer;

        /// <summary>
        /// Input manager
        /// </summary>
        public readonly Input Input;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Context()
        {
            _current = this;

            InitGlfw();

            Renderer = new Renderer(this);
            Input = new Input(this);
        }

        /// <summary>
        /// Open the window
        /// </summary>
        public Context OpenWindow(int width, int height, string title)
        {
            this._window = new Window(width, height, title, this);

            this.ClientSize.X = width;
            this.ClientSize.Y = height;

            return this;
        }

        /// <summary>
        /// Center the window on screen
        /// </summary>
        public Context CenterOnScreen()
        {
            EnsureWindowExists();

            _window.CenterOnScreen();

            return this;
        }

        

        /// <summary>
        /// Lock the cursor in the window
        /// </summary>
        public Context LockCursor()
        {
            EnsureWindowExists();

            _window.CursorMode = CursorMode.Disabled;

            return this;
        }

        /// <summary>
        /// Unlock the cursor in the window
        /// </summary>
        public Context UnlockCursor()
        {
            EnsureWindowExists();

            _window.CursorMode = CursorMode.Normal;

            return this;
        }







        /// <summary>
        /// Init the game/application
        /// </summary>
        public Context Init(InitHandler initHandler = null)
        {
            InitInternal();

            if (initHandler != null)
                initHandler();

            return this;
        }

        /// <summary>
        /// Run the game/application
        /// </summary>
        public void Run(RunHandler runHandler = null)
        {
            if (this._window == null)
                OpenWindow(1200, 800, "L renderer");

            //Now we can initialize the renderer...
            InitInternal();

            //And we are looping...
            while (!this._window.IsClosing)
            {

                GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

                //Exécute rendering...
                if (runHandler != null)
                    runHandler();

                //Swapping buffer...
                this._window.SwapBuffers();

                //Indicate a new frame...
                Input.OnNewFrame();

                //Get new mouse and keyboard pulls
                try
                {
                    Glfw.PollEvents();
                }
                catch { }
            }

            //We dispose already, not else to to with this context anyway...
            this.Dispose();
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void Quit()
        {
            if (this._window != null)
                this._window.Close();
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }

            _isDisposed = true;
        }


        /// <summary>
        /// Internal initialization
        /// </summary>
        private void InitInternal()
        {
            if (_isInitialized)
                return;

            //Now we can initialize the renderer...
            Renderer.Init();

            _isInitialized = true;


        }


        /// <summary>
        /// Init the Glfw
        /// </summary>
        private void InitGlfw()
        {
            // Set some common hints for the OpenGL profile creation
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);
            Glfw.WindowHint(Hint.OpenglForwardCompatible, true);
            Glfw.WindowHint(Hint.DepthBits, true);                  //Depth test

        }

        /// <summary>
        /// Be sure that the window is created
        /// </summary>
        private void EnsureWindowExists()
        {
            if (_window == null)
                OpenWindow((int)ClientSize.X, (int)ClientSize.Y, "MiniEngine");
        }


    }
}
