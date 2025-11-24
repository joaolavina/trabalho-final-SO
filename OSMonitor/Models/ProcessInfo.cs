using System;
using System.ComponentModel;

namespace OSMonitor.Models
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        private int id;
        private string? processName;
        private float pageFaults;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string? ProcessName
        {
            get => processName;
            set { processName = value; OnPropertyChanged(nameof(ProcessName)); }
        }

        public float PageFaults
        {
            get => pageFaults;
            set { pageFaults = value; OnPropertyChanged(nameof(PageFaults)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
