using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Profiling
{
    /// <summary>
    /// Profiler instance
    /// </summary>
    public class Profiler
    {
        /// <summary>
        /// Ticks at the beginning
        /// </summary>
        private long _ticksBeginFrame;

        /// <summary>
        /// Window visible
        /// </summary>
        public bool IsWindowVisible { get; set; }

        /// <summary>
        /// Elapsed timer
        /// </summary>
        internal Stopwatch Elapsed { get; private set; } = Stopwatch.StartNew();

        /// <summary>
        /// Steps
        /// </summary>
        private List<ProfilerStep> _steps = new List<ProfilerStep>();


        /// <summary>
        /// Infos
        /// </summary>
        private List<ProfilerInfo> _infos = new List<ProfilerInfo>();

        /// <summary>
        /// Profiler name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Number of frame
        /// </summary>
        public long NbFrame { get; private set; }


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
        public Profiler(string name)
        {
            Name = name;
        }


        /// <summary>
        /// Start a new frame
        /// </summary>
        [Conditional("DEBUG")]
        public void BeginNewFrame()
        {
            _ticksBeginFrame = Elapsed.ElapsedTicks;
            NbFrame++;
        }

        /// <summary>
        /// End the current frame
        /// </summary>
        [Conditional("DEBUG")]
        public void EndNewFrame()
        {
            LastTicks = (Elapsed.ElapsedTicks - _ticksBeginFrame);
            TotalTicks += LastTicks;
        }

        /// <summary>
        /// Add a step
        /// </summary>
        public ProfilerStep AddStep(string name)
        {
            ProfilerStep step = new ProfilerStep(name, this);
            lock (_steps)
                _steps.Add(step);
            return step;

        }

        /// <summary>
        /// Add an info
        /// </summary>
        public ProfilerInfo AddInfo(string name)
        {
            ProfilerInfo info = new ProfilerInfo(name, this);
            lock (_infos)
                _infos.Add(info);
            return info;

        }


        /// <summary>
        /// Reset the profiler stats
        /// </summary>
        [Conditional("DEBUG")]
        public void Reset()
        {
            foreach (var step in _steps)
                step.Reset();
        }

        /// <summary>
        /// Toggle window visibility
        /// </summary>
        public void ToggleWindowVisibility()
        {
            IsWindowVisible = !IsWindowVisible;
        }

        /// <summary>
        /// Show profiler window
        /// </summary>
        public void ShowProfilerWindow()
        {

            if (!IsWindowVisible)
                return;

            if (ImGui.Begin(Name))
            {
                if (ImGui.BeginTable(Name, 7))
                {



                    ImGui.TableSetupColumn("Step");
                    ImGui.TableSetupColumn("Last time (ms)");
                    ImGui.TableSetupColumn("Last time (%)");
                    ImGui.TableSetupColumn("NbCalls");
                    ImGui.TableSetupColumn("Avg time (ms)");
                    ImGui.TableSetupColumn("Avg time (%)");
                    ImGui.TableSetupColumn("Total time (ms)");
                    ImGui.TableHeadersRow();

                    float totalLastMs = 0;
                    float totaltotalMs = 0;
                    float totalAvgTime = NbFrame > 0 ? (TotalMilliseconds / NbFrame) : 0;
                    foreach (var step in _steps)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(step.Name);

                        ImGui.TableSetColumnIndex(1);

                        ImGui.Text(step.LastMilliseconds.ToString("0.000"));

                        ImGui.TableSetColumnIndex(2);
                        if (LastMilliseconds > 0)
                            ImGui.Text((step.LastMilliseconds / LastMilliseconds * 100f).ToString("0") + "%%");
                        else
                            ImGui.Text("---");

                        ImGui.TableSetColumnIndex(3);
                        ImGui.Text(step.NbCalls.ToString());

                        ImGui.TableSetColumnIndex(4);
                        if (step.NbCalls > 0)
                            ImGui.Text((step.TotalMilliseconds / step.NbCalls).ToString("0.000"));
                        else
                            ImGui.Text("---");

                        ImGui.TableSetColumnIndex(5);
                        if (totalAvgTime > 0 && step.NbCalls > 0)
                            ImGui.Text(((step.TotalMilliseconds / step.NbCalls) / totalAvgTime * 100f).ToString("0") + "%%");
                        else
                            ImGui.Text("---");

                        ImGui.TableSetColumnIndex(6);
                        ImGui.Text(step.TotalMilliseconds.ToString("0.000"));

                        totalLastMs += step.LastMilliseconds;
                        totaltotalMs += step.TotalMilliseconds;
                    }


                    //Missing time
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Other");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text((LastMilliseconds - totalLastMs).ToString("0.000"));

                    ImGui.TableSetColumnIndex(2);
                    if (LastMilliseconds > 0)
                        ImGui.Text(((LastMilliseconds - totalLastMs) * 100f).ToString("0") + "%%");
                    else
                        ImGui.Text("---");

                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text(NbFrame.ToString());

                    ImGui.TableSetColumnIndex(4);
                    if (NbFrame > 0)
                        ImGui.Text(((TotalMilliseconds - totaltotalMs) / NbFrame).ToString("0.000"));
                    else
                        ImGui.Text("---");

                    ImGui.TableSetColumnIndex(5);
                    if (totalAvgTime > 0 && NbFrame > 0)
                        ImGui.Text((((TotalMilliseconds - totaltotalMs) / NbFrame) / totalAvgTime * 100f).ToString("0") + "%%");
                    else
                        ImGui.Text("---");

                    ImGui.TableSetColumnIndex(6);
                    ImGui.Text((TotalMilliseconds - totaltotalMs).ToString("0.000"));


                    //Totals
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Totals");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(LastMilliseconds.ToString("0.000"));

                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text("100%%");

                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text(NbFrame.ToString());

                    ImGui.TableSetColumnIndex(4);
                    if (NbFrame > 0)
                        ImGui.Text((TotalMilliseconds / NbFrame).ToString("0.000"));
                    else
                        ImGui.Text("---");

                    ImGui.TableSetColumnIndex(5);
                    ImGui.Text("100%%");

                    ImGui.TableSetColumnIndex(6);
                    ImGui.Text(TotalMilliseconds.ToString("0.000"));

                    ImGui.EndTable();

                    //Informations....
                    ImGui.Separator();
                }


                foreach (var info in _infos)
                {
                    ImGui.Text($"{info.Name}: {info.Value}");
                }
            }

            ImGui.End();

        }


    }
}
