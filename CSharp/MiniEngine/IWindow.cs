using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Interface for the window used in the Context
    /// </summary>
    public interface IWindow: IDisposable
    {
        /// <summary>
        /// Event when the window is resized
        /// </summary>
        event Action<Vector2> OnWindowResized;

        /// <summary>
        /// Indicate if the window is closing (should stop the application)
        /// </summary>
        bool IsClosing { get; }

        /// <summary>
        /// Get or Set the ClientSize
        /// </summary>
        Vector2 ClientSize { get; set; }

        /// <summary>
        /// Do the input events
        /// </summary>
        void DoEvents();

        /// <summary>
        /// Close the window
        /// </summary>
        void Close();

        /// <summary>
        /// Create a surface
        /// </summary>
        void CreateSurface(IntPtr instance, IntPtr surface);

        /// <summary>
        /// Lock the cursor in the window
        /// </summary>
        void LockCursor();

        /// <summary>
        /// Unlock the cursor in the window
        /// </summary>
        void UnlockCursor();

        /// <summary>
        ///     Centers the on window on the screen.
        ///     <para>Has no effect on fullscreen or maximized windows.</para>
        /// </summary>
        void CenterOnScreen();

        /// <summary>
        ///     Sets the window fullscreen on the primary monitor.
        /// </summary>
        void Fullscreen();

    }
}
