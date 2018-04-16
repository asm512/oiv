using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace oiv_Demo.OIVlib
{
    class OIV
    {
        string zipPath;
        public string extractedPath;
        public bool hasBeenOpened = false;
        public string RootFolder { private set; get; }
        private System.Windows.Controls.RichTextBox logTextbox = null;

        public OIV(string path)
        {
            zipPath = path;
        }

        public OIV(string path, System.Windows.Controls.RichTextBox rtb)
        {
            zipPath = path;
            logTextbox = rtb;
            logTextbox.IsReadOnly = true;
        }

        private void AppendtoVisibleLog(string msg)
        {
            logTextbox.AppendText(msg + "\r\n");
        }

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
            if (logTextbox != null) { AppendtoVisibleLog($"Opening {zipPath}"); }
            if (hasBeenOpened) { if (logTextbox != null) { AppendtoVisibleLog("OIV package has already been extracted."); } throw new InvalidOperationException("OIV package has already been extracted."); }
            string oivExtractionPath;
            if (path == "") { oivExtractionPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\oiv\{Path.GetFileNameWithoutExtension(zipPath)}\"; }
            else { oivExtractionPath = path; }
            if (!Directory.Exists(oivExtractionPath)) { Directory.CreateDirectory(oivExtractionPath); }
            ZipFile zf = null;
            FileStream fs = File.OpenRead(zipPath);
            zf = new ZipFile(fs);

            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) { continue; }
                String entryFileName = zipEntry.Name;

                byte[] buffer = new byte[4096]; //4096 is optimal
                Stream zipStream = zf.GetInputStream(zipEntry);

                String fullZipToPath = Path.Combine(oivExtractionPath, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0) { Directory.CreateDirectory(directoryName); };

                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                }
            }
            hasBeenOpened = true;
            extractedPath = oivExtractionPath;
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
                    File.Delete(file);
                }
                catch (Exception)
                {
                    foreach (var process in FileUtil.WhoIsLocking(file)) { MessageBox.Show(process.ToString()); }
                }
            }
        }
        
        private void SetRootFolder()
        {
            string[] searchResult = Directory.GetFiles(extractedPath, "assembly.xml", SearchOption.AllDirectories);
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
        /// Check whether the object is a valid OIV archive
        /// </summary>
        public bool IsValidOIV()
        {
            if (Directory.GetFiles(extractedPath, "assembly.xml", SearchOption.AllDirectories).Length > 0) { return true; }
            else { return false; } 
        }

        /// <summary>
        /// Returns the icon that accompanies said oiv in a BitmapImage format which can be applied directly as a source to an Image control
        /// </summary>
        /// <returns>OIV Icon</returns>
        public BitmapImage GetIcon()
        {
            if (!hasBeenOpened) { throw new InvalidOperationException("OIV package has not been opened"); }
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
    }
}
