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

        private bool flying = false;

        private bool valuesUpdated = false;
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
                //Send rest watchdog and land command at start up.... and hover...
                sendCommand("AT*REF=1,290717696\r");
                Thread.Sleep(15);
                sendCommand("AT*PCMD=1,0,0,0,0,0\r");
                Thread.Sleep(15);
                sendCommand("AT*COMWDG=1\r");
                Thread.Sleep(15);

                //LEDS EXAMPLE..........................................................................
                sendCommand("AT*CONFIG=2,\"leds:leds_anim\",\"3,1073741824,2\"\r");
                //LEDS EXAMPLE END......................................................................

                Thread.Sleep(1000);

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

        public void sendCommand(Command c)
        {
            this.pitch = c.FB;
            this.roll = c.RL;
            this.gaz = c.UD;
            this.yaw = c.TRL;
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
                flying = false;
                land = false;
                takeOff = false;
            }
            else if (takeOff)
            {
                sendCommand("AT*REF=" + seqNr + ",290718208");
                flying = true;
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
            else if (flying)
            {
                if (valuesUpdated)
                {
                    
                    sendCommand("AT*PCMD=" + seqNr + "," + 1 + "," + intOfFloat(pitch) + "," + intOfFloat(roll)
                        + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));
                    valuesUpdated = false;

                    if (sw.IsRunning)
                        sw.Stop();
                }
                else
                {
                    if (sw.IsRunning)
                    {
                        if (sw.ElapsedMilliseconds < 200)
                        {
                            sendCommand("AT*PCMD=" + seqNr + "," + 1 + "," + intOfFloat(pitch) + "," + intOfFloat(roll)
                                + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));
                        }
                        if (sw.ElapsedMilliseconds <= 10000)
                        {
                            sendCommand("AT*PCMD=" + seqNr + "," + 0 + "," + 0 + "," + 0 + "," + 0 + "," + 0);
                        }
                        else if (sw.ElapsedMilliseconds > 10000)
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
            else
            {
                sendCommand("AT*PCMD=" + seqNr + "," + 1 + "," + 0 + "," + 0 + "," + 0 + "," + 0);
                //sendCommand("AT*COMWDG=" + seqNr);
            }
            Thread.Sleep(25);
        }

        private bool sendCommand(string command)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(command + "\r");
            return socket_at.Send(buffer) == buffer.Count();
            if(flying) Console.WriteLine("-----------------" + command);
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
