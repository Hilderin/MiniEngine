using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{

    internal class NativeReference : IDisposable
    {
        internal IntPtr Handle { get; private set; }

        internal NativeReference(int size, bool zero = false)
        {
            Handle = Marshal.AllocHGlobal(size);
            if (NativeMemoryDebug.Enabled)
            {
                lock (NativeMemoryDebug.Allocations)
                {
                    NativeMemoryDebug.Allocations[Handle] = size;
                    NativeMemoryDebug.AllocatedSize += size;
                }
            }
            if (!zero)
                return;
            unsafe
            {
                byte* bptr = (byte*)Handle;
                for (int i = 0; i < size; i++)
                    bptr[i] = 0;
            }
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                if (NativeMemoryDebug.Enabled)
                {
                    lock (NativeMemoryDebug.Allocations)
                    {
                        NativeMemoryDebug.AllocatedSize -= NativeMemoryDebug.Allocations[Handle];
                        if (NativeMemoryDebug.Allocations.ContainsKey(Handle))
                            NativeMemoryDebug.Allocations.Remove(Handle);
                        else
                            NativeMemoryDebug.Report("unknown handle found: {0}", Handle);
                    }
                }
                Marshal.FreeHGlobal(Handle);
            }
            Handle = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~NativeReference()
        {
            Dispose();
        }
    }

    internal class NativePointer
    {
        internal NativeReference Reference { get; private set; }
        internal IntPtr Handle { get; private set; }

        internal NativePointer(NativeReference reference, IntPtr pointer)
        {
            Reference = reference;
            Handle = pointer;
        }

        internal NativePointer(NativeReference reference)
        {
            Reference = reference;
            Handle = reference.Handle;
        }

        internal void Release()
        {
            Reference = null;
            Handle = IntPtr.Zero;
        }
    }

    public class MarshalledObject : IDisposable, IMarshalling
    {
        internal NativePointer native;

        IntPtr IMarshalling.Handle
        {
            get
            {
                return native.Handle;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            native.Release();
            native = null;
        }

        ~MarshalledObject()
        {
            Dispose(false);
        }
    }
}
