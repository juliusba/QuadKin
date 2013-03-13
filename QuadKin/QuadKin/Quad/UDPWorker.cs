using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuadKin.Quad
{
    public abstract class UDPWorker
    {
        protected Thread workerThread;

        protected abstract void doWork();

        public void StartWorkerThread()
        {
            workerThread = new Thread(new ThreadStart(threadLoop));
            workerThread.Name = this.GetType().ToString() + "_WorkerThread";
            workerThread.Start();
        }

        private void threadLoop()
        {
            while (true)
            {
                Thread.Sleep(20);
                doWork();
            }
        }
    }
}