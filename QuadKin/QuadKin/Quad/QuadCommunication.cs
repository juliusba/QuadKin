using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace QuadKin.Quad
{
    class QuadCommunication
    {
        static readonly int PORT_NAVDATA = 5554;
        static readonly int PORT_VIDEO = 5555;
        static readonly int PORT_AT = 5556;

        static readonly string DRONE_IP_ADRESS = "192.168.1.1";
        static readonly IPAddress ipAdd = IPAddress.Parse(DRONE_IP_ADRESS);
        //static const string DRONE_NETWORKNAME = "ardrone_";

        private Thread worker_at;
        private Thread worker_navData;
        private Thread worker_video;

        Socket socket_at;
        Socket socket_navData;
        Socket socket_video;

        private Stopwatch sw;

        int seq = 1;

        private byte[] buffer_navData = new byte[10240];
        private byte[] buffer_video = new byte[64000];

        public QuadCommunication()
        {
            if (initConnection())
            {
                startWorkers();
            }
            else
                Console.WriteLine();
        }

        public void takeOff()
        {
            sw = new Stopwatch();
            sw.Start();
            sendCommand("AT*REF=" + getSeq() + ",290718208");
            while (sw.ElapsedMilliseconds < 5000)
            {
                //sendCommand("AT*REF=" + getSeq() + ",290718208");
                sendUpdatedValues(1, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        }

        private bool initConnection()
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };

            socket_at = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_at.Connect(ipAdd, PORT_AT);
            if (!socket_at.Connected) return false;

            socket_navData = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_navData.Connect(ipAdd, PORT_NAVDATA);
            if (!socket_navData.Connected) return false;
            else
            {
                socket_navData.Send(buffer);
                sendCommand("AT*CONFIG=" + getSeq() + ",\"general:navdata_demo\",\"TRUE\"");
            }

            socket_video = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_video.Connect(ipAdd, PORT_VIDEO);
            if (!socket_video.Connected) return false;
            else
            {
                socket_video.Send(buffer);
                sendCommand("AT*CONFIG=" + getSeq() + ",\"general:video_enable\",\"TRUE\"");
            }

            return true;
        }

        private void startWorkers()
        {
            worker_at = new Thread(() =>
            {
                while (true)
                {
                    int last_seq = seq;
                    Thread.Sleep(1500);
                    if (seq == last_seq)
                    {
                        sendCommand("AT*REF=" + getSeq() + ",290717696");
                    }
                }
            });
            worker_at.Start();

            worker_navData = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        socket_navData.Receive(buffer_navData);
                        Console.WriteLine("NavData Received: " + buffer_navData.Count() + " bytes");
                        Console.WriteLine("navData: " + buffer_navData[4567]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            //worker_navData.Start();
            
            worker_video = new Thread(() =>
            {
                while (true)
                {
                    try {
		                socket_video.Receive(buffer_video);
		                Console.WriteLine("Video Received: " + buffer_video.Count() + " bytes");
                        Console.WriteLine("video: " + buffer_video[4567]);
		            } catch(Exception e) {
	    	                Console.WriteLine(e.Message);
		            }
                }
            });
            //worker_video.Start();
        }

        private void sendCommand(string command)
        {
            byte[] buffer = Encoding.ASCII.GetBytes (command + "\r");
            if(socket_at.Send(buffer) == buffer.Count())
                Console.WriteLine();
        }

        public void sendUpdatedValues(int enable, float pitch, float roll, float gaz, float yaw) {
	        sendCommand("AT*PCMD=" + getSeq() + "," + enable + "," + intOfFloat(pitch) + "," + intOfFloat(roll) 
            + "," + intOfFloat(gaz) + "," + intOfFloat(yaw));
        }

        private int intOfFloat(float f)
        {
            var bytes = new byte[4];
            var floatArray = new float[] { f };
            Buffer.BlockCopy(floatArray, 0, bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        private int getSeq()
        {
            return seq++;
        }

        internal void sendUpdatedValues(Command c)
        {
            if (c.valid)
            {
                sendUpdatedValues(1, c.FB, c.RL, c.UD, c.TRL);
            }
            else
            {
                sendCommand("AT*REF=" + getSeq() + ",290717696");
            }
        }
    }
}
