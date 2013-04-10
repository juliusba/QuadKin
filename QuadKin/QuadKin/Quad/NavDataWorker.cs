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
    public class NavData
    {
        private static readonly int navdata_state = 20;
        private static readonly int navdata_battery = 24;
        private static readonly int navdata_pitch = 28;
        private static readonly int navdata_roll = 32;
        private static readonly int navdata_yaw = 36;
        private static readonly int navdata_altitude = 40;
        private static readonly int navdata_vx = 44;
        private static readonly int navdata_vy = 48;
        private static readonly int navdata_vz = 52;
        
        public int state { get; private set; }
        public int battery { get; private set; }
        public int pitch { get; private set; }
        public int roll { get; private set; }
        public int yaw { get; private set; }
        public int altitude { get; private set; }
        public int vx { get; private set; }
        public int vy { get; private set; }
        public int vz { get; private set; }

        public NavData(byte[] buffer_navData)
        {
            this.state = get_int_from_packet(buffer_navData, navdata_state);
            this.battery = get_int_from_packet(buffer_navData, navdata_battery);
            this.pitch = get_int_from_packet(buffer_navData, navdata_pitch);
            this.roll = get_int_from_packet(buffer_navData, navdata_roll);
            this.yaw = get_int_from_packet(buffer_navData, navdata_yaw);
            this.altitude = get_int_from_packet(buffer_navData, navdata_altitude);
            this.vx = get_int_from_packet(buffer_navData, navdata_vx);
            this.vy = get_int_from_packet(buffer_navData, navdata_vy);
            this.vz = get_int_from_packet(buffer_navData, navdata_vz);
        }

        //Return int value from udp packet sent from quadcopter
        private int get_int_from_packet(Byte[] data, int offset)
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

    public class NavDataWorker : UDPWorker
    {

        private UdpClient socket_navData;
        private IPEndPoint ipEP;
        private byte[] buffer;        

        public delegate void NavDataHandler(NavData data);
        public event NavDataHandler NavDataReady;

        internal bool Init(IPAddress ipAdd, string ipAddress, int PORT_NAVDATA, ATWorker atWorker)
        {
            this.ipEP = new IPEndPoint(ipAdd, PORT_NAVDATA);
            
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };
            socket_navData = new UdpClient(ipAddress, PORT_NAVDATA);

            socket_navData.Send(buffer, buffer.Length);
            atWorker.InitNavData();
            
            StartWorkerThread();
            return true;
        }

        protected override void doWork()
        {
            try
            {
                buffer = socket_navData.Receive(ref ipEP);
                
                NavData data = new NavData(buffer);
                
                if(NavDataReady != null)
                    NavDataReady(data);

                Console.WriteLine("Battery: " + data.battery + "%");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
