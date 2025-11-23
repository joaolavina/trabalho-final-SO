using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using MonitorGpu.Models;

namespace MonitorGpu.Services
{
    public class ProcessInfoReader
    {
        private Dictionary<int, PerformanceCounter> counters = new Dictionary<int, PerformanceCounter>();

        public List<ProcessInfo> GetProcessesPageFaults()
        {
            var result = new List<ProcessInfo>();
            var processes = Process.GetProcesses();

            foreach (var proc in processes)
            {
                try
                {
                    if (!counters.ContainsKey(proc.Id))
                    {
                        var pc = new PerformanceCounter("Process", "Page Faults/sec", proc.ProcessName, true);
                        counters[proc.Id] = pc;

                        pc.NextValue();
                    }

                    float faultsPerSec = counters[proc.Id].NextValue();
                    result.Add(new ProcessInfo
                    {
                        Id = proc.Id, 
                        ProcessName = proc.ProcessName, 
                        PageFaults = faultsPerSec
                    });
                }
                catch (Exception)
                {
                }
            }

            return result;
        }

        public void DisposeCounters()
        {
            foreach (var pc in counters.Values)
            {
                pc.Dispose();
            }
            counters.Clear();
        }
    }
}
