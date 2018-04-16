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

namespace oiv_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OIVlib.OIV muhOIV;

        public MainWindow()
        {
            InitializeComponent();
            muhOIV = new OIVlib.OIV(@"C:\Users\Jason\Desktop\oivSample.zip", logRichTextbox);
            muhOIV.Open();
            //oivIconDisplay.ImageSource = muhOIV.GetIcon();
            muhOIV.GetIcon();
            this.Title = muhOIV.IsValidOIV().ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            oivIconDisplay.ImageSource = null;
            muhOIV.Close();
        }
    }
}
