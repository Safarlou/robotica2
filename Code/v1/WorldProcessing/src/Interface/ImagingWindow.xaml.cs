using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Planning;
using WorldProcessing.Representation;
using WorldProcessing.Vision;

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

		Image<Bgr, byte> originalImage;
		Image<Gray, byte> maskImage;

		public ImagingWindow(InputStream input, ImageAnalyser analyser, WorldModel model, Planner planner)
		{
			InitializeComponent();

			input.FrameReadyEvent += OnFrameReadyEvent;
			analyser.FrameAnalysedEvent += OnFrameAnalysedEvent;
			// todo: subscribe to worldmodel and planner

			fileTextBox.Text = filename;

			foreach (Constants.Colors color in Enum.GetValues(typeof(Constants.Colors))) ColorChooser.Items.Add(color);
			ColorChooser.SelectedIndex = 0;
		}

		private void OnFrameReadyEvent(object sender, FrameReadyEventArgs args)
		{
			originalImage = args.image;
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
			{
				originalImageBox.Width = originalImage.Width;
				originalImageBox.Height = originalImage.Height;

				if (calibrating)
				{
					var maskedImage = originalImage.Copy();
					maskedImage.SetValue(new Bgr(0, 0, 0), maskImage);
					originalImageBox.Source = Utility.ToBitmapSource(maskedImage);
				}
				else
					originalImageBox.Source = Utility.ToBitmapSource(originalImage);
			}));
		}

		private void OnFrameAnalysedEvent(object sender, FrameAnalysedEventArgs args)
		{
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
				{
					// just showing all the red versions for now by taking [0] from each step
					var results = args.results;
					var color = ColorChooser.SelectedValue;
					extractImageBox.Source = Utility.ToBitmapSource(results.colorMasks[(int)color].Item2);
					contoursImageBox.Source = Utility.ToBitmapSource(ContoursToImage(results.contours[(int)color].Item2));
					shapesImageBox.Source = Utility.ToBitmapSource(ShapesToImage(results.shapes[(int)color].Item2));
					//objectsImageBox.Source = Utility.ToBitmapSource(objectsImage);
				}));
		}

		private Image<Gray, byte> ContoursToImage(Contour<System.Drawing.Point> contours)
		{
			var contoursImage = originalImage.CopyBlank().Convert<Gray, byte>();

			for (Contour<System.Drawing.Point> drawingcontours = contours; drawingcontours != null; drawingcontours = drawingcontours.HNext)
			{
				contoursImage.Draw(drawingcontours, new Gray(256), 1);
			}

			return contoursImage;
		}

		private Image<Gray, byte> ShapesToImage(List<MCvBox2D> shapes)
		{
			var shapesImage = originalImage.CopyBlank().Convert<Gray, byte>();

			foreach (MCvBox2D shape in shapes)
				shapesImage.Draw(shape, new Gray(256), 1);

			return shapesImage;
		}

		//public void Process()
		//{

		//	// extractedImage = Utility.FastColorMask(ref originalImage, new Constants.Colors[] { Constants.Colors.Red })[0];

		//	using (MemStorage storage = new MemStorage())
		//	{
		//		Contour<System.Drawing.Point> contours = extractedImage.FindContours(
		//				Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_LINK_RUNS,
		//				Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);

		//		/* use in interface */
		//		contoursImage = extractedImage.CopyBlank();

		//		for (Contour<System.Drawing.Point> drawingcontours = contours; drawingcontours != null; drawingcontours = drawingcontours.HNext)
		//		{
		//			contoursImage.Draw(drawingcontours, new Gray(255), 1);
		//		}
		//		/**/

		//		List<MCvBox2D> rectangles = Utility.FindRectangles(contours);

		//		/* use in interface */
		//		// Draw all rectangles on a black image
		//		shapesImage = originalImage.CopyBlank();
		//		foreach (MCvBox2D rect in rectangles)
		//			shapesImage.Draw(rect, new Bgr(System.Drawing.Color.Red), 1);

		//		objectsImage = shapesImage.CopyBlank();

		//		foreach (MCvBox2D rect in rectangles)
		//		{
		//			Image<Gray, byte> mask = originalImage.CopyBlank().Convert<Gray, byte>();
		//			mask.Draw(rect, new Gray(256), -1);
		//			Bgr avg = originalImage.GetAverage(mask);
		//			objectsImage.Draw(rect, avg, -1);
		//		}
		//		/**/
		//	}

		//	//originalImageBox.Source = Utility.ToBitmapSource(originalImage);
		//	//extractImageBox.Source = Utility.ToBitmapSource(extractedImage);
		//	//contoursImageBox.Source = Utility.ToBitmapSource(contoursImage);
		//	//shapesImageBox.Source = Utility.ToBitmapSource(shapesImage);
		//	objectsImageBox.Source = Utility.ToBitmapSource(objectsImage);
		//}

		public void StartCalibration(object sender, EventArgs e)
		{
			if (!calibrating)
			{
				ColorChooser.IsEnabled = false;
				Constants.Colors color = (Constants.Colors)ColorChooser.SelectedValue;

				calibrationList = new List<System.Drawing.Point>();
				calibrating = true;
				maskImage = originalImage.CopyBlank().Convert<Gray, byte>();
			}
		}

		public void FinalizeCalibration(object sender, EventArgs e)
		{
			if (calibrating)
			{
				Constants.Colors color = (Constants.Colors)ColorChooser.SelectedValue;

				var bgrs = Utility.PointsToBgr(ref originalImage, calibrationList.ToArray());
				//Constants.UpdateColor(color, bgrs.ToArray());
				Constants.UpdateColor(color, originalImage, maskImage);
				calibrating = false;
				originalImageBox.Source = Utility.ToBitmapSource(originalImage);

				ColorChooser.IsEnabled = true;
			}
			// System.Windows.Interop.ComponentDispatcher.ThreadIdle += new System.EventHandler(UpdateFrame);
		}


		public void OriginalImageClicked(object sender, RoutedEventArgs e)
		{
			if (calibrating)
			{
				System.Windows.Point wp = Mouse.GetPosition(originalImageBox);
				System.Drawing.Point dp = new System.Drawing.Point((int)wp.X, (int)wp.Y);
				calibrationList.Add(dp);

				var circlesize = 10;
				maskImage.Draw(new CircleF(dp, circlesize), new Gray(255), -1);
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

