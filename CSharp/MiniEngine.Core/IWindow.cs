using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    ///     Indicates the behavior of the mouse cursor.
    /// </summary>
    public enum CursorMode
    {
        /// <summary>
        ///     The cursor is visible and behaves normally.
        /// </summary>
        Normal = 0x00034001,

        /// <summary>
        ///     The cursor is invisible when it is over the client area of the window but does not restrict the cursor from
        ///     leaving.
        /// </summary>
        Hidden = 0x00034002,

        /// <summary>
        ///     Hides and grabs the cursor, providing virtual and unlimited cursor movement. This is useful for implementing for
        ///     example 3D camera controls.
        /// </summary>
        Disabled = 0x00034003
    }

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
        /// Current CursorMode
        /// </summary>
        CursorMode CursorMode { get; }

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
        /// Show the cursor in the window in normal mode
        /// </summary>
        void ShowCursor();

        /// <summary>
        /// Hide the cursor in the window
        /// </summary>
        void HideCursor();


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
