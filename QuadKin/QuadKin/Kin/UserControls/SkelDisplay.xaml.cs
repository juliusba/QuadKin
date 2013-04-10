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

namespace QuadKin.Kin.UserControls
{
    /// <summary>
    /// Interaction logic for KinectDisplay.xaml
    /// </summary>
    public partial class SkelDisplay : UserControl
    {

        public SkelDisplay()
        {
            InitializeComponent();

            stateChanged(KinCom.instance.State);
            KinCom.instance.stateChanged += stateChanged;
        }

        ~SkelDisplay()
        {
            KinCom.instance.stateChanged -= stateChanged;
            KinCom.instance.skeletonReady -= drawSkeleton;
        }

        private void stateChanged(State state)
        {
            if (state == State.Ready)
            {
                KinCom.instance.skeletonReady += drawSkeleton;
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

            setLinePosition(this.UpperRightArm, skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight]);
            setLinePosition(this.UpperLeftArm, skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft]);
            setLinePosition(this.LowerRightArm, skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
            setLinePosition(this.LowerLeftArm, skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
            setLinePosition(this.RightHand, skel.Joints[JointType.WristRight], skel.Joints[JointType.HandRight]);
            setLinePosition(this.LeftHand, skel.Joints[JointType.WristLeft], skel.Joints[JointType.HandLeft]);
            setLinePosition(this.Neck, skel.Joints[JointType.Head], skel.Joints[JointType.ShoulderCenter]);
            setLinePosition(this.Spine, skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.HipCenter]);
            setLinePosition(this.RightShoulder, skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.ShoulderRight]);
            setLinePosition(this.LeftShoulder, skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.ShoulderLeft]);
            setLinePosition(this.RightHip, skel.Joints[JointType.HipCenter], skel.Joints[JointType.HipRight]);
            setLinePosition(this.LeftHip, skel.Joints[JointType.HipCenter], skel.Joints[JointType.HipLeft]);
            setLinePosition(this.UpperRightLeg, skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight]);
            setLinePosition(this.UpperLeftLeg, skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft]);
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

        private void setLinePosition(Line line, Joint j1, Joint j2)
        {
            if (j1.TrackingState == JointTrackingState.Tracked && j2.TrackingState == JointTrackingState.Tracked)
            {
                line.X1 = j1.Position.X * this.ActualWidth / 2 + this.ActualWidth / 2;
                line.X2 = j2.Position.X * this.ActualWidth / 2 + this.ActualWidth / 2;
                line.Y1 = this.ActualHeight - (j1.Position.Y * this.ActualHeight / 2 + this.ActualHeight / 2);
                line.Y2 = this.ActualHeight - (j2.Position.Y * this.ActualHeight / 2 + this.ActualHeight / 2);
                line.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                line.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
