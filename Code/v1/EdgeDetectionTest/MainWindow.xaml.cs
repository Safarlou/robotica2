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
        private string filename = "images/foto1.png";

        private List<System.Drawing.Point> calibrationList;
		private bool calibrating = false;

        #region Image variables and values and shit

        Image<Bgr, byte> originalImage, ccImage, shapesImage, objectsImage;
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

			try { originalImage = new Image<Bgr, byte>(filename); }
			catch { return; }
			originalImageBox.Source = Utility.ToBitmapSource(originalImage);
        }

        // Maybe this needs to be refactored even more, but it's a lot better already.
        public void Process()
        {
            try { originalImage = new Image<Bgr, byte>(filename); }
            catch { return; }

            //ccImage = originalImage.Copy();
			//ccImage = Utility.ApplyPerPixel(ref originalImage, pix => Utility.MaskByColor(pix, Constants.Red, Constants.ThresholdRed));
			ccImage = Utility.FastColorExtract(ref originalImage);

            #region Optional (needed) processing
			//ccImage._EqualizeHist();
			//ccImage = copyImage.SmoothGaussian(3,3,3,3);
			//ccImage._GammaCorrect(1.5);
            #endregion

            #region Parsing textbox values to doubles, return if failure

            double threshold = Utility.ParseString(thresholdTextBox.Text);
            double thresholdLinking = Utility.ParseString(thresholdLinkingTextBox.Text);
            double minArea = Utility.ParseString(minAreaTextBox.Text);

            if (threshold == -1.0 || thresholdLinking == -1.0 || minArea == -1.0) return;

            #endregion

            edgesImage = Utility.FindEdges(ref ccImage, threshold, thresholdLinking);

            List<MCvBox2D> rectangles = Utility.FindRectangles(ref edgesImage, minArea);

			// Draw all rectangles on a black image
            shapesImage = originalImage.CopyBlank();
            foreach (MCvBox2D rect in rectangles)
                shapesImage.Draw(rect, new Bgr(System.Drawing.Color.Red), 1);

            foreach (MCvBox2D rect in rectangles)
            {
                Image<Gray, byte> mask = originalImage.CopyBlank().Convert<Gray, byte>();
                mask.Draw(rect, new Gray(256), -1);
                Bgr avg = originalImage.GetAverage(mask);
                shapesImage.Draw(rect, avg, -1);
            }

			objectsImage = shapesImage.Copy();

			originalImageBox.Source = Utility.ToBitmapSource(originalImage);
			ccImageBox.Source = Utility.ToBitmapSource(ccImage);
			edgesImageBox.Source = Utility.ToBitmapSource(edgesImage);
            shapesImageBox.Source = Utility.ToBitmapSource(shapesImage);
			objectsImageBox.Source = Utility.ToBitmapSource(objectsImage);
        }

        public void ProcessClicked(object sender, RoutedEventArgs e)
        {
            filename = fileTextBox.Text;
            Process();
        }

		public void StartCalibration(Constants.Colors color)
		{
			if (!calibrating)
			{
				calibrationList = new List<System.Drawing.Point>();
				calibrating = true;
			}
		}

		public void FinalizeCalibration(Constants.Colors color)
		{
			if (calibrating)
			{
				var bgrs = Utility.PointsToBgr(ref originalImage, calibrationList.ToArray());
				Constants.UpdateColor(color, bgrs.ToArray());
				calibrating = false;
			}
		}

        public void StartCalibrationRed(object sender, RoutedEventArgs e)
        {
			StartCalibration(Constants.Colors.Red);
        }

        public void FinalizeCalibrationRed(object sender, RoutedEventArgs e)
        {
			FinalizeCalibration(Constants.Colors.Red);
        }

        public void StartCalibrationGreen(object sender, RoutedEventArgs e)
		{
			StartCalibration(Constants.Colors.Green);
        }

        public void FinalizeCalibrationGreen(object sender, RoutedEventArgs e)
		{
			FinalizeCalibration(Constants.Colors.Green);
        }

        public void OriginalImageClicked(object sender, RoutedEventArgs e)
        {
			if (calibrating)
			{
				System.Windows.Point wp = Mouse.GetPosition(originalImageBox);
				//Hehe, dp
				System.Drawing.Point dp = new System.Drawing.Point((int)wp.X, (int)wp.Y);			// we need to add a bunch of points around the clicked point
				calibrationList.Add(dp); 
			}
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
