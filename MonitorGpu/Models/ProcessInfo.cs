namespace MonitorGpu.Models
{
public class ProcessInfo
{
    public int Id { get; set; }
    public string? ProcessName { get; set; }
    public float PageFaults { get; set; }
}
}