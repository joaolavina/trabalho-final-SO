namespace MonitorGpu.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public double Cpu { get; set; }
        public double Gpu { get; set; }
        public ulong RamUsedBytes { get; set; }
        public ulong RamTotalBytes { get; set; }
    }
}