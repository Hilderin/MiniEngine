using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Provides information on the time
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// The interval in seconds from the last frame to the current one
        /// </summary>
        public static float DeltaTime { get; private set; }
    }
}
