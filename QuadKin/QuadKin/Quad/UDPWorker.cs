using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuadKin.Quad
{
    public abstract class UDPWorker : BackgroundWorker
    {

        protected abstract void doWork();

        public void StartWorkerThread()
        {
            DoWork += threadLoop;
            RunWorkerAsync();
        }

        private void threadLoop(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(15);
                doWork();
            }
        }
    }
}