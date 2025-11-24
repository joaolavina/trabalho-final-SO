namespace OSMonitor.Models
{
    public class GpuSnapshot
    {
        public string Name { get; set; } = "N/A";
        public float? UtilizationGpuPercent { get; set; }
        public int? TemperatureC { get; set; }
        public int? GraphicsClockMHz { get; set; }
        public int? MemoryClockMHz { get; set; }
    }
}