namespace OSMonitor.Models
{
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public float Cpu { get; set; }
    public float Gpu { get; set; }
    public string? RamFormatted { get; set; }
    public string CpuFormatted => $"{Cpu:0.0}%";
    public string GpuFormatted => $"{Gpu:0.0}%";
    public string TimestampFormatted => $"{Timestamp:HH:mm:ss}";

    public string AsLogLine =>
        $"{TimestampFormatted} | CPU {CpuFormatted} | GPU {GpuFormatted} | RAM {RamFormatted}";
    }
}