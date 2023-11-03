using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Trace listener to trap Debug.Print, Debug.PrintLine....
    /// </summary>
    public class DebugTraceListener : TraceListener
    {

        /// <summary>
        /// Log on screen
        /// </summary>
        private StringBuilder _log = new StringBuilder();
        private List<LogData> _logData = new List<LogData>();

        /// <summary>
        /// Constructor
        /// </summary>
        public DebugTraceListener()
        {
            Context.Current.RegisterUpdate(Update);
        }

        /// <summary>
        /// Write
        /// </summary>
        public override void Write(string message)
        {
            HandleLog(message);
        }

        /// <summary>
        /// Write Line
        /// </summary>
        public override void WriteLine(string message)
        {
            HandleLog(message + Environment.NewLine);
        }

        /// <summary>
        /// Update a each frame
        /// </summary>
        private void Update()
        {
            if (_logData.Count > 0)
            {
                _log.Clear();
                int indexToDelete = -1;
                
                //We will only keep the 30 most recent
                if (_logData.Count > 30)
                    _logData.RemoveRange(0, _logData.Count - 30);

                for (int i = _logData.Count - 1; i >= 0 ; i--)
                {
                    if (Time.TotalTime - _logData[i].LogTime > 3)
                    {
                        indexToDelete = i;
                    }
                    else
                    {
                        _log.AppendLine(_logData[i].Data);
                    }
                }

                if (indexToDelete >= 0)
                    _logData.RemoveRange(0, indexToDelete + 1);

                if (_log.Length > 0)
                {
                    ImGui.Begin("DebugOutput", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs);
                    ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));
                    ImGui.Text(_log.ToString());
                    ImGui.End();
                }
            }
        }

        /// <summary>
        /// Handle console log add
        /// </summary>
        private void HandleLog(string message)
        {
            _logData.Add(new LogData() { LogTime = Time.TotalTime, Data = message });
        }

        /// <summary>
        /// Information on a log
        /// </summary>
        private struct LogData
        {
            public float LogTime;
            public string Data;
        }
    }
}
