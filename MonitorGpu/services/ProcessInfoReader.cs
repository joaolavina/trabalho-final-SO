using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MonitorGpu.Models;

namespace MonitorGpu.Services
{
    public class ProcessInfoReader
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public ulong PeakWorkingSetSize;
            public ulong WorkingSetSize;
            public ulong QuotaPeakPagedPoolUsage;
            public ulong QuotaPagedPoolUsage;
            public ulong QuotaPeakNonPagedPoolUsage;
            public ulong QuotaNonPagedPoolUsage;
            public ulong PagefileUsage;
            public ulong PeakPagefileUsage;
        }

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

        private Dictionary<int, (string Name, uint PageFaults)> GetProcessesSnapshot()
        {
            var snapshot = new Dictionary<int, (string, uint)>();
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (GetProcessMemoryInfo(proc.Handle, out var counters, (uint)Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS))))
                    {
                        snapshot[proc.Id] = (proc.ProcessName, counters.PageFaultCount);
                    }
                }
                catch
                {
                }
            }
            return snapshot;
        }

        public List<ProcessInfo> GetProcessesPageFaults()
        {
            var snapshot1 = GetProcessesSnapshot();

            Thread.Sleep(1000);

            var snapshot2 = GetProcessesSnapshot();

            var result = new List<ProcessInfo>();

            foreach (var kvp in snapshot2)
            {
                int pid = kvp.Key;
                string name = kvp.Value.Name;
                uint faults2 = kvp.Value.PageFaults;

                if (snapshot1.TryGetValue(pid, out var prev))
                {
                    uint faults1 = prev.PageFaults;
                    float perSec = (faults2 - faults1) / (1000 / 1000f);
                    result.Add(new ProcessInfo
                    {
                        Id = pid,
                        ProcessName = name,
                        PageFaults = perSec
                    });
                }
                else
                {
                    result.Add(new ProcessInfo
                    {
                        Id = pid,
                        ProcessName = name,
                        PageFaults = 0
                    });
                }
            }

            return result;
        }
    }
}
