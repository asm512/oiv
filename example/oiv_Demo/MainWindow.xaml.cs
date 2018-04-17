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
            nameDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.PackageName);
            authorDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.AuthorName);
            versionDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.Version);
            descriptionDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.Description);
            foregroundDislay.Text = muhOIV.BlackHeaderForeground().ToString();

            headerBackgroundDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.HeaderBackground);
            headerBackgroundDisplay.Foreground = muhOIV.HeaderBackgroundBrush();

            iconBackgroundDisplay.Text = muhOIV.GetProperty(OIVlib.OIV.Package.IconBackground);
            iconBackgroundDisplay.Foreground = muhOIV.IconBackgroundBrush();
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
