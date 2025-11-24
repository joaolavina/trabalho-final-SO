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
        public string GetName()
        {
            string gpuName = null;

            var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");

            foreach (ManagementObject mo in searcher.Get())
            {
                var n = mo["Name"]?.ToString();
                if (!string.IsNullOrEmpty(n))
                {
                    gpuName = n;
                    break;
                }
            }

            return gpuName ?? "Não encontrado";
        }
    }

}