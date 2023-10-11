using System;
using System.Collections.Generic;
using MiniEngine.GLFW;
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

        /// <summary>
        /// The maximum number of point lights
        /// </summary>
        public const int MAX_POINT_LIGHTS = 2;

        /// <summary>
        /// The maximum number of spot lights
        /// </summary>
        public const int MAX_SPOT_LIGHTS = 2;

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
        /// ClientSize
        /// </summary>
        private Vector2 _clientSize = new Vector2(1200, 800);

        /// <summary>
        /// Current context
        /// </summary>
        [ThreadStatic]
        private static Context _current;

        /// <summary>
        /// Window
        /// </summary>
        private Window _window;

        /// <summary>
        /// Meshes to render
        /// </summary>
        private List<Mesh> _meshes = new List<Mesh>();

        /// <summary>
        /// Renderer
        /// </summary>
        private IRenderer _renderer;

        #endregion

        #region Public properties


        /// <summary>
        /// Current camera
        /// </summary>
        public Camera Camera = new Camera();

        /// <summary>
        /// List of meshes
        /// </summary>
        public List<Mesh> Meshes { get { return _meshes; } }

        /// <summary>
        /// Directional light
        /// </summary>
        public DirectionalLight DirectionalLight = null;

        /// <summary>
        /// Default ambient light
        /// </summary>
        public AmbientLight AmbientLight = AmbientLight.Default;

        /// <summary>
        /// Point lights
        /// </summary>
        public List<PointLight> PointLights = new List<PointLight>(MAX_POINT_LIGHTS);

        /// <summary>
        /// Spot lights
        /// </summary>
        public List<SpotLight> SpotLights = new List<SpotLight>(MAX_SPOT_LIGHTS);


        /// <summary>
        /// ClientSize
        /// </summary>
        public Vector2 ClientSize
        { 
            get { return _clientSize; }
            set { Resize(value.X, value.Y); }
        }


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
        public Context(IRenderer renderer)
        {
            _current = this;

            renderer.PreInitGlfw();

            _renderer = renderer;

            Input = new Input();
        }

        /// <summary>
        /// Open the window
        /// </summary>
        public Context OpenWindow(float width, float height, string title)
        {
            if (this._window == null)
            {
                this._window = new Window((int)width, (int)height, title, this);

                _clientSize = new Vector2(width, height);

                Renderer.SetWindow(this._window);

                this.CenterOnScreen();

            }
            else
            {
                //Ensure that the window is visible...
                Resize(width, height);

                this.Show();
            }

            return this;
        }

        /// <summary>
        /// Create the context for testing only
        /// </summary>
        public Context CreateTest(int width, int height)
        {
            this._window = new Window(width, height, "Test", this);

            _clientSize = new Vector2(width, height);

            this.Hide();

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
        /// Show the screen
        /// </summary>
        public Context Show()
        {
            EnsureWindowExists();

            _window.Visible = true;

            return this;
        }

        /// <summary>
        /// Hide the screen
        /// </summary>
        public Context Hide()
        {
            EnsureWindowExists();

            _window.Visible = false;

            return this;
        }

        /// <summary>
        /// Resize the screen
        /// </summary>
        public Context Resize(float width, float height)
        {
            EnsureWindowExists();

            Vector2 newSize = new Vector2((float)width, (float)height);
            this._window.ClientSize = _clientSize;
            _clientSize = newSize;

            return this;

        }

        /// <summary>
        /// Set the title of the screen
        /// </summary>
        public Context SetTitle(string title)
        {
            EnsureWindowExists();

            this._window.Title = title;

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
        /// Add a mesh to render
        /// </summary>
        public void Add(Mesh mesh)
        {
            _meshes.Add(mesh);
        }

        /// <summary>
        /// Remove a mesh
        /// </summary>
        public void Remove(Mesh mesh)
        {
            _meshes.Remove(mesh);
        }

        /// <summary>
        /// Add a point light to render
        /// </summary>
        public void Add(PointLight pointLight)
        {
            this.PointLights.Add(pointLight);
        }

        /// <summary>
        /// Remove a point light to render
        /// </summary>
        public void Remove(PointLight pointLight)
        {
            this.PointLights.Remove(pointLight);
        }

        /// <summary>
        /// Add a spot light to render
        /// </summary>
        public void Add(SpotLight SpotLight)
        {
            this.SpotLights.Add(SpotLight);
        }

        /// <summary>
        /// Remove a spot light to render
        /// </summary>
        public void Remove(SpotLight SpotLight)
        {
            this.SpotLights.Remove(SpotLight);
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
            while (!this._window.IsClosing)
            {

                _renderer.Clear();

                //Custom run code...
                if (runHandler != null)
                    runHandler();

                if (this._window.IsClosing)
                    break;

                //Rendering...
                Renderer.Render(this);

                //Swapping buffer...
                if(Renderer.ShouldSwapBuffer)
                    this._window.SwapBuffers();

                //Indicate a new frame...
                Input.OnNewFrame();

                //Get new mouse and keyboard pulls
                Glfw.PollEvents();
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

            _renderer.Clear();

            //Custom run code...
            if (runHandler != null)
                runHandler();

            if (this._window.IsClosing)
                return;

            //Rendering...
            _renderer.Render(this);

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
        /// Take a screenshot
        /// </summary>
        public SixLabors.ImageSharp.Image<Rgba32> TakeScreenshot(int x, int y, int width, int height)
        {
            byte[] buffer = _renderer.GetFramebufferRGBA(x, y, width, height);

            return SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(buffer, width, height);

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
            {
                this._window = new Window((int)_clientSize.X, (int)_clientSize.Y, "MiniEngine", this);
            }
        }


    }
}
