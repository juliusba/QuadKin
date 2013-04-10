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
using System.Windows.Threading;

namespace QuadKin.Quad.UserControls
{
    /// <summary>
    /// Interaction logic for CamDisplay.xaml
    /// </summary>
    public partial class FrontCamDisplay : UserControl
    {
        private WriteableBitmap source;
        private DispatcherTimer timerVideoUpdate;

        public FrontCamDisplay()
        {
            InitializeComponent();

            timerVideoUpdate = new DispatcherTimer();
            timerVideoUpdate.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timerVideoUpdate.Tick += new EventHandler(timerVideoUpdate_Tick);
            timerVideoUpdate.Start();

            //QuadCom.instance.stateChanged += stateChanged;
            QuadCom.instance.videoDataWorker.VideoBitmapReady += showCamImage;
        }

        ~FrontCamDisplay()
        {
            //QuadCom.instance.stateChanged -= stateChanged;
            QuadCom.instance.videoDataWorker.VideoBitmapReady -= showCamImage;
            timerVideoUpdate.Stop();
        }

        //private void stateChanged(State state)
        //{
            
        //}

        private void showCamImage(WriteableBitmap wmp)
        {
            this.source = wmp;
        }

        private void timerVideoUpdate_Tick(object sender, EventArgs e)
        {
            this.CamImage.Source = this.source;
        }
    }
}
