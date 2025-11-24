using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Threading;

namespace OSMonitor.Services
{
    public class PageFaultReader
    {
        private TraceEventSession? _session;
        private long _hardFaults = 0;
        private long _softFaults = 0;

        public void Start()
        {
            _session = new TraceEventSession("PageFaultSession")
            {
                StopOnDispose = true
            };

            _session.EnableKernelProvider(KernelTraceEventParser.Keywords.Memory); // eventos de memória do kernel

            _session.Source.Kernel.MemoryHardFault += _ => _hardFaults++; // acesso ao disco
            _session.Source.Kernel.MemoryTransitionFault += _ => _softFaults++; // fora do working set do processo
            _session.Source.Kernel.MemoryDemandZeroFault += _ => _softFaults++; // página zerada

            Thread etwThread = new Thread(() => _session.Source.Process())
            {
                IsBackground = true
            };

            etwThread.Start();
        }

        public long GetAndResetFaults()
        {
            var v = _hardFaults + _softFaults;
            _hardFaults = 0;
            _softFaults = 0;
            return v;
        }

        public void Stop()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}
