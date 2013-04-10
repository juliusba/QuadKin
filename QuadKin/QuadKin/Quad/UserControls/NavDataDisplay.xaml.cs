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
    /// Interaction logic for NavDataDisplay.xaml
    /// </summary>
    public partial class NavDataDisplay : UserControl
    {
        private string battery;
        private string altitude;
        private string vx;
        private string vy;
        private string vz;

        private DispatcherTimer timerVideoUpdate;
        
        public NavDataDisplay()
        {
            InitializeComponent();

            timerVideoUpdate = new DispatcherTimer();
            timerVideoUpdate.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timerVideoUpdate.Tick += new EventHandler(timerNavDataUpdate_Tick);
            timerVideoUpdate.Start();
            QuadCom.instance.navDataWorker.NavDataReady += update;
        }

        ~NavDataDisplay()
        {
            QuadCom.instance.navDataWorker.NavDataReady -= update;
            timerVideoUpdate.Stop();
        }

        private void update(NavData data)
        {
            this.battery = data.battery + "%";
            this.altitude = Math.Round((float)data.altitude / 1000, 2) + " m";
            this.vx = "" + data.vx;
            this.vy = "" + data.vy;
            this.vz = "" + data.vz;
        }

        private void timerNavDataUpdate_Tick(object sender, EventArgs e)
        {
            this.Battery.Content = this.battery;
            this.Altitude.Content = this.altitude;
            this.XSpedd.Content = this.vx;
            this.YSpeed.Content = this.vy;
            this.ZSpeed.Content = this.vz;
        }
    }
}
