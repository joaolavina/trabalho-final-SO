using MonitorGpu.Models;
using MonitorGpu.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace MonitorGpu
{
    public partial class MainWindow : Window
    {
        private CpuReader cpuReader;
        private GpuReader gpuReader;
        private RamReader ramReader;
        private FpsCounter fpsCounter;

        private CancellationTokenSource? pollingCts;
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
                    var gpuUsage = gpuReader.GetUsage();
                    var (total, avail, usedPercent) = ramReader.GetMemoryStatus();
                    var timestamp = DateTime.Now;

                    var log = $"{timestamp:HH:mm:ss} | CPU {cpu:0.0}% | GPU {gpuUsage:0.0}% | RAM {(total - avail) / 1024.0 / 1024.0:N0}MB/{total / 1024.0 / 1024.0:N0}MB ({usedPercent:0.0}%)";
                    history.Add(new LogEntry { Timestamp = timestamp, Cpu = cpu, Gpu = gpuUsage, RamUsedBytes = total - avail, RamTotalBytes = total });

                    Dispatcher.Invoke(() =>
                    {
                        TxtCpu.Text = $"{cpu:0.0}%";
                        TxtGpu.Text = $"{gpuUsage:0.0}%";
                        TxtRam.Text = $"{(total - avail) / 1024.0 / 1024.0:N0} MB / {total / 1024.0 / 1024.0:N0} MB ({usedPercent:0.0}%)";
                        TxtGpuName.Text = "Não encontrado";
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
        private int GetSelectedPollingMs()
        {
            if (CbPolling.SelectedItem is System.Windows.Controls.ComboBoxItem it && int.TryParse(it.Content.ToString(), out int v))
                return Math.Max(50, v);
            return 500;
        }
    }
}