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
        public static float DeltaTime { get; internal set; }

        /// <summary>
        /// Total time that the application is running (minus when paused)
        /// </summary>
        public static float TotalTime { get; internal set; }

        /// <summary>
        /// The time that took the last frame to generate
        /// </summary>
        public static TimeSpan LastFrameGenerationTime { get; internal set; }
        
    }
}
