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
using oiv_Demo.OIVlib;
using System.Windows.Shapes;

namespace oiv_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReadOIV();
        }

        private void ReadOIV()
        {
            var muhOIV = new OIV("oivSample.zip");
            muhOIV.AddLog(logRichTextbox);
            muhOIV.Open();
            oivIconDisplay.ImageSource = muhOIV.GetIcon();
            nameDisplay.Text = muhOIV.GetProperty(OIV.Package.PackageName);
            authorDisplay.Text = muhOIV.GetProperty(OIV.Package.AuthorName);
            versionDisplay.Text = muhOIV.GetProperty(OIV.Package.Version);
            descriptionDisplay.Text = muhOIV.GetProperty(OIV.Package.Description);
            foregroundDislay.Text = muhOIV.BlackHeaderForeground().ToString();

            headerBackgroundDisplay.Text = muhOIV.GetProperty(OIV.Package.HeaderBackground);
            headerBackgroundDisplay.Foreground = muhOIV.HeaderBackgroundBrush();

            iconBackgroundDisplay.Text = muhOIV.GetProperty(OIV.Package.IconBackground);
            iconBackgroundDisplay.Foreground = muhOIV.IconBackgroundBrush();

            string tagVersion = muhOIV.GetProperty(OIV.Package.Tag);
            if (tagVersion != null) { tagDisplay.Text = tagVersion; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
