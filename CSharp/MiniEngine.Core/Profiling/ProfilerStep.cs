using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Profiling
{
    /// <summary>
    /// Step in the profiling
    /// </summary>
    public class ProfilerStep
    {
        /// <summary>
        /// Ticks at the beginning
        /// </summary>
        private long _ticksBegin;

        /// <summary>
        /// Profiler
        /// </summary>
        private Profiler _profiler;

        /// <summary>
        /// Step name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Number of calls
        /// </summary>
        public long NbCalls { get; private set; }

        /// <summary>
        /// Ticks for the last frame
        /// </summary>
        public long LastTicks { get; private set; }

        /// <summary>
        /// Total ticks
        /// </summary>
        public long TotalTicks { get; private set; }

        /// <summary>
        /// Last time in ms
        /// </summary>
        public float LastMilliseconds => (float)(LastTicks / 10000D);

        /// <summary>
        /// Total time in ms
        /// </summary>
        public float TotalMilliseconds => (float)(TotalTicks / 10000D);

        /// <summary>
        /// Constructor
        /// </summary>
        internal ProfilerStep(string name, Profiler profiler)
        {
            Name = name;

            _profiler = profiler;
        }

        /// <summary>
        /// Start the step
        /// </summary>
        [Conditional("DEBUG")]
        public void Begin()
        {
            _ticksBegin = _profiler.Elapsed.ElapsedTicks;
            NbCalls++;
        }

        /// <summary>
        /// End the step
        /// </summary>
        [Conditional("DEBUG")]
        public void End()
        {
            LastTicks = (_profiler.Elapsed.ElapsedTicks - _ticksBegin);
            TotalTicks += LastTicks;
        }

        /// <summary>
        /// Reset the stats
        /// </summary>
        [Conditional("DEBUG")]
        public void Reset()
        {
            NbCalls = 0;
            TotalTicks = 0;
        }
    }
}
