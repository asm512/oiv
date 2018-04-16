using System;
using System.Xml.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;

namespace oiv_Demo.OIVlib
{
    class OIV
    {
        public string ZipPath { private set; get; }
        public string ExtractedPath { private set; get; }
        public bool HasBeenOpened { private set; get; }
        public string RootFolder { private set; get; }
        private System.Windows.Controls.RichTextBox logTextbox = null;

        #region Constructor

        public OIV(string path)
        {
            ZipPath = path;
            HasBeenOpened = false;
        }

        public OIV(string path, System.Windows.Controls.RichTextBox rtb)
        {
            ZipPath = path;
            logTextbox = rtb;
            logTextbox.IsReadOnly = true;
            HasBeenOpened = false;
        }

        #endregion Ctor

        private void AppendtoVisibleLog(string msg) => logTextbox.AppendText(msg + "\r\n");

        public void AddLog(System.Windows.Controls.RichTextBox rtb)
        {
            logTextbox = rtb;
        }

        /// <summary>
        /// Opens/Extracts the OIV so edits can be made
        /// </summary>
        /// <param name="path">Explict path to which to extract</param>
        public void Open(string path = "")
        {
            if (logTextbox != null) { AppendtoVisibleLog($"Opening {ZipPath}"); }
            if (HasBeenOpened) { if (logTextbox != null) { AppendtoVisibleLog("OIV package has already been extracted."); } throw new InvalidOperationException("OIV package has already been extracted."); }
            string oivExtractionPath;
            if (path == "") { oivExtractionPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\oiv\{Path.GetFileNameWithoutExtension(ZipPath)}\"; }
            else { oivExtractionPath = path; }
            if (!Directory.Exists(oivExtractionPath)) { Directory.CreateDirectory(oivExtractionPath); }
            ZipFile zf = null;
            FileStream fs = File.OpenRead(ZipPath);
            zf = new ZipFile(fs);

            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) { continue; }
                String entryFileName = zipEntry.Name;

                byte[] buffer = new byte[4096]; //4096 is optimal for performance, noticeable especially for Zips > 1Gb
                Stream zipStream = zf.GetInputStream(zipEntry);

                String fullZipToPath = Path.Combine(oivExtractionPath, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0) { Directory.CreateDirectory(directoryName); };

                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                }
            }
            HasBeenOpened = true;
            ExtractedPath = oivExtractionPath;
            SetRootFolder();
        }

        /// <summary>
        /// Closes the OIV package and disposes of any files
        /// </summary>
        public void Close()
        {
            foreach (var file in Directory.GetFiles(RootFolder))
            {
                try
                {
                    if (!file.Contains("icon")) { File.Delete(file); }
                }
                catch (Exception)
                {
                    foreach (var process in FileUtil.WhoIsLocking(file)) { MessageBox.Show($"{process.ToString()} is preventing the file from being deleted"); }
                }
            }
            foreach(var dir in Directory.GetDirectories(RootFolder))
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Check whether the object is a valid OIV archive
        /// </summary>
        public bool IsValidOIV()
        {
            if (Directory.GetFiles(ExtractedPath, "assembly.xml", SearchOption.AllDirectories).Length > 0) { return true; }
            else { return false; }
        }

        private void SetRootFolder()
        {
            string[] searchResult = Directory.GetFiles(ExtractedPath, "assembly.xml", SearchOption.AllDirectories);
            if (searchResult.Length > 0)
            {
                RootFolder = Path.GetDirectoryName(searchResult[0]); 
            }
            else
            {
                throw new InvalidOperationException("Root folder could not be determined");
            }
            MessageBox.Show($"Root folder: {RootFolder}");
        }

        /// <summary>
        /// Returns the icon that accompanies said oiv in a BitmapImage format which can be applied directly as a source to an Image control
        /// </summary>
        /// <returns>OIV Icon</returns>
        public BitmapImage GetIcon()
        {
            if (!HasBeenOpened) { throw new InvalidOperationException("OIV package has not been opened"); }
            BitmapImage image = new BitmapImage(new Uri($@"{RootFolder}\icon.png", UriKind.Absolute));
            return image;
            //return ConvertBitmap(new Bitmap($@"{RootFolder}\icon.png"));
        }

        private BitmapImage ConvertBitmap(Bitmap src)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public enum Package
        {
            Name,
            Version,
            DisplayName,
            Description,
            HeaderBackground,
            IconBackground
        }

        private XDocument ContentXML;
        private bool xmlLoaded = false;

        //Maybe use XMLReader if files seem to be too big for XDocument to load them properly
        private void LoadXML()
        {
            XDocument doc = XDocument.Load($"{RootFolder}\\assembly.xml");
            ContentXML = doc;
            xmlLoaded = true;
        }

        public string GetProperty(Package package)
        {
            if (!xmlLoaded) { LoadXML(); }
            switch (package)
            {
                case Package.Name:
                    var name = ContentXML.Root.Elements().Select(x => x.Element("name"));
                    return name.First().Value;
                case Package.Version:
                    break;
                case Package.DisplayName:
                    break;
                case Package.Description:
                    break;
                case Package.HeaderBackground:
                    break;
                case Package.IconBackground:
                    break;
                default:
                    break;
            }
            return "";
        }
        
    }
}
