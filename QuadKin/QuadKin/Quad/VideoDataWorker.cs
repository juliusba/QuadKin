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
        private UdpClient socket_video;
        private VideoUtils videoUtils = new VideoUtils();
        private IPEndPoint ipEP;
        private byte[] buffer;

        public delegate void WritableBitmapHandler(WriteableBitmap wbm);
        public event WritableBitmapHandler VideoBitmapReady;

        internal bool Init(IPAddress ipAdd, string ipAdress, int PORT_VIDEO)
        {
            this.ipEP = new IPEndPoint(ipAdd, PORT_VIDEO);

            byte[] buffer = { 0x01, 0x00, 0x00, 0x00 };
            socket_video = new UdpClient(ipAdress, PORT_VIDEO);

            socket_video.Send(buffer, buffer.Length);

            videoUtils.ImageComplete += VideoImage_ImageComplete;

            StartWorkerThread();
            return true;
        }

        protected override void doWork()
        {
            try
            {
                buffer = socket_video.Receive(ref ipEP);
                Console.WriteLine("Video:" + buffer[1045]);
                videoUtils.ProcessByteStream(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void VideoImage_ImageComplete(object sender, DroneImageCompleteEventArgs e)
        {
            WriteableBitmap videoImage = e.ImageSource as WriteableBitmap;
            if (VideoBitmapReady != null)
                VideoBitmapReady(videoImage);
        }
    }
}
