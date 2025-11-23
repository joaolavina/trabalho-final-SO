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

        private Dictionary<int, (string Name, uint PageFaults)> GetProcessesState() // pegar estado atual dos processos
        {
            var state = new Dictionary<int, (string, uint)>();

            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (GetProcessMemoryInfo(proc.Handle, out var counters, (uint)Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS))))
                    {
                        state[proc.Id] = (proc.ProcessName, counters.PageFaultCount);
                    }
                }
                catch {  }
            }

            return state;
        }

        public List<ProcessInfo> GetProcessesPageFaults() 
        {
            var state1 = GetProcessesState();

            Thread.Sleep(1000);

            var state2 = GetProcessesState();

            var result = new List<ProcessInfo>();

            foreach (var proc in state2)
            {
                int pid = proc.Key; 
                string name = proc.Value.Name;
                uint faults2 = proc.Value.PageFaults;

                if (state1.TryGetValue(pid, out var prev)) // diferença do estado 2 para o 1
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
