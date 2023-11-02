using System;
using System.Collections.Generic;
using System.Diagnostics;
using MiniEngine.ResourceDefinitions;
using SixLabors.ImageSharp.PixelFormats;

namespace MiniEngine
{

    /// <summary>
    /// Context for a game instance
    /// </summary>
    public class Context : IDisposable
    {

        #region Private members

        private bool _isInitialized = false;
        private bool _isDisposed = false;
        private IWindow _window;
        private IRenderer _renderer;
        private DebugCallback _debugCallback;
        private FrameLoop _frameLoop;

        /// <summary>
        /// Current context
        /// </summary>
        private static Context _current;

        /// <summary>
        /// Methods to call at each frame
        /// </summary>
        private List<Action> _updates = new List<Action>();

        /// <summary>
        /// Methods to call at the beginning of the next frame once before the updates
        /// </summary>
        private List<Action> _onces = new List<Action>();

        #endregion

        #region Public properties


        ///// <summary>
        ///// Scene to render
        ///// </summary>
        //public Scene Scene;

        /// <summary>
        /// AssetManager
        /// </summary>
        public AssetManager Asset;


        /// <summary>
        /// Current context
        /// </summary>
        public static Context Current
        { 
            get
            {
                _current ??= new Context();
                return _current;
            } 
        }

        /// <summary>
        /// Renderer
        /// </summary>
        public IRenderer Renderer { get { return _renderer; } }

        /// <summary>
        /// Input manager
        /// </summary>
        public readonly InputManager Input;

        /// <summary>
        /// Indicate if we are in debug mode
        /// </summary>
        public bool DebugEnabled;


        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Context()
        {
            _current = this;

            Input = new InputManager();

            Asset = new AssetManager(this);

            _frameLoop = new FrameLoop();
        }

        /// <summary>
        /// Enable debug
        /// </summary>
        public Context EnableDebug(DebugCallback callback = null)
        {
            _debugCallback = callback;

            _renderer?.EnableDebug(RendererDebugCallback);

            DebugEnabled = true;

            System.Diagnostics.Trace.Listeners.Add(new MiniEngineTraceListener());

            Asset.StartWatchUpdateContent();

            return this;
        }

        /// <summary>
        /// Set the current renderer
        /// </summary>
        public Context SetRenderer(IRenderer render)
        {
            _renderer = render;

            if (_window != null)
                _renderer.SetWindow(_window);

            if (DebugEnabled)
                _renderer.EnableDebug(RendererDebugCallback);

            return this;
        }

        /// <summary>
        /// Set the window handle for win32 (Windows)
        /// </summary>
        public Context SetWindow32Handle(IntPtr handle)
        {
            _renderer?.SetWindow32Handle(handle);

            return this;
        }

        /// <summary>
        /// Set the current window
        /// </summary>
        public Context SetWindow(IWindow window)
        {
            _window = window;

            _renderer?.SetWindow(window);

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
        /// <summary>
        /// Init the game/application
        /// </summary>
        public Context Init(Action initHandler = null)
        {
            InitInternal();

            if (initHandler != null)
                initHandler();

            return this;
        }


        /// <summary>
        /// Run the game/application
        /// </summary>
        public void Run(Action runHandler = null)
        {
            EnsureWindowExists();

            //Now we can initialize the renderer...
            InitInternal();

            _frameLoop.RunLoop(() => RunOneFrame(runHandler));

        }


        /// <summary>
        /// Run only one frame and does not swap buffers
        /// Important if we want to grab the framebuffer for screenshots
        /// </summary>
        public void RenderOneFramebuffer(Action runHandler = null)
        {
            EnsureWindowExists();

            //Now we can initialize the renderer...
            InitInternal();

            RunOneFrame(runHandler);

        }

        /// <summary>
        /// Excute on frame
        /// </summary>
        private bool RunOneFrame(Action runHandler)
        {
           
            //Custom run code...
            if (runHandler != null)
                runHandler();

            if (_window.IsClosing)
                return false;

            RecalculateNextFrame();

            //Rendering...
            Renderer.Render();


            //Indicate a new frame...
            Input.OnNewFrame();

            //Get new mouse and keyboard pulls
            _window.DoEvents();

            return true;
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void Quit()
        {
            _window?.Close();
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
        /// Register an update action
        /// </summary>
        public void RegisterUpdate(Action updateAction)
        {
            _updates.Add(updateAction);

        }

        /// <summary>
        /// Register an action the execute at the beginning of the next frame once before the updates
        /// </summary>
        public void RegisterOnce(Action onceAction)
        {
            _onces.Add(onceAction);

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
        /// Be sure that the window is created
        /// </summary>
        private void EnsureWindowExists()
        {
            if (_window == null)
                throw new InvalidOperationException("The Window has not been setupped, you must call SetWindow before this method.");
        }


        /// <summary>
        /// Callback in debug mode
        /// </summary>
        private void RendererDebugCallback(DebugLevel level, int messageCode, string message)
        {
            if (_debugCallback != null)
                _debugCallback(level, messageCode, message);

            if (level == DebugLevel.Error)
                throw new Exception($"Renderer error: {message}");

            Debug.WriteLine($"{level}: {message}");



        }

        /// <summary>
        /// Recalculate information for the next frame
        /// </summary>
        private void RecalculateNextFrame()
        {

            //-------------------
            //Executes onces...
            if (_onces.Count > 0)
            {
                for (int i = 0; i < _onces.Count; i++)
                {
                    _onces[i]();
                }
                _onces.Clear();
            }


            //-------------------
            //Executes updates...
            for (int i = 0; i < _updates.Count; i++)
            {
                _updates[i]();
            }

        }

    }
}
