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
        private OIVlib.OIV muhOIV = new OIVlib.OIV("oivSample.zip");

        public MainWindow()
        {
            InitializeComponent();
            muhOIV.AddLog(logRichTextbox);
            muhOIV.Open();
            oivIconDisplay.ImageSource = muhOIV.GetIcon();
            this.Title = muhOIV.GetProperty(OIVlib.OIV.Package.Name);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            muhOIV.Close();
        }
    }
}
