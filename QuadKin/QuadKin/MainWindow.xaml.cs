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
using System.IO;
using System.Diagnostics;
using QuadKin.Quad;
using QuadKin.Kinect;
using QuadKin.Kinect.UserControls;

namespace QuadKin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int gridCol = 0;
        private int gridRow = 0;

        public MainWindow()
        {
            InitializeComponent();

            KinCom.instance.stateChanged += kinStateChanged;
            QuadCom.instance.stateChanged += quadStateChanged;
            QuadKinCom.instance.stateChanged += quadKinStateChanged;
            kinStateChanged(KinCom.instance.State);
            quadStateChanged(QuadCom.instance.State);
            quadKinStateChanged(QuadKinCom.instance.State);
        }

        private void kinStateChanged(State state)
        {
            stateChanged(LabelKinectStatus, state);
        }

        private void quadStateChanged(State state)
        {
            stateChanged(LabelQuadStatus, state);
        }

        private void quadKinStateChanged(State state)
        {
            stateChanged(LabelState, state);
        }

        private void stateChanged(Label label, State state)
        {
            label.Content = state.ToString();
            switch (state)
            {
                case State.NoConnection:
                    label.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case State.Initializing:
                    label.Foreground = new SolidColorBrush(Colors.Yellow);
                    break;
                case State.Ready:
                    label.Foreground = new SolidColorBrush(Colors.Green);
                    break;
            }
        }

        private void addDisplay()
        {
            SkelDisplay kinectDisplay = new SkelDisplay();
            Grid.SetColumn(kinectDisplay, gridCol);
            Grid.SetRow(kinectDisplay, gridRow);
            Displays.Children.Add(kinectDisplay);
            Displays.UpdateLayout();


            //if (gridCol == 0 && gridRow == 0)
            //{
            //    ColumnDefinition coldef = new ColumnDefinition();
            //    coldef.Width = new GridLength(1, GridUnitType.Star);
            //    Displays.ColumnDefinitions.Add(coldef); 
            //}
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KinCom.instance.Stop();
            QuadCom.instance.Stop();
        }
    }
}
