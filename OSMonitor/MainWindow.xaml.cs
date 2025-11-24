using OSMonitor.Models;
using OSMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace OSMonitor
{
    public partial class MainWindow : Window
    {
        private CpuReader cpuReader;
        private GpuReader gpuReader;
        private RamReader ramReader;
        private PageFaultReader pageFaultReader;
        private ProcessInfoReader processInfoReader;
        private CancellationTokenSource? pollingCts;
        private CancellationTokenSource? pageFaultCts;
        public ObservableCollection<ProcessInfo> ProcessList { get; set; } = new ObservableCollection<ProcessInfo>();
        private List<LogEntry> history = new();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            cpuReader = new CpuReader();
            gpuReader = new GpuReader();
            ramReader = new RamReader();
            pageFaultReader = new PageFaultReader();
            processInfoReader = new ProcessInfoReader();

            TxtCpu.Text = TxtGpu.Text = TxtRam.Text = "?";

            TxtGpuName.Text = gpuReader.GetName();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (pollingCts != null) return;

            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;

            pollingCts = new CancellationTokenSource();

            int pollingMs = GetSelectedPollingMs();
            await Task.Run(() => PollLoopAsync(pollingMs, pollingCts.Token));
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            pollingCts?.Cancel();
            pollingCts = null;

            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }

        private async void BtnPfStart_Click(object sender, RoutedEventArgs e)
        {
            if (pageFaultCts != null) return;

            BtnPfStart.IsEnabled = false;
            BtnPfStop.IsEnabled = true;

            pageFaultCts = new CancellationTokenSource();

            var t1 = Task.Run(() => PageFaultMonitorLoopAsync(pageFaultCts.Token));
            var t2 = Task.Run(() => ProcessPageFaultsLoopAsync(pageFaultCts.Token));

            await Task.WhenAll(t1, t2);
        }

        private void BtnPfStop_Click(object sender, RoutedEventArgs e)
        {
            pageFaultCts?.Cancel();
            pageFaultCts = null;

            pageFaultReader.Stop();

            BtnPfStart.IsEnabled = true;
            BtnPfStop.IsEnabled = false;
        }

        private async Task PageFaultMonitorLoopAsync(CancellationToken token)
        {
            try
            {
                pageFaultReader.Start();

                while (!token.IsCancellationRequested)
                {
                    long faults = pageFaultReader.GetAndResetFaults();
                    string log = $"{DateTime.Now:HH:mm:ss} | Page Faults: {faults} F/s";

                    Dispatcher.Invoke(() =>
                    {
                        TxtPageFaults.Text = $"{faults}";
                        TxtPageFaultsLog.Text = log + "\n" + TxtPageFaultsLog.Text;
                    });

                    await Task.Delay(1000);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Erro no loop de page faults: {ex.Message}"));
            }
        }

        private async Task ProcessPageFaultsLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var data = processInfoReader.GetProcessesPageFaults();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProcessList.Clear();
                        foreach (var p in data)
                            ProcessList.Add(p);
                    });

                    await Task.Delay(10);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Erro no loop de page faults: {ex.Message}"));
            }
        }

        private async Task PollLoopAsync(int pollingMs, CancellationToken token)
        {
            try
            {            
                while (!token.IsCancellationRequested)
                {
                    var cpu = cpuReader.GetUsage();
                    var gpuUsage = gpuReader.GetUsage();
                    var ramUsage = ramReader.GetReadableUsage();
                    var timestamp = DateTime.Now;

                    var entry = new LogEntry
                    { Timestamp = timestamp, Cpu = cpu, Gpu = gpuUsage, RamFormatted = ramUsage };

                    history.Add(entry);

                    Dispatcher.Invoke(() =>
                                {
                                    TxtCpu.Text = entry.CpuFormatted;
                                    TxtGpu.Text = entry.GpuFormatted;
                                    TxtRam.Text = entry.RamFormatted;
                                    TxtLog.Text = entry.AsLogLine + "\n" + TxtLog.Text;
                                });

                    await Task.Delay(pollingMs, token);
                }
            }
            catch (OperationCanceledException) { }
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