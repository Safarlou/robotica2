using EdgeDetectionTest.Properties;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace EdgeDetectionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
         * Calibration is quite shitty at the moment...
         * Make sure to start and stop one calibration at a time.
         * I.e.: start red, stop red, start green, stop green.
         * DO NOT: start red, start green, etc... World will implode.
         * */
        private string filename = "foto1.png";

        private List<System.Drawing.Point> calibrationListRed;
        private List<System.Drawing.Point> calibrationListGreen;
        private bool calibratingRed = false, calibratingGreen = false;

        #region Image variables and values and shit

        Image<Bgr, byte> originalImage, ccImage, contourResult;
        Image<Gray, byte> edgesImage;

        double startingThreshold = 150;
        double startingThresholdLinking = 90;
        double startingMinArea = 50;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            fileTextBox.Text = filename;
            thresholdTextBox.Text = startingThreshold.ToString();
            thresholdLinkingTextBox.Text = startingThresholdLinking.ToString();
            minAreaTextBox.Text = startingMinArea.ToString();
        }

        // Maybe this needs to be refactored even more, but it's a lot better already.
        public void Process()
        {
            try { originalImage = new Image<Bgr, byte>(filename); }
            catch { return; }
            originalImageBox.Source = Utility.ToBitmapSource(originalImage);

            // red in foto1 = new Bgr(73, 55, 206)
            // green in foto1 = new Bgr(106, 169, 74)

            ccImage = originalImage.Copy();

            //Utility.ApplyPerPixel(ccImage, pix => Utility.MaskByColor(pix, new Bgr(73, 55, 206), 100));

            #region Optional (needed) processing
            //copyImage._EqualizeHist();
            //copyImage = copyImage.SmoothGaussian(3,3,3,3);
            //copyImage._GammaCorrect(1.5);
            //ccImageBox.Source = ToBitmapSource(copyImage);
            #endregion

            ccImageBox.Source = Utility.ToBitmapSource(ccImage);

            #region Parsing textbox values to doubles, return if failure

            double threshold = Utility.ParseString(thresholdTextBox.Text);
            double thresholdLinking = Utility.ParseString(thresholdLinkingTextBox.Text);
            double minArea = Utility.ParseString(minAreaTextBox.Text);

            if (threshold == -1.0 || thresholdLinking == -1.0 || minArea == -1.0) return;

            #endregion

            edgesImage = Utility.FindEdges(ref ccImage, threshold, thresholdLinking);
            edgesImageBox.Source = Utility.ToBitmapSource(edgesImage);

            List<MCvBox2D> rectangles = Utility.FindRectangles(ref edgesImage, minArea);

            contourResult = originalImage.CopyBlank();

            // Draw all rectangles on a black image
            foreach (MCvBox2D rect in rectangles)
                contourResult.Draw(rect, new Bgr(System.Drawing.Color.Red), 1);

            shapesImageBox.Source = Utility.ToBitmapSource(contourResult);

            foreach (MCvBox2D rect in rectangles)
            {
                Image<Gray, byte> mask = originalImage.CopyBlank().Convert<Gray, byte>();
                mask.Draw(rect, new Gray(256), -1);
                Bgr avg = originalImage.GetAverage(mask);
                contourResult.Draw(rect, avg, -1);
            }

            objectsImageBox.Source = Utility.ToBitmapSource(contourResult);
        }

        public void ProcessClicked(object sender, RoutedEventArgs e)
        {
            filename = fileTextBox.Text;
            Process();
        }

        public void StartCalibrationRed(object sender, RoutedEventArgs e)
        {
            if (calibrationListRed == null) calibrationListRed = new List<System.Drawing.Point>();
            calibratingRed = true;
            calibratingGreen = false;
        }

        public void FinalizeCalibrationRed(object sender, RoutedEventArgs e)
        {
            //do things
            Constants.UpdateRed(Utility.PointsToBgr(ref originalImage, calibrationListRed.ToArray()).ToArray());
            calibrationListRed = null;
        }

        public void StartCalibrationGreen(object sender, RoutedEventArgs e)
        {
            if (calibrationListGreen == null) calibrationListGreen = new List<System.Drawing.Point>();
            calibratingRed = false;
            calibratingGreen = true;
        }

        public void FinalizeCalibrationGreen(object sender, RoutedEventArgs e)
        {
            //do things
            Constants.UpdateGreen(Utility.PointsToBgr(ref originalImage, calibrationListGreen.ToArray()).ToArray());
            calibrationListRed = null;
        }

        public void OriginalImageClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Point wp = Mouse.GetPosition(originalImageBox);
            //Hehe, dp
            System.Drawing.Point dp = new System.Drawing.Point((int)wp.X, (int)wp.Y);
            if (calibratingRed && calibrationListRed != null) { calibrationListRed.Add(dp); }
            else if (calibratingGreen && calibrationListGreen != null) { calibrationListGreen.Add(dp); }
        }

        //Method is here for shits and giggles, not actually used right now
        public void UpdateFilepath(object sender, RoutedEventArgs e)
        {
            //Create file dialog object
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //Set default extension and filters
            dlg.DefaultExt = "*.jpg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";

            //Show the file dialog
            Nullable<bool> result = dlg.ShowDialog();

            //Get selected filename and display in textbox
            if (result == true)
            {
                filename = dlg.FileName;
                fileTextBox.Text = filename;
            }
        }
    }
}
