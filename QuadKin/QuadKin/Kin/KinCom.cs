using Microsoft.Kinect;
using QuadKin.Kin.UserControls;
using QuadKin.Quad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuadKin.Kin
{
    public class KinCom : StateClass
    {
        /// <summary>
        /// The kinect sensor/camera.
        /// </summary>
        private KinectSensor kinSensor;

        public delegate void SkeletonHandler(Skeleton skel);
        public event SkeletonHandler skeletonReady;

        public delegate void DepthFrameHandler(DepthImageFrame depthFrame);
        public event DepthFrameHandler depthFrameReady;

        private static KinCom kinCom;
        private static object syncRoot = new Object();

        private KinCom()
        {
            Init();
        }

        public static KinCom instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (kinCom == null)
                    {
                        kinCom = new KinCom();
                    }
                    return kinCom;
                }
            }
        }

        public static void Stop()
        {
            if (null != kinCom.kinSensor)
            {
                kinCom.kinSensor.Stop();
                // Turn off the skeleton stream to receive skeleton frames
                kinCom.kinSensor.SkeletonStream.Disable();
                kinCom.kinSensor.DepthStream.Disable();

                // Add an event handler to be called whenever there is new color frame data
                kinCom.kinSensor.SkeletonFrameReady -= kinCom.KinectSkeletonFrameReady;
                // Add an event handler to be called whenever there is new color frame data
                kinCom.kinSensor.DepthFrameReady -= kinCom.KinectDepthFrameReady;
            }
            kinCom = null;
        }

        public bool Init(Label statusLabel = null)
        {
            State = State.Initializing;
            
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

                if (statusLabel != null)
                {
                    statusLabel.Content = "Ready";
                    statusLabel.Foreground = new SolidColorBrush(Colors.Green); 
                }
            }

            if (null == this.kinSensor)
            {
                State = State.NoConnection;
                return false;
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
                this.kinSensor.DepthFrameReady += this.KinectDepthFrameReady;

                State = State.Ready;
                return true;
            }
        }

        public DepthImageStream getDepthStream()
        {
            return this.kinSensor.DepthStream;
        }

        private void KinectDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    if (depthFrameReady != null)
                    {
                        depthFrameReady(depthFrame);
                    }
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
                        if (skeletonReady != null)
                        {
                            skeletonReady(skel);
                        }
                    }
                }
            }
        }
    }
}
