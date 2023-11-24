using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Profiling
{
    /// <summary>
    /// Information to display inthe profiler
    /// </summary>
    public class ProfilerInfo
    {

        /// <summary>
        /// Profiler
        /// </summary>
        private Profiler _profiler;

        /// <summary>
        /// Info name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Info value
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal ProfilerInfo(string name, Profiler profiler)
        {
            Name = name;

            _profiler = profiler;
        }

        /// <summary>
        /// Start the step
        /// </summary>
        [Conditional("DEBUG")]
        public void Update(string value)
        {
            Value = value;
        }
    }
}
