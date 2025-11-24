using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Threading;

namespace OSMonitor.Services
{
    public class PageFaultReader
    {
        private TraceEventSession? session;
        private long hardFaults = 0;
        private long softFaults = 0;

        public void Start()
        {
            session = new TraceEventSession("PageFaultSession")
            {
                StopOnDispose = true
            };

            session.EnableKernelProvider(KernelTraceEventParser.Keywords.Memory); // eventos de memória do kernel

            session.Source.Kernel.MemoryHardFault += _ => hardFaults++;
            session.Source.Kernel.MemoryTransitionFault += _ => softFaults++;
            session.Source.Kernel.MemoryDemandZeroFault += _ => softFaults++;

            Thread etwThread = new Thread(() => session.Source.Process())
            {
                IsBackground = true
            };

            etwThread.Start();
        }

        public long GetAndResetFaults()
        {
            var v = hardFaults + softFaults;
            hardFaults = 0;
            softFaults = 0;
            return v;
        }

        public void Stop()
        {
            session?.Dispose();
            session = null;
        }
    }
}
