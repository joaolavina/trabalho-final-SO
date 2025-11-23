using System.Management;

namespace MonitorGpu.Services
{
    public class GpuReader
    {
        public float GetUsage()
        {
            float usage = 0.0f;
            var searcher = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine"); // select no Windows Management Instrumentation

            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["UtilizationPercentage"] != null)
                    usage += Convert.ToSingle(obj["UtilizationPercentage"]);
            }

            return usage;
        }
    }
}