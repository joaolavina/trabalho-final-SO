using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management; // pode precisar de `dotnet add package System.Management`
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MonitorGpu
{
    public partial class MainWindow : Window
    {
        // leitores/serviços simples
        private CpuReader cpuReader;
        private GpuReader gpuReader;
        private RamReader ramReader;
        private FpsCounter fpsCounter;

        private CancellationTokenSource pollingCts;
        private List<LogEntry> history = new();

        public MainWindow()
        {
            InitializeComponent();

            cpuReader = new CpuReader();
            gpuReader = new GpuReader();
            ramReader = new RamReader();
            fpsCounter = new FpsCounter();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            TxtFps.Text = "0.0";
            TxtCpu.Text = TxtGpu.Text = TxtRam.Text = TxtGpuName.Text = "—";
            TxtLog.Text = "Pronto.\n";
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            fpsCounter.Frame();
            TxtFps.Text = $"{fpsCounter.GetFps():0.0}";
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (pollingCts != null) return;
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            int pollingMs = GetSelectedPollingMs();
            pollingCts = new CancellationTokenSource();
            await Task.Run(() => PollLoopAsync(pollingMs, pollingCts.Token));
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            pollingCts?.Cancel();
            pollingCts = null;
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }

        private async Task PollLoopAsync(int pollingMs, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var cpu = cpuReader.GetUsage();
                    var (gpuUsage, gpuName) = gpuReader.GetUsageAndName();
                    var (total, avail, usedPercent) = ramReader.GetMemoryStatus();
                    var timestamp = DateTime.Now;

                    var log = $"{timestamp:HH:mm:ss} | CPU {cpu:0.0}% | GPU {gpuUsage:0.0}% | RAM {(total - avail) / 1024.0 / 1024.0:N0}MB/{total / 1024.0 / 1024.0:N0}MB ({usedPercent:0.0}%)";
                    history.Add(new LogEntry { Timestamp = timestamp, Cpu = cpu, Gpu = gpuUsage, RamUsedBytes = total - avail, RamTotalBytes = total });

                    Dispatcher.Invoke(() =>
                    {
                        TxtCpu.Text = $"{cpu:0.0}%";
                        TxtGpu.Text = $"{gpuUsage:0.0}%";
                        TxtRam.Text = $"{(total - avail) / 1024.0 / 1024.0:N0} MB / {total / 1024.0 / 1024.0:N0} MB ({usedPercent:0.0}%)";
                        TxtGpuName.Text = gpuName ?? "Desconhecido";
                        TxtLog.Text = log + "\n" + TxtLog.Text;
                    });

                    await Task.Delay(pollingMs, token);
                }
            }
            catch (OperationCanceledException) { /* normal */ }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Erro no loop de polling: {ex.Message}"));
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "gpu_monitor_log.csv");
                var sb = new StringBuilder();
                sb.AppendLine("Timestamp;CPU;GPU;RamUsedMB;RamTotalMB");
                foreach (var r in history)
                {
                    sb.AppendLine($"{r.Timestamp:O};{r.Cpu:0.0};{r.Gpu:0.0};{r.RamUsedBytes/1024.0/1024.0:N0};{r.RamTotalBytes/1024.0/1024.0:N0}");
                }
                System.IO.File.WriteAllText(path, sb.ToString());
                MessageBox.Show($"CSV exportado para: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro exportando CSV: {ex.Message}");
            }
        }

        private int GetSelectedPollingMs()
        {
            if (CbPolling.SelectedItem is System.Windows.Controls.ComboBoxItem it && int.TryParse(it.Content.ToString(), out int v))
                return Math.Max(50, v);
            return 500;
        }

        // ----- classes auxiliares / leitores simples (implementações básicas) -----

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public double Cpu { get; set; }
            public double Gpu { get; set; }
            public ulong RamUsedBytes { get; set; }
            public ulong RamTotalBytes { get; set; }
        }

        // CPU via PerformanceCounter (Total)
        private class CpuReader
        {
            private PerformanceCounter cpuCounter;

            public CpuReader()
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                // primeira leitura costuma retornar 0, então chama uma vez.
                _ = cpuCounter.NextValue();
            }

            public double GetUsage()
            {
                try
                {
                    return Math.Round(cpuCounter.NextValue(), 1);
                }
                catch
                {
                    return 0;
                }
            }
        }

        // GPU via WMI: tenta somar engines 3D (se disponíveis) e obter nome da GPU
        private class GpuReader
        {
            public (double usagePercent, string? gpuName) GetUsageAndName()
            {
                double usage = 0;
                string? gpuName = null;

                try
                {
                    // Nome via Win32_VideoController (pode retornar múltiplas GPUs)
                    try
                    {
                        using var searcherName = new ManagementObjectSearcher("select Name from Win32_VideoController");
                        foreach (ManagementObject mo in searcherName.Get())
                        {
                            var n = mo["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(n))
                            {
                                gpuName = n;
                                break;
                            }
                        }
                    }
                    catch { /* ignora se falhar */ }

                    // Uso via GPUEngine (Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine)
                    try
                    {
                        using var searcher = new ManagementObjectSearcher("root\\CIMV2",
                            "SELECT Name,UtilizationPercentage FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var name = (obj["Name"] as string) ?? "";
                            var valObj = obj["UtilizationPercentage"];
                            if (valObj == null) continue;
                            if (!double.TryParse(valObj.ToString(), out double val)) continue;

                            // filtra engines 3D (nome costuma conter "engtype_3D") OR sumariza tudo
                            if (name.IndexOf("engtype_3D", StringComparison.OrdinalIgnoreCase) >= 0)
                                usage += val;
                            else if (usage == 0) // fallback: some systems report differently — soma tudo como fallback
                                usage += val;
                        }
                    }
                    catch
                    {
                        // fallback: quando WMI não fornece GPUEngine, podemos tentar PerformanceCounter (categoria "GPU Engine")
                        try
                        {
                            // opcional: implementar PerformanceCounter fallback se necessário
                        }
                        catch { }
                    }
                }
                catch
                {
                    // em caso de qualquer erro, retorno 0 e nome nulo
                }

                // cap em 0-100
                if (usage < 0) usage = 0;
                if (usage > 100) usage = 100;
                return (usage, gpuName);
            }
        }

        // RAM via GlobalMemoryStatusEx (P/Invoke)
        private class RamReader
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private class MEMORYSTATUSEX
            {
                public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

            public (ulong totalBytes, ulong availBytes, float usedPercent) GetMemoryStatus()
            {
                var m = new MEMORYSTATUSEX();
                if (!GlobalMemoryStatusEx(m))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                ulong total = m.ullTotalPhys;
                ulong avail = m.ullAvailPhys;
                float usedPercent = (float)((double)(total - avail) * 100.0 / total);
                return (total, avail, usedPercent);
            }
        }

        // Contador de FPS simples (conta frames via CompositionTarget.Rendering)
        private class FpsCounter
        {
            private Stopwatch sw = Stopwatch.StartNew();
            private int frames = 0;
            private double lastFps = 0;

            public void Frame()
            {
                frames++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    lastFps = frames / (sw.ElapsedMilliseconds / 1000.0);
                    frames = 0;
                    sw.Restart();
                }
            }

            public double GetFps() => lastFps;
        }
    }
}
