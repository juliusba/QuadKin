using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace QuadKin.Quad
{
    public class QuadCom : StateClass
    {
        static readonly int PORT_NAVDATA = 5554;
        static readonly int PORT_VIDEO = 5555;
        static readonly int PORT_AT = 5556;

        static readonly string DRONE_IP_ADRESS = "192.168.1.1";
        static readonly IPAddress ipAdd = IPAddress.Parse(DRONE_IP_ADRESS);
        //static const string DRONE_NETWORKNAME = "ardrone_";


        private Stopwatch sw = new Stopwatch();

        private ATWorker atWorker = new ATWorker();
        public NavDataWorker navDataWorker = new NavDataWorker();
        public VideoDataWorker videoDataWorker = new VideoDataWorker();

        private static QuadCom quadCom;
        private static object syncRoot = new Object();

        private QuadCom()
        {
            Init();
        }

        public static QuadCom instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (quadCom == null)
                    {
                        quadCom = new QuadCom();
                    }
                    return quadCom;
                }
            }
        }

        public void Init()
        {
            if (!atWorker.Init(ipAdd, DRONE_IP_ADRESS, PORT_AT))
            {
                State = State.NoConnection;
                return;
            }


            if (!navDataWorker.Init(ipAdd, DRONE_IP_ADRESS, PORT_NAVDATA, atWorker))
            {
                State = State.NoConnection;
                return;
            }

            if (!videoDataWorker.Init(ipAdd, DRONE_IP_ADRESS, PORT_VIDEO))
            {
                State = State.NoConnection;
                return;
            }

            State = State.Ready;
        }

        public static void Stop()
        {
            quadCom.atWorker.CancelAsync();
            quadCom.navDataWorker.CancelAsync();
            quadCom.videoDataWorker.CancelAsync();
            quadCom = null;
        }

        public void TakeOff()
        {
            atWorker.TakeOff();
        }

        public void Land()
        {
            atWorker.Land();
        }

        public void SendCommand(Command c)
        {
            atWorker.sendCommand(c);
        }
    }
}