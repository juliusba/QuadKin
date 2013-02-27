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
using Microsoft.Kinect;
using System.Threading;
using System.IO;
using System.Diagnostics;
using QuadKin.Quad;

namespace QuadKin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum State
        {
            None,
            Awaiting,
            Controlling
        }


        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// The kinect sensor/camera.
        /// </summary>
        private KinectSensor kinSensor;

        private QuadCommunication quadCom;

        private DepthImagePixel[] depthPixels;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;

        private State state = State.None;
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            quadCom = new QuadCommunication();
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.kinSensor = potentialSensor;
                    break;
                }
            }

            if (null != this.kinSensor)
            {
                // Start the sensor!
                try
                {
                    this.kinSensor.Start();
                }
                catch (IOException)
                {
                    this.kinSensor = null;
                }

                this.LabelSensorStatus.Content = "Ready";
                this.LabelSensorStatus.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (null == this.kinSensor)
            {
                this.LabelSensorStatus.Content = "Connection failed";
                this.LabelSensorStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                this.kinSensor.ElevationAngle = 20;
                //this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                // Turn on the skeleton stream to receive skeleton frames
                this.kinSensor.SkeletonStream.Enable();
                this.kinSensor.DepthStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.kinSensor.SkeletonFrameReady += this.KinectSkeletonFrameReady;
                // Add an event handler to be called whenever there is new color frame data
                this.kinSensor.DepthFrameReady += this.KinectVideoFrameReady;

                this.depthPixels = new DepthImagePixel[this.kinSensor.DepthStream.FramePixelDataLength];
                this.colorPixels = new byte[this.kinSensor.DepthStream.FramePixelDataLength * sizeof(int)];
                this.colorBitmap = new WriteableBitmap(this.kinSensor.DepthStream.FrameWidth, this.kinSensor.DepthStream.FrameHeight,
                    96.0, 96.0, PixelFormats.Bgr32, null);

                this.Display.Source = this.colorBitmap;
            }
        }

        private void KinectVideoFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
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
                            colorPixelIndex ++;
                            
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

        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    Skeleton skel = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                    if (skel != null)
                    {
                        Command c = new Command(skel);

                        if (c.valid)
                        {
                            switch (this.state)
                            {
                                case State.None:
                                    this.state = State.Awaiting;
                                    Thread t = new Thread(() =>
                                        {
                                            quadCom.takeOff();
                                        });
                                    t.Start();
                                    this.stopwatch.Restart();
                                    this.LabelCountDown.Content = 5;
                                    this.LabelCountDown.FontSize = 64;
                                    this.LabelCountDown.Visibility = System.Windows.Visibility.Visible;
                                    break;
                                case State.Awaiting:
                                    long timeLeft = 5000 - this.stopwatch.ElapsedMilliseconds;
                                    if (timeLeft <= 0)
                                    {
                                        this.state = State.Controlling;
                                        this.LabelCountDown.Visibility = System.Windows.Visibility.Hidden;
                                        this.LabelsControlData.Visibility = System.Windows.Visibility.Visible;
                                        this.stopwatch.Reset();
                                    }
                                    else
                                    {

                                        int mod = (int)timeLeft % 1000;
                                        if (mod > 900)
                                        {
                                            this.LabelCountDown.Content = Math.Round((double)timeLeft / 1000);
                                            this.LabelCountDown.FontSize = Math.Max((double)Math.Abs(900 - mod) * 64 / 100, 1);
                                        }
                                        else
                                        {
                                            this.LabelCountDown.FontSize = Math.Max((double)mod * 64 / 900, 1);
                                        }
                                    }
                                    break;
                                case State.Controlling:
                                    quadCom.sendUpdatedValues(c);
                                    this.LabelUpDown.Content = c.UD + "%";
                                    this.LabelRightLeft.Content = c.RL + "%";
                                    this.LabelForthBack.Content = c.FB + "%";
                                    this.LabelTurnRightLeft.Content = c.TRL + "%";
                                    break;
                            }

                        }
                        else
                        {

                        }

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
                }
            }
        }

        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {
            if (joint.TrackingState == JointTrackingState.Tracked)
            {
                ellipse.Visibility = System.Windows.Visibility.Visible;
                ellipse.Margin = new Thickness(joint.Position.X * 320 + 320, 480 - (joint.Position.Y * 240 + 240), 0, 0);
            }
            else
            {
                ellipse.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.kinSensor)
            {
                this.kinSensor.Stop();
            }
        }
    }
}
