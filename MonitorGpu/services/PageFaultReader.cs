using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Threading;

namespace MonitorGpu.Services
{
    public class PageFaultReader
    {
        private TraceEventSession? session;
        private long hardFaults = 0;

        public void Start()
        {
            if (session != null) return;

            session = new TraceEventSession("PageFaultSession")
            {
                StopOnDispose = true
            };

            session.EnableKernelProvider(KernelTraceEventParser.Keywords.Memory);

            session.Source.Kernel.All += evt =>
            {
                if (evt.TaskName == "PageFault" &&
                    evt.EventName == "PageFault")
                {
                    hardFaults++;
                }
            };

            Thread etwThread = new Thread(() => session.Source.Process());
            etwThread.IsBackground = true;
            etwThread.Start();
        }

        public long GetAndResetFaults()
        {
            var v = hardFaults;
            hardFaults = 0;
            return v;
        }

        public void Stop()
        {
            session?.Dispose();
            session = null;
        }
    }
}
