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
        private int _seqNr = 1;
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
        private UdpClient socket_at;

        private bool land = false;
        private bool takeOff = false;
        private bool valuesUpdated = false;
        private bool initNav = false;

        private bool flying = false;

        private int hover = 0;
        private float pitch;
        private float roll;
        private float gaz;
        private float yaw;

        private List<string> commandLines = new List<string>();

        private Stopwatch sw = new Stopwatch();

        public bool Init(IPAddress ipAdd, string ipAddress, int PORT_AT)
        {
            socket_at = new UdpClient(ipAddress, PORT_AT);

            //Send rest watchdog and land command at start up.... and hover...
            sendCommand("AT*REF=" + seqNr + ",290717696\r");
            Thread.Sleep(15);
            sendCommand("AT*PCMD=" + seqNr + ",0,0,0,0,0\r");
            Thread.Sleep(15);
            sendCommand("AT*FTRIM=" + seqNr + ",");
            Thread.Sleep(15);
            sendCommand("AT*COMWDG=" + seqNr + "\r");
            Thread.Sleep(15);

            //LEDS EXAMPLE..........................................................................
            sendCommand("AT*CONFIG=" + seqNr + ",\"leds:leds_anim\",\"3,1073741824,2\"\r");
            //LEDS EXAMPLE END......................................................................

            Thread.Sleep(1000);

            StartWorkerThread();
            return true;
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

            this.hover = (pitch == 0 && roll == 0 && gaz == 0 && yaw == 0)? 0 : 1;
        }

        public void InitNavData()
        {
            initNav = true;
        }

        protected override void doWork()
        {
            Thread.Sleep(25);
            sendCommand("AT*COMWDG=" + seqNr);
            Thread.Sleep(25);            

            if (land)
            {
                sendCommand("AT*REF=" + seqNr + ",290717696");
                flying = false;
                land = false;
                takeOff = false;
            }
            else if (takeOff)
            {
                sendCommand("AT*COMWDG=" + seqNr);
                Thread.Sleep(50);
                sendCommand("AT*FTRIM=" + seqNr + ",");
                Thread.Sleep(50);
                sendCommand("AT*REF=" + seqNr + ",290718208");
                Thread.Sleep(50);
                sendCommand("AT*COMWDG=" + seqNr);
                flying = true;
                takeOff = false;
            }
            else if (initNav)
            {
                sendCommand("AT*CONFIG=" + seqNr + ",\"general:navdata_demo\",\"TRUE\"");

                Thread.Sleep(40);
                sendCommand("AT*CTRL=0\r");

                initNav = false;
            }
            else if (flying)
            {
                if (valuesUpdated)
                {
                    
                    sendCommand("AT*PCMD=" + seqNr + "," + this.hover + "," + intOfFloat(roll) + "," + intOfFloat(pitch)
                        + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));
                    commandLines.Add("pit: " + pitch + "\troll: " + roll + "\tgaz: " + gaz + "\tyaw: " + yaw);
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
                            sendCommand("AT*PCMD=" + seqNr + "," + this.hover + "," + intOfFloat(pitch) + "," + intOfFloat(roll)
                                + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));
                            commandLines.Add("pitch: " + pitch + "\troll: " + roll + "\tgaz: " + gaz + "\tyaw: " + yaw);
                        }
                        else if (sw.ElapsedMilliseconds <= 10000)
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
        }

        private bool sendCommand(string command)
        {
            command += "\r";
            byte[] buffer = Encoding.ASCII.GetBytes(command);
            commandLines.Add(command + " \t\t" + DateTime.Now.ToString("mm:ss tt"));
            writeCommandsToFile();
            return socket_at.Send(buffer, buffer.Length) == buffer.Count();
        }

        private int intOfFloat(float f)
        {
            if (f == 0.0)
                return 0;
            var bytes = new byte[4];
            var floatArray = new float[] { f };
            Buffer.BlockCopy(floatArray, 0, bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        internal void writeCommandsToFile()
        {
            System.IO.File.WriteAllLines(@"C:\Users\juliusbuset\EIT\commandLines.txt", commandLines);
        }
    }
}
