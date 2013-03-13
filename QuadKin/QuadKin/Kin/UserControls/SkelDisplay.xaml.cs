using Microsoft.Kinect;
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

namespace QuadKin.Kinect.UserControls
{
    /// <summary>
    /// Interaction logic for KinectDisplay.xaml
    /// </summary>
    public partial class SkelDisplay : UserControl
    {
        private DepthImagePixel[] depthPixels;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;

        public SkelDisplay()
        {
            InitializeComponent();

            stateChanged(KinCom.instance.State);
            KinCom.instance.stateChanged += stateChanged;
        }

        //~SkelDisplay()
        //{
        //    KinCom.instance.skeletonReady -= drawSkeleton;
        //    KinCom.instance.depthFrameReady -= drawWhiteOnBlack;
        //}

        private void stateChanged(State state)
        {
            if (state == State.Ready)
            {
                DepthImageStream stream = KinCom.instance.getDepthStream();
                this.depthPixels = new DepthImagePixel[stream.FramePixelDataLength];
                this.colorPixels = new byte[stream.FramePixelDataLength * sizeof(int)];
                this.colorBitmap = new WriteableBitmap(stream.FrameWidth, stream.FrameHeight,
                    96.0, 96.0, PixelFormats.Bgr32, null);

                this.ImageDisplay.Source = this.colorBitmap;

                KinCom.instance.skeletonReady += drawSkeleton;
                KinCom.instance.depthFrameReady += drawWhiteOnBlack;
            }
        }

        private void drawSkeleton(Skeleton skel)
        {
            SetEllipsePosition(this.JointHipCenter, skel.Joints[JointType.HipCenter]);
            SetEllipsePosition(this.JointSpine, skel.Joints[JointType.Spine]);
            SetEllipsePosition(this.JointShoulderCenter, skel.Joints[JointType.ShoulderCenter]);
            SetEllipsePosition(this.JointHead, skel.Joints[JointType.Head]);
            SetEllipsePosition(this.JointShoulderLeft, skel.Joints[JointType.ShoulderLeft]);
            SetEllipsePosition(this.JointElbowLeft, skel.Joints[JointType.ElbowLeft]);
            SetEllipsePosition(this.JointWristLeft, skel.Joints[JointType.WristLeft]);
            SetEllipsePosition(this.JointHandLeft, skel.Joints[JointType.HandLeft]);
            SetEllipsePosition(this.JointShoulderRight, skel.Joints[JointType.ShoulderRight]);
            SetEllipsePosition(this.JointElbowRight, skel.Joints[JointType.ElbowRight]);
            SetEllipsePosition(this.JointWristRight, skel.Joints[JointType.WristRight]);
            SetEllipsePosition(this.JointHandRight, skel.Joints[JointType.HandRight]);
            SetEllipsePosition(this.JointHipLeft, skel.Joints[JointType.HipLeft]);
            SetEllipsePosition(this.JointKneeLeft, skel.Joints[JointType.KneeLeft]);
            SetEllipsePosition(this.JointAnkleLeft, skel.Joints[JointType.AnkleLeft]);
            SetEllipsePosition(this.JointFootLeft, skel.Joints[JointType.FootLeft]);
            SetEllipsePosition(this.JointHipRight, skel.Joints[JointType.HipRight]);
            SetEllipsePosition(this.JointKneeRight, skel.Joints[JointType.KneeRight]);
            SetEllipsePosition(this.JointAnkleRight, skel.Joints[JointType.AnkleRight]);
            SetEllipsePosition(this.JointFootRight, skel.Joints[JointType.FootRight]);
        }

        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {
            if (joint.TrackingState == JointTrackingState.Tracked)
            {
                ellipse.Visibility = System.Windows.Visibility.Visible;
                ellipse.Margin = new Thickness(
                    joint.Position.X * this.ActualWidth / 2 + this.ActualWidth / 2,
                    this.ActualHeight - (joint.Position.Y * this.ActualHeight / 2 + this.ActualHeight / 2), 0, 0);
            }
            else
            {
                ellipse.Visibility = System.Windows.Visibility.Hidden;
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
                    this.colorPixels[colorPixelIndex++] = (byte)255;
                    this.colorPixels[colorPixelIndex++] = (byte)255;
                    this.colorPixels[colorPixelIndex++] = (byte)255;
                    colorPixelIndex++;

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
                    this.colorPixels[colorPixelIndex++] = intensity;
                    // Write out green byte
                    this.colorPixels[colorPixelIndex++] = intensity;
                    // Write out red byte                        
                    this.colorPixels[colorPixelIndex++] = intensity;
                    // Write alpha if Bgra, else unused...
                    ++colorPixelIndex;
                }
            }

            this.colorBitmap.WritePixels(
                new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                this.colorPixels,
                this.colorBitmap.PixelWidth * sizeof(int),
                0);
        }
    }
}
