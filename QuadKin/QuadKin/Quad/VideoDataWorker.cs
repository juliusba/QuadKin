using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuadKin.Quad
{
    public class VideoDataWorker : UDPWorker
    {
        private byte[] buffer_video = new byte[64000];
        private Socket socket_video;
        private VideoUtils videoUtils = new VideoUtils();

        public delegate void WritableBitmapHandler(WriteableBitmap wbm);
        public event WritableBitmapHandler VideoBitmapReady;

        public bool Init(IPAddress ipAdd, int PORT_VIDEO, ATWorker atWorker)
        {
            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };
            socket_video = new Socket(SocketType.Dgram, ProtocolType.IP);
            socket_video.Connect(ipAdd, PORT_VIDEO);

            if (socket_video.Connected && socket_video.Send(buffer) > 0)
            {
                atWorker.InitVideoData();
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
                socket_video.Receive(buffer_video);
                Console.WriteLine("Video:" + buffer_video[1045]);
                VideoBitmapReady(videoUtils.ProcessByteStream(buffer_video));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
