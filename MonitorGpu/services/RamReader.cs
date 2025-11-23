using System;
using System.Runtime.InteropServices;

namespace MonitorGpu.Services
{
    public class RamReader
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public (ulong totalBytes, ulong availBytes, float usedPercent) GetMemoryStatus()
        {
            var memStatus = new MEMORYSTATUSEX();
            if (!GlobalMemoryStatusEx(memStatus))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            ulong total = memStatus.ullTotalPhys;
            ulong avail = memStatus.ullAvailPhys;
            float usedPercent = (float)((double)(total - avail) * 100.0 / total);
            return (total, avail, usedPercent);
        }

        public string GetReadableUsage()
        {
            var (total, avail, usedPercent) = GetMemoryStatus();
            ulong used = total - avail;

            const ulong OneGB = 1024UL * 1024UL * 1024UL;

            if (used >= OneGB)
                return $"{BytesToGB(used):N0} GB / {BytesToGB(total):N0} GB ({usedPercent:0.0}%)";
            else
                return $"{BytesToMB(used):N0} MB / {BytesToMB(total):N0} MB ({usedPercent:0.0}%)";
        }

        private static double BytesToMB(ulong bytes) => bytes / 1024.0 / 1024.0;

        private static double BytesToGB(ulong bytes) => bytes / 1024.0 / 1024.0 / 1024.0;
    }
}