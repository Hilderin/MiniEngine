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
        private int _targetFramerate = -1;
        private TimeSpan _targetElapsedTimePerFrame;    // = TimeSpan.FromTicks(166667); // 60fps
        private long _previousTicks = 0;



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

        /// <summary>
        /// Constructor
        /// </summary>
        public FrameLoop()
        {
            _gameTimer = Stopwatch.StartNew();

            //Start the watcher for debugger if one is attached. We don't support attach after starting the process for that.
            if (System.Diagnostics.Debugger.IsAttached)
                DebugTimeWatcher.Start();

            RecalculateTargetElapsedTime();
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
                FpsCounter.AddNewFrameTime(currentTicks);

                if (!tickAction())
                    return;

            }

        }

        /// <summary>
        /// Recalculate the targetet elapsedtime from the target framerate
        /// </summary>
        private void RecalculateTargetElapsedTime()
        {
            if (_targetFramerate >= 0)
                _targetElapsedTimePerFrame = TimeSpan.FromTicks((long)(1f / _targetFramerate * 10000000L));
            else
                _targetElapsedTimePerFrame = TimeSpan.Zero;
        }

        /// <summary>
        /// FpsCounter
        /// </summary>
        private static class FpsCounter
        {
            private const int NB_FRAMES_FPS = 40;
            private static long[] _lastFramesTicks = new long[NB_FRAMES_FPS];
            private static int _nextFrameIndex = 0;
            private static long _totalTickforLastFrames = 0;
            private static int _nbFrames = 0;
            private static long _previousTicks = 0;
            private static long _lastFrameUpdateFPS = 0;
            private static long _nbTicksBetweenFPSUpdates = (long)(10000000D * 0.1);        //0.1 sec

            /// <summary>
            /// Add a frame
            /// </summary>
            public static void AddNewFrameTime(long totalElapsedTicks)
            {

                long frameTicks = totalElapsedTicks - _previousTicks;
                _previousTicks = totalElapsedTicks;


                long previousTicks = _lastFramesTicks[_nextFrameIndex];
                _lastFramesTicks[_nextFrameIndex] = frameTicks;

                _totalTickforLastFrames -= previousTicks;
                _totalTickforLastFrames += frameTicks;

                _nextFrameIndex++;
                if (_nextFrameIndex >= NB_FRAMES_FPS)
                    _nextFrameIndex = 0;

                if (_nbFrames < NB_FRAMES_FPS)
                    _nbFrames++;

                if (totalElapsedTicks - _lastFrameUpdateFPS > _nbTicksBetweenFPSUpdates)
                {
                    double nbSecondsTotal = _totalTickforLastFrames / 10000000D;
                    if (nbSecondsTotal > 0)
                        Time.FramePerSeconds = Math.RoundInt(_nbFrames / nbSecondsTotal);
                    else
                        Time.FramePerSeconds = 0;
                    _lastFrameUpdateFPS = totalElapsedTicks;
                }

                //Debug.Print(frameTicks.ToString() + " => " + _averageFps.ToString() + " (" + _nbFrames + " / " + nbSecondsTotal + ")");
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
