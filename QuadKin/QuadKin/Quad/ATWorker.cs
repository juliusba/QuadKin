using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuadKin.Quad
{
    public class ATWorker : UDPWorker
    {
        private int _seqNr = 0;
        private int seqNr
        {
            get
            {
                return _seqNr++;
            }
            set
            {
                _seqNr = value;
            }
        }
        private Socket socket_at;

        private bool valuesUpdated;
        private float pitch;
        private float roll;
        private float gaz;
        private float yaw;

        private Stopwatch sw = new Stopwatch();

        public bool Init(IPAddress ipAdd, int PORT_AT)
        {
            socket_at = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_at.Connect(ipAdd, PORT_AT);

            if (socket_at.Connected)
            {
                StartWorkerThread();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TakeOff()
        {
            sendCommand("AT*REF=" + seqNr + ",290718208");
        }

        public void Land()
        {
            sendCommand("AT*REF=" + seqNr + ",290717696");
        }

        public void sendNullCommand()
        {
            sendCommand("AT*PCMD=" + seqNr + "," + 1 + "," + 0 + "," + 0 + "," + 0 + "," + 0);
        }

        public void sendUpdatedValues(int enable, float pitch, float roll, float gaz, float yaw)
        {
            this.pitch = pitch;
            this.roll = roll;
            this.gaz = gaz;
            this.yaw = yaw;
            this.valuesUpdated = true;
        }

        public void InitNavData()
        {
            sendCommand("AT*CONFIG=" + seqNr + ",\"general:navdata_demo\",\"TRUE\"");
        }

        public void InitVideoData()
        {
            sendCommand("AT*CONFIG=" + seqNr + ",\"general:video_enable\",\"TRUE\"");
        }

        protected override void doWork()
        {
            Thread.Sleep(25);
            if (valuesUpdated)
            {
                sendCommand("AT*PCMD=" + seqNr + "," + 1 + "," + intOfFloat(pitch) + "," + intOfFloat(roll)
                    + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));

                if (sw.IsRunning)
                    sw.Stop();
            }
            else
            {
                if (sw.IsRunning)
                {
                    if (sw.ElapsedMilliseconds > 200)
                    {
                        sendCommand("AT*PCMD=" + seqNr + "," + 0 + "," + 0 + "," + 0 + "," + 0 + "," + 0);
                    }
                }
                else
                {
                    sw.Restart();
                }
            }
        }

        private bool sendCommand(string command)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(command + "\r");
            return socket_at.Send(buffer) == buffer.Count();
        }

        private int intOfFloat(float f)
        {
            var bytes = new byte[4];
            var floatArray = new float[] { f };
            Buffer.BlockCopy(floatArray, 0, bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
