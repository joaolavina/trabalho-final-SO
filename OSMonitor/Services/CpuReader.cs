using System.Runtime.InteropServices;

namespace OSMonitor.Services
{
    public class CpuReader
    {
        [StructLayout(LayoutKind.Sequential)]
        struct FILETIME
        {
            public uint Low;
            public uint High;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(out FILETIME idle, out FILETIME kernel, out FILETIME user);

        private ulong oldIdle;
        private ulong oldKernel;
        private ulong oldUser;

        public float GetUsage()
        {
            GetSystemTimes(out var idle, out var kernel, out var user);

            ulong idleTime = ((ulong)idle.High << 32) | idle.Low; // Tempo ocioso da CPU
            ulong kernelTime = ((ulong)kernel.High << 32) | kernel.Low; // Tempo rodando instruções em modo kernel
            ulong userTime = ((ulong)user.High << 32) | user.Low; // Tempo rodando instruções em modo usuário

            ulong idleDiff = idleTime - oldIdle;
            ulong kernelDiff = kernelTime - oldKernel;
            ulong userDiff = userTime - oldUser;

            oldIdle = idleTime;
            oldKernel = kernelTime;
            oldUser = userTime;

            ulong total = kernelDiff + userDiff; // Tempo que não é ocioso desde a última leitura 

            if (total == 0)
                return 0;

            return (float)(100.0 - (idleDiff * 100.0 / total));
        }
    }
}