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

namespace QuadKin.Quad.UserControls
{
    /// <summary>
    /// Interaction logic for CamDisplay.xaml
    /// </summary>
    public partial class CamDisplay : UserControl
    {

        public CamDisplay()
        {
            InitializeComponent();

            QuadCom.instance.stateChanged += stateChanged;
            QuadCom.instance.VideoBitmapReady += showCamImage;
        }

        ~CamDisplay()
        {
            QuadCom.instance.stateChanged -= stateChanged;
            QuadCom.instance.VideoBitmapReady -= showCamImage;
        }

        private void stateChanged(State state)
        {
            
        }

        private void showCamImage(WriteableBitmap wmp)
        {
            this.CamImage.Source = wmp;
        }
    }
}
