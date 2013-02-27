using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

        public QuadCommunication()
        {
            if (initConnection())
            {
                sendCommand("AT* noe...");
            }
            //startWorkers();

        }

        private bool initConnection()
        {
            socket_at = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_at.Connect(ipAdd, PORT_AT);
            if (!socket_at.Connected) return false;

            //socket_navData = new Socket(SocketType.Dgram, ProtocolType.IP);
            //socket_navData.Connect(ipAdd, PORT_NAVDATA);
            //if (!socket_navData.Connected) return false;

            //socket_video = new Socket(SocketType.Dgram, ProtocolType.IP);
            //socket_video.Connect(ipAdd, PORT_VIDEO);
            //if (!socket_video.Connected) return false;

            return true;
        }

        private void startWorkers()
        {
            worker_at = new Thread(() =>
            {
                
            });
            worker_at.Start();

            worker_navData = new Thread(() =>
            {
                //while (true)
                //{

                //}
            });
            worker_navData.Start();

            worker_video = new Thread(() =>
            {
                //while (true)
                //{

                //}
            });
            worker_video.Start();

        }

        private void sendCommand(string command)
        {
            byte[] buffer = Encoding.ASCII.GetBytes (command + "\r");
            socket_at.Send(buffer);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
