using System;
using System.Runtime.InteropServices;

namespace MiniEngine.Drivers.Vulkan.Interop
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class MarshalString : IDisposable
    {
        private string _content;
        private IntPtr _ptr = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public MarshalString(string str)
        {
            _content = str;
        }



        /// <summary>
        /// Dispose the string
        /// </summary>
        public void Dispose()
        {
            if (_ptr > 0)
            {
                Marshal.FreeHGlobal(_ptr);
                _ptr = 0;
            }
        }
    }
}
