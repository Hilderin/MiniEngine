using System;
using System.Collections.Generic;
using MiniEngine.ResourceDefinitions;
using SixLabors.ImageSharp.PixelFormats;

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

        ///// <summary>
        ///// ClientSize
        ///// </summary>
        //private Vector2 _clientSize = new Vector2(1200, 800);

        /// <summary>
        /// Current context
        /// </summary>
        [ThreadStatic]
        private static Context _current;

        /// <summary>
        /// Window
        /// </summary>
        private IWindow _window;


        /// <summary>
        /// Renderer
        /// </summary>
        private IRenderer _renderer;

        #endregion

        #region Public properties


        /// <summary>
        /// Scene to render
        /// </summary>
        public Scene Scene = new Scene();


        ///// <summary>
        ///// ClientSize
        ///// </summary>
        //public Vector2 ClientSize
        //{ 
        //    get { return _clientSize; }
        //    set { Resize(value.X, value.Y); }
        //}


        /// <summary>
        /// Current context
        /// </summary>
        public static Context Current { get { return _current; } }

        /// <summary>
        /// Renderer
        /// </summary>
        public IRenderer Renderer { get { return _renderer; } }

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

            Input = new Input();
        }

        /// <summary>
        /// Set the current renderer
        /// </summary>
        public Context SetRenderer(IRenderer render)
        {
            _renderer = render;

            if (_window != null)
                _renderer.SetWindow(_window);

            return this;
        }

        /// <summary>
        /// Set the current window
        /// </summary>
        public Context SetWindow(IWindow window)
        {
            _window = window;

            if (_renderer != null)
                _renderer.SetWindow(window);

            return this;
        }

        ///// <summary>
        ///// Open the window
        ///// </summary>
        //public Context OpenWindow(float width, float height, string title)
        //{
        //    if (_window == null)
        //    {
        //        _window = new Window((int)width, (int)height, title, this);

        //        _clientSize = new Vector2(width, height);

        //        Renderer.SetWindow(_window);

        //        this.CenterOnScreen();

        //    }
        //    else
        //    {
        //        //Ensure that the window is visible...
        //        Resize(width, height);

        //        this.Show();
        //    }

        //    return this;
        //}

        ///// <summary>
        ///// Create the context for testing only
        ///// </summary>
        //public Context CreateTest(int width, int height)
        //{
        //    _window = new Window(width, height, "Test", this);

        //    _clientSize = new Vector2(width, height);

        //    this.Hide();

        //    return this;
        //}

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
        /// Sets the window fullscreen on the primary monitor.
        /// </summary>
        public Context Fullscreen()
        {
            EnsureWindowExists();

            _window.Fullscreen();

            return this;
        }



        /// <summary>
        /// Lock the cursor in the window
        /// </summary>
        public Context LockCursor()
        {
            EnsureWindowExists();

            _window.LockCursor();

            return this;
        }

        /// <summary>
        /// Unlock the cursor in the window
        /// </summary>
        public Context UnlockCursor()
        {
            EnsureWindowExists();

            _window.UnlockCursor();

            return this;
        }


        ///// <summary>
        ///// Show the screen
        ///// </summary>
        //public Context Show()
        //{
        //    EnsureWindowExists();

        //    _window.Visible = true;

        //    return this;
        //}

        ///// <summary>
        ///// Hide the screen
        ///// </summary>
        //public Context Hide()
        //{
        //    EnsureWindowExists();

        //    _window.Visible = false;

        //    return this;
        //}

        ///// <summary>
        ///// Resize the screen
        ///// </summary>
        //public Context Resize(float width, float height)
        //{
        //    EnsureWindowExists();

        //    Vector2 newSize = new Vector2((float)width, (float)height);
        //    _window.ClientSize = _clientSize;
        //    _clientSize = newSize;

        //    return this;

        //}

        ///// <summary>
        ///// Set the title of the screen
        ///// </summary>
        //public Context SetTitle(string title)
        //{
        //    EnsureWindowExists();

        //    _window.Title = title;

        //    return this;

        //}

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
            EnsureWindowExists();

            //Now we can initialize the renderer...
            InitInternal();

            //And we are looping...
            while (!_window.IsClosing)
            {

                

                //Custom run code...
                if (runHandler != null)
                    runHandler();

                if (_window.IsClosing)
                    break;
                                
                //Rendering...
                Renderer.Render(Scene);


                //Indicate a new frame...
                Input.OnNewFrame();

                //Get new mouse and keyboard pulls
                _window.DoEvents();
            }

        }


        /// <summary>
        /// Run only one frame and does not swap buffers
        /// Important if we want to grab the framebuffer for screenshots
        /// </summary>
        public void RenderOneFramebuffer(RunHandler runHandler = null)
        {
            EnsureWindowExists();

            //Now we can initialize the renderer...
            InitInternal();

            //Custom run code...
            if (runHandler != null)
                runHandler();

            if (_window.IsClosing)
                return;

            //Rendering...
            _renderer.Render(Scene);

        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void Quit()
        {
            if (_window != null)
                _window.Close();
        }


        /// <summary>
        /// Take a screenshot
        /// </summary>
        public SixLabors.ImageSharp.Image<Rgba32> TakeScreenshot(int x, int y, int width, int height)
        {
            byte[] buffer = _renderer.GetFramebufferRGBA(x, y, width, height);

            return SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(buffer, width, height);

        }



        /// <summary>
        /// Create a new mesh
        /// </summary>
        public Mesh CreateMesh(MeshDefinition meshDefinition)
        {
            return _renderer.CreateMesh(meshDefinition);
        }

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        public Texture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            return _renderer.CreateTexture2D(texDef);
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        public Material CreateMaterial(MaterialDefinition matDef)
        {
            return _renderer.CreateMaterial(matDef);
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_renderer != null)
            {
                _renderer.Dispose();
                _renderer = null;
            }

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
            _renderer.Init();

            _isInitialized = true;


        }


        /// <summary>
        /// Init the Glfw
        /// </summary>
        private void InitGlfw()
        {
            

        }

        /// <summary>
        /// Be sure that the window is created
        /// </summary>
        private void EnsureWindowExists()
        {
            if (_window == null)
                throw new InvalidOperationException("The Window has not been setupped, you must call SetWindow before this method.");
        }


    }
}
