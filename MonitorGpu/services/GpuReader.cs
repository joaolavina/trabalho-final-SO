using System.Management;

public class GpuReader
{
    public float GetUsage()
    {
        float usage = 0.0f;
        var searcher = new ManagementObjectSearcher("root\\CIMV2",
            "SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine"); // whats 2

        foreach (ManagementObject obj in searcher.Get())
        {
            string name = obj["Name"]?.ToString() ?? ""; // whats
            if (name.Contains("engtype_3D")) // foca em engine 3D POR QUE ?
            {
                usage += Convert.ToSingle(obj["UtilizationPercentage"]);
            }
        }
        return usage; // 0-100%
    }
}