using System.Diagnostics;

namespace MonitorGpu.Services
{
    public class FpsCounter
    {
        private Stopwatch sw = new Stopwatch();
        private int frames = 0;
        private double fps = 0;

        public void Start()
        {
            sw.Restart();
            frames = 0;
        }

        public void Frame()
        {
            frames++;
            if (sw.ElapsedMilliseconds >= 1000)
            {
                fps = frames / (sw.ElapsedMilliseconds / 1000.0);
                frames = 0;
                sw.Restart();
            }
        }

        public double GetFps() => fps;
    }
}