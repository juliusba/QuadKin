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

namespace QuadKin.QuadKin.UserControls
{
    /// <summary>
    /// Interaction logic for ControlDisplay.xaml
    /// </summary>
    public partial class ControlDisplay : UserControl
    {
        public ControlDisplay()
        {
            InitializeComponent();

            QuadKinCom.instance.CommandReady += update;
        }

        private void update(Command cmd)
        {
            if (cmd.valid)
            {
                Valid.Content = "Valid";
                Valid.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                Valid.Content = "Invalid";
                Valid.Foreground = new SolidColorBrush(Colors.Red);
            }

            UD.Content = cmd.UD;
            RL.Content = cmd.RL;
            FB.Content = cmd.FB;
            TRL.Content = cmd.TRL;
        }
    }
}
