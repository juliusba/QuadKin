using Microsoft.Kinect;
using QuadKin.Kin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuadKin.Kin.UserControls
{
    /// <summary>
    /// Interaction logic for CamDisplay.xaml
    /// </summary>
    public partial class CamDisplay : UserControl
    {
        private DepthImagePixel[] depthPixels;
        private byte[] depthColorPixels;
        private WriteableBitmap depthColorBitmap;

        private int depthAlpha = 100;
        
        public CamDisplay()
        {
            InitializeComponent();

            stateChanged(KinCom.instance.State);
            KinCom.instance.stateChanged += stateChanged;
        }

        ~CamDisplay()
        {
            KinCom.instance.stateChanged -= stateChanged;
            KinCom.instance.depthFrameReady -= drawWhiteOnBlack;
        }

        private void stateChanged(State state)
        {
            if (state == State.Ready)
            {
                DepthImageStream stream = KinCom.instance.getDepthStream();
                this.depthPixels = new DepthImagePixel[stream.FramePixelDataLength];
                this.depthColorPixels = new byte[stream.FramePixelDataLength * sizeof(int)];
                this.depthColorBitmap = new WriteableBitmap(stream.FrameWidth, stream.FrameHeight,
                    96.0, 96.0, PixelFormats.Bgr32, null);

                this.DepthDisplay.Source = this.depthColorBitmap;
                KinCom.instance.depthFrameReady += drawWhiteOnBlack;


            }
        }

        private void drawWhiteOnBlack(DepthImageFrame depthFrame)
        {
            depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

            // Get the min and max reliable depth for the current players
            //int minDepth = depthPixels[0].Depth;
            //int maxDepth = depthPixels[0].Depth;
            //double avgDepth = 0;
            //int playerPixelCount = 0;

            //for (int i = 0; i < this.depthPixels.Length; ++i)
            //{
            //    if (depthPixels[i].PlayerIndex == 1)
            //    {
            //        short depth = this.depthPixels[i].Depth;
            //        minDepth = Math.Min(depth, minDepth);
            //        maxDepth = Math.Max(depth, maxDepth);
            //        avgDepth += depth;
            //        playerPixelCount++;
            //    }
            //}

            //avgDepth /= playerPixelCount;

            // Convert the depth to RGB
            int colorPixelIndex = 0;
            for (int i = 0; i < this.depthPixels.Length; ++i)
            {
                if (depthPixels[i].PlayerIndex != 0)
                {
                    this.depthColorPixels[colorPixelIndex++] = (byte)255;
                    this.depthColorPixels[colorPixelIndex++] = (byte)255;
                    this.depthColorPixels[colorPixelIndex++] = (byte)255;
                    this.depthColorPixels[colorPixelIndex++] = (byte) this.depthAlpha;

                    //short depth = this.depthPixels[i].Depth;

                    //// Write out blue byte
                    //this.colorPixels[colorPixelIndex++] = (byte)Math.Min(depth - avgDepth, 0);
                    //// Write out green byte
                    //this.colorPixels[colorPixelIndex++] = (byte)maxDepth;
                    //// Write out red byte                        
                    //this.colorPixels[colorPixelIndex++] = (byte)Math.Min(avgDepth - depth, 0);
                    //// Write alpha if Bgra, else unused...
                    //++colorPixelIndex;
                }
                else
                {

                    byte intensity = (byte)0;

                    // Write out blue byte
                    this.depthColorPixels[colorPixelIndex++] = intensity;
                    // Write out green byte
                    this.depthColorPixels[colorPixelIndex++] = intensity;
                    // Write out red byte                        
                    this.depthColorPixels[colorPixelIndex++] = intensity;
                    // Write alpha if Bgra, else unused...
                    this.depthColorPixels[colorPixelIndex++] = (byte) this.depthAlpha;
                }
            }

            this.depthColorBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthColorBitmap.PixelWidth, this.depthColorBitmap.PixelHeight),
                this.depthColorPixels,
                this.depthColorBitmap.PixelWidth * sizeof(int),
                0);
        }
    }
}
