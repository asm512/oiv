using System;
using System.Xml.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Media;

namespace oiv_Demo.OIVlib
{
    /// <summary>
    /// OpenIV's OIV 
    /// </summary>
    class OIV : IDisposable
    {
        public string ZipPath { private set; get; }
        public string ExtractedPath { private set; get; }
        public bool HasBeenOpened { private set; get; }
        public string RootFolder { private set; get; }
        private System.Windows.Controls.RichTextBox logTextbox = null;

        #region Constructor

        public OIV(string path)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException(); }
            ZipPath = path;
            HasBeenOpened = false;
        }

        public OIV(string path, System.Windows.Controls.RichTextBox rtb)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException(); }
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
        public void Cleanup()
        {
            foreach (var file in Directory.GetFiles(RootFolder))
            {
                try
                {
                    //if (!file.Contains("icon")) { File.Delete(file); }
                    File.Delete(file);
                }
                catch (Exception)
                {
                    //TODO: Dispose of the stream in memory
                    foreach (var process in FileUtil.WhoIsLocking(file)) { MessageBox.Show($"{process.ToString()} is preventing the file from being deleted"); }
                }
            }
            foreach (var dir in Directory.GetDirectories(RootFolder))
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
            /// <summary>
            /// OIV Package Name
            /// </summary>
            PackageName,
            /// <summary>
            /// OIV Author Name
            /// </summary>
            AuthorName,
            /// <summary>
            /// OIV Package Mod Version
            /// </summary>
            Version,
            /// <summary>
            /// OIV Package Description
            /// </summary>
            Description,
            /// <summary>
            /// OIV Hex formatted header background colour
            /// </summary>
            HeaderBackground,
            /// <summary>
            /// OIV Icon Background
            /// </summary>
            IconBackground
        }

        private XDocument ContentXML;
        private bool xmlLoaded = false;

        //Maybe use XMLReader if files seem to be too big for XDocument to load them properly
        private void LoadXML()
        {
            if (xmlLoaded) { return; }
            XDocument doc = XDocument.Load($"{RootFolder}\\assembly.xml", LoadOptions.None);
            ContentXML = doc;
            xmlLoaded = true;
        }

        /// <summary>
        /// Returns custom oiv properties
        /// </summary>
        public string GetProperty(Package package)
        {
            if (!xmlLoaded) { LoadXML(); }
            switch (package)
            {
                case Package.PackageName:
                    var packageName = ContentXML.Root.Elements().Select(x => x.Element("name"));
                    return packageName.First().Value;
                case Package.AuthorName:
                    foreach (var element in ContentXML.Root.Descendants().Descendants())
                    {
                        if (element.Name.LocalName == "displayName")
                        {
                            return element.Value;
                        }
                    }
                    throw new InvalidOperationException("Assembly.xml is not valid");
                case Package.Version:
                    string version = "";
                    foreach (var element in ContentXML.Root.Descendants().Descendants())
                    {
                        if (element.Name.LocalName == "major") { version = element.Value; }
                        if (element.Name.LocalName == "minor") { return $"{version}.{element.Value}"; }
                    }
                    break;
                case Package.Description:
                    var description = ContentXML.Root.Elements().Select(x => x.Element("description"));
                    return description.First().Value;
                case Package.HeaderBackground:
                    foreach (var element in ContentXML.Root.Descendants().Descendants())
                    {
                        if (element.Name.LocalName == "headerBackground")
                        {
                            if (!element.Value.Contains("$")) { return element.Value; }
                            else { return element.Value.Replace("$", "#"); } 
                        }
                    }
                    break;
                case Package.IconBackground:
                    foreach (var element in ContentXML.Root.Descendants().Descendants())
                    {
                        if (element.Name.LocalName == "iconBackground")
                        {
                            if (!element.Value.Contains("$")) { return element.Value; }
                            else { return element.Value.Replace("$", "#"); }
                        }
                    }
                    break;
            }
            return "";
        }

        public bool BlackHeaderForeground()
        {
            foreach (var element in ContentXML.Root.Descendants().Descendants())
            {
                if (element.Name.LocalName == "headerBackground")
                {
                    if (element.HasAttributes)
                    {
                        foreach (var attrib in element.Attributes())
                        {
                            if(attrib.Name.LocalName == "useBlackTextColor")
                            {
                                if (attrib.Value.ToLower() == "true") { return true; }
                                else { return false; }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public System.Windows.Media.Brush IconBackgroundBrush()
        {
            var converter = new BrushConverter();
            var iconBrush = (System.Windows.Media.Brush)converter.ConvertFromString(GetProperty(Package.IconBackground));
            return iconBrush;
        }

        public System.Windows.Media.Brush HeaderBackgroundBrush()
        {
            var converter = new BrushConverter();
            var headerBrush = (System.Windows.Media.Brush)converter.ConvertFromString(GetProperty(Package.HeaderBackground));
            return headerBrush;
        }

        public void Dispose() => Cleanup();
    }
}