using System.Diagnostics;

public class CpuReader
{
    private PerformanceCounter cpuCounter;

    public CpuReader()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    }

    public float GetCpuUsage()
    {
        return cpuCounter.NextValue();
    }
}