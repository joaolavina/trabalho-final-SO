using OSMonitor.Models;
using System;
using System.Diagnostics;
using System.Globalization;

public class GpuReader
{
    private string _nvidiaSmiCmd = "nvidia-smi";
    private string _queryFields = "name,utilization.gpu,temperature.gpu,fan.speed,clocks.gr,clocks.mem";
    private string _format = "csv,noheader,nounits";

    public string GetName()
    {
        var snap = RunQueryOnce();
        return snap?.Name ?? "Não encontrado";
    }

    public float GetUsage()
    {
        var snap = RunQueryOnce();
        if (snap?.UtilizationGpuPercent != null) return snap.UtilizationGpuPercent.Value;
        return 0f;
    }

    public int GetTemperature()
    {
        var snap = RunQueryOnce();
        return snap?.TemperatureC ?? 0;
    }
    public int GetGraphicsClock()
    {
        var snap = RunQueryOnce();
        return snap?.GraphicsClockMHz ?? 0;
    }

    public int GetMemoryClock()
    {
        var snap = RunQueryOnce();
        return snap?.MemoryClockMHz ?? 0;
    }
    private GpuSnapshot? RunQueryOnce()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _nvidiaSmiCmd,
                Arguments = $"--query-gpu={_queryFields} --format={_format}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null) return null;

            string outText = p.StandardOutput.ReadToEnd().Trim();
            string errText = p.StandardError.ReadToEnd().Trim();
            p.WaitForExit(1500);

            var firstLine = outText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)[0];

            var parts = firstLine.Split(',');
            for (int i = 0; i < parts.Length; i++) parts[i] = parts[i].Trim();

            var snap = new GpuSnapshot
            {
                Name = parts.Length > 0 ? parts[0] : "N/A"
            };

            if (parts.Length > 1 && float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float util))
                snap.UtilizationGpuPercent = util;

            if (parts.Length > 2 && int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int temp))
                snap.TemperatureC = temp;

            if (parts.Length > 4 && int.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out int gclk))
                snap.GraphicsClockMHz = gclk;

            if (parts.Length > 5 && int.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out int mclk))
                snap.MemoryClockMHz = mclk;

            return snap;
        }
        catch
        {
            return null;
        }
    }
}
