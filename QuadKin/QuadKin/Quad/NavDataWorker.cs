using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

//Gaute

namespace QuadKin.Quad
{
    public class NavDataWorker : UDPWorker
    {
        private byte[] buffer_navData = new byte[10240];        
        private Socket socket_navData;

        public delegate void ByteArrayHandler(byte[] data);
        public event ByteArrayHandler NavDataReady;

        public bool Init(IPAddress ipAdd, int PORT_NAVDATA, ATWorker atWorker)
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };
            socket_navData = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_navData.Connect(ipAdd, PORT_NAVDATA);

            if (socket_navData.Connected)
            {
                socket_navData.Send(buffer);
                atWorker.InitNavData();
                StartWorkerThread();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void doWork()
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

        //Return int value from udp packet sent from quadcopter
        public static int get_int_from_packet(Byte[] data, int offset)
        {
            if (data.Length < offset + 3) return 0; //Test if there is actually something to read where you want to read
            int temp = 0; int n = 0;
            for (int i = 3; i >= 0; i--)
            {
                n <<= 8;
                temp = data[offset + i] & 0xFF;
                n |= temp;
            }
            return n;
        }
    }
}
