using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Manager for the time and the framerate
    /// </summary>
    public class FrameLoop
    {
        private Stopwatch _gameTimer = null;
        private int _targetFramerate = 60;
        private TimeSpan _targetElapsedTimePerFrame;    // = TimeSpan.FromTicks(166667); // 60fps
        private long _previousTicks = 0;


        /// <summary>
        /// ElapsedTime between frame
        /// </summary>
        public TimeSpan TargetElapsedTime { get { return _targetElapsedTimePerFrame; } set { _targetElapsedTimePerFrame = value; } }


        /// <summary>
        /// Indicate if server running
        /// </summary>
        public int TargetFramerate
        {
            get { return _targetFramerate; }
            set
            {
                _targetFramerate = value;
                RecalculateTargetElapsedTime();
            }
        }

        private void RecalculateTargetElapsedTime()
        {
            if (_targetFramerate >= 0)
                _targetElapsedTimePerFrame = TimeSpan.FromTicks((long)(1f / _targetFramerate * 10000000L));
            else
                _targetElapsedTimePerFrame = TimeSpan.Zero;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public FrameLoop()
        {
            _gameTimer = Stopwatch.StartNew();


            DebugTimeWatcher.Start();
        }

        /// <summary>
        /// Execute the loop
        /// </summary>
        public void RunLoop(Func<bool> tickAction)
        {
            TimeSpan elapsed = _gameTimer.Elapsed;
            float timePassInDebug = 0f;

            while (true)
            {
                Time.LastFrameGenerationTime = _gameTimer.Elapsed - elapsed;
                
                if (_targetElapsedTimePerFrame.Ticks > 0)
                {
                    if (_targetElapsedTimePerFrame > Time.LastFrameGenerationTime)
                        System.Threading.Thread.Sleep(_targetElapsedTimePerFrame - Time.LastFrameGenerationTime);
                }

                elapsed = _gameTimer.Elapsed;
                long currentTicks = elapsed.Ticks;
                TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - _previousTicks);
                _previousTicks = currentTicks;

                if (DebugTimeWatcher.AccumulatedTimeStopped.Ticks > 0)
                {
                    timePassInDebug += (float)DebugTimeWatcher.AccumulatedTimeStopped.TotalSeconds;
                    timeAdvanced -= DebugTimeWatcher.AccumulatedTimeStopped;
                    DebugTimeWatcher.AccumulatedTimeStopped = TimeSpan.Zero;
                    if (timeAdvanced.Ticks < 0)
                        timeAdvanced = _targetElapsedTimePerFrame;
                }

                if (_targetElapsedTimePerFrame.Ticks > 0)
                {
                    while (timeAdvanced >= _targetElapsedTimePerFrame * 2)
                    {
                        Time.DeltaTime = (float)_targetElapsedTimePerFrame.TotalSeconds;
                        Time.TotalTime += Time.DeltaTime;
                        timeAdvanced -= _targetElapsedTimePerFrame;

                        TimeSpan beforeFrame = _gameTimer.Elapsed;
                        if (!tickAction())
                            return;
                        Time.LastFrameGenerationTime = _gameTimer.Elapsed - beforeFrame;
                    }
                }

                Time.DeltaTime = (float)timeAdvanced.TotalSeconds;
                Time.TotalTime = (float)elapsed.TotalSeconds - timePassInDebug;

                if (!tickAction())
                    return;

            }

        }

        /// <summary>
        /// Class to be able to accumulate the time diffenrencial
        /// </summary>
        private static class DebugTimeWatcher
        {
            private static Stopwatch _stopwatch;
            private static System.Timers.Timer pollingTimer;
            private static TimeSpan _lastMeasuredDebugTimespan;

            public static TimeSpan AccumulatedTimeStopped;


            public static void Start()
            {
                _stopwatch = Stopwatch.StartNew();

                pollingTimer = new System.Timers.Timer();
                pollingTimer.Interval = 10;
                pollingTimer.Enabled = true;
                pollingTimer.Elapsed += PollingTimer_Elapsed;
            }

            private static void PollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                lock (_stopwatch)
                {

                    TimeSpan elapsed = _stopwatch.Elapsed;
                    if ((elapsed - _lastMeasuredDebugTimespan).TotalMilliseconds > 100)
                    {
                        AccumulatedTimeStopped += (elapsed - _lastMeasuredDebugTimespan);
                    }
                    _lastMeasuredDebugTimespan = elapsed;
                }

            }

        }
    }
}
