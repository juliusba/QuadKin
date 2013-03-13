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

        private bool land = false;
        private bool takeOff = false;
        private bool initNav = false;
        private bool initVideo = false;

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
            takeOff = true;
        }

        public void Land()
        {
            land = true;
        }

        public void sendNullCommand()
        {
            sendUpdatedValues(0, 0, 0, 0, 0);
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
            initNav = true;
        }

        public void InitVideoData()
        {
            initVideo = true;
        }

        protected override void doWork()
        {
            if (land)
            {
                sendCommand("AT*REF=" + seqNr + ",290717696");
                land = false;
                takeOff = false;
            }
            else if (takeOff)
            {
                sendCommand("AT*REF=" + seqNr + ",290718208");
                takeOff = false;
            }
            else if (initNav)
            {
                sendCommand("AT*CONFIG=" + seqNr + ",\"general:navdata_demo\",\"TRUE\"");
                initNav = false;
            }
            else if (initVideo)
            {
                sendCommand("AT*CONFIG=" + seqNr + ",\"general:video_enable\",\"TRUE\"");
                initVideo = false;
            }
            else
            {
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
                        if (sw.ElapsedMilliseconds > 200 && sw.ElapsedMilliseconds <= 2000)
                        {
                            sendCommand("AT*PCMD=" + seqNr + "," + 0 + "," + 0 + "," + 0 + "," + 0 + "," + 0);
                        }
                        else if (sw.ElapsedMilliseconds > 2000)
                        {
                            sendCommand("AT*REF=" + seqNr + ",290717696");
                        }
                    }
                    else
                    {
                        sw.Restart();
                    }
                }
            }
            Thread.Sleep(25);
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
