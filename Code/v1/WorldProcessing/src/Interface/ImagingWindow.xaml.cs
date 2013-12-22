using WorldProcessing.Properties;
using Emgu.CV;
using Emgu.CV.CvEnum;
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

namespace WorldProcessing
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ImagingWindow : Window
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

		private Capture capture;

		#region Image variables and values and shit

		Image<Bgr, byte> originalImage, shapesImage, objectsImage, tempImage, maskImage;
		Image<Gray, byte> extractedImage, contoursImage;

		#endregion

		public ImagingWindow()
		{
			InitializeComponent();
            fileTextBox.Text = filename;
            capture = new Capture();

			try { originalImage = new Image<Bgr, byte>(filename); }
            catch { return; }
			//capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1600);
			//capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 1200);
            capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_AUTO_EXPOSURE, 0);
			originalImage = capture.QueryFrame().Copy(); // must do copy because queryframe sucks
			originalImage = capture.QueryFrame().Copy(); // must do copy because queryframe sucks
			originalImageBox.Width = originalImage.Width;
			originalImageBox.Height = originalImage.Height;
			originalImageBox.Source = Utility.ToBitmapSource(originalImage);

		}

        public void UpdateFrame(object sender, EventArgs e)
        {
            originalImage = capture.QueryFrame().Copy(); // must do copy because queryframe sucks
            //originalImageBox.Source = Utility.ToBitmapSource(originalImage);
            Process();
        }

		// Maybe this needs to be refactored even more, but it's a lot better already.
		public void Process()
		{

			extractedImage = Utility.FastColorExtract(ref originalImage, new Constants.Colors[] { Constants.Colors.Red })[0];

			using (MemStorage storage = new MemStorage())
			{
				Contour<System.Drawing.Point> contours = extractedImage.FindContours(
						Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_LINK_RUNS,
						Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);

				contoursImage = extractedImage.CopyBlank();

				for (Contour<System.Drawing.Point> drawingcontours = contours; drawingcontours != null; drawingcontours = drawingcontours.HNext)
				{
					contoursImage.Draw(drawingcontours, new Gray(255), 1);
				}

				List<MCvBox2D> rectangles = Utility.FindRectangles(contours);

				// Draw all rectangles on a black image
				shapesImage = originalImage.CopyBlank();
				foreach (MCvBox2D rect in rectangles)
					shapesImage.Draw(rect, new Bgr(System.Drawing.Color.Red), 1);

				objectsImage = shapesImage.CopyBlank();

				foreach (MCvBox2D rect in rectangles)
				{
					Image<Gray, byte> mask = originalImage.CopyBlank().Convert<Gray, byte>();
					mask.Draw(rect, new Gray(256), -1);
					Bgr avg = originalImage.GetAverage(mask);
					objectsImage.Draw(rect, avg, -1);
				}
			}

            //originalImageBox.Source = Utility.ToBitmapSource(originalImage);
            //extractImageBox.Source = Utility.ToBitmapSource(extractedImage);
            //contoursImageBox.Source = Utility.ToBitmapSource(contoursImage);
            //shapesImageBox.Source = Utility.ToBitmapSource(shapesImage);
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
				tempImage = originalImage.Copy();
				maskImage = originalImage.CopyBlank();
			}
		}

		public void FinalizeCalibration(Constants.Colors color)
		{
			if (calibrating)
			{
				var bgrs = Utility.PointsToBgr(ref originalImage, calibrationList.ToArray());
				//Constants.UpdateColor(color, bgrs.ToArray());
				Constants.UpdateColor(color, originalImage, maskImage);
				calibrating = false;
				originalImageBox.Source = Utility.ToBitmapSource(originalImage);
            }
            System.Windows.Interop.ComponentDispatcher.ThreadIdle += new System.EventHandler(UpdateFrame);
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

				var circlesize = 10;
				tempImage.Draw(new CircleF(dp, circlesize), new Bgr(0, 0, 0), -1);
				maskImage.Draw(new CircleF(dp, circlesize), new Bgr(1, 1, 1), -1);
				
				originalImageBox.Source = Utility.ToBitmapSource(tempImage);
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

