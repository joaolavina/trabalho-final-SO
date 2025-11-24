using System;
using System.Runtime.InteropServices;

namespace OSMonitor.Services
{
    public class RamReader
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX // estrutura de memória usada pela função GlobalMemoryStatusEx
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public (ulong totalBytes, ulong availBytes, float usedPercent) GetMemoryStatus()
        {
            var memStatus = new MEMORYSTATUSEX();

            if (!GlobalMemoryStatusEx(memStatus))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()); // erro na chamada da API

            ulong total = memStatus.ullTotalPhys;
            ulong available = memStatus.ullAvailPhys;

            if (total == 0)
                return (total, available, 0.0f);    

            float usedPercent = (float)((double)(total - available) * 100.0 / total);
            return (total, available, usedPercent);
        }

        public string GetReadableUsage()
        {
            var (total, available, usedPercent) = GetMemoryStatus();
            ulong used = total - available;

            const ulong GB = 1024UL * 1024UL * 1024UL; // 1GB = 1024^3 bytes = 1.073.741.824 bytes

            if (used >= GB) 
                return $"{BytesToGB(used):N1} GB / {BytesToGB(total):N1} GB ({usedPercent:0.0}%)";
            else
                return $"{BytesToMB(used):N0} MB / {BytesToMB(total):N0} MB ({usedPercent:0.0}%)";
        }

        private static double BytesToMB(ulong bytes) => bytes / 1024.0 / 1024.0;

        private static double BytesToGB(ulong bytes) => bytes / 1024.0 / 1024.0 / 1024.0;
    }
}