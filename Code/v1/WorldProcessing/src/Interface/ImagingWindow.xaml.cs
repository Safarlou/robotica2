using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Interface;
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

			foreach (Constants.Color color in Enum.GetValues(typeof(Constants.Color))) ColorChooser.Items.Add(color);
			ColorChooser.SelectedIndex = 0;
		}

		private void OnFrameReadyEvent(object sender, FrameReadyEventArgs args)
		{
			originalImage = args.image;
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
			{
				// todo: make some function to do this more reasonably...
				originalImageBox.Width = originalImage.Width;
				originalImageBox.Height = originalImage.Height;
				extractImageBox.Width = originalImage.Width;
				extractImageBox.Height = originalImage.Height;
				contoursImageBox.Width = originalImage.Width;
				contoursImageBox.Height = originalImage.Height;
				shapesImageBox.Width = originalImage.Width;
				shapesImageBox.Height = originalImage.Height;
				objectsImageBox.Width = originalImage.Width;
				objectsImageBox.Height = originalImage.Height;

				if (calibrating)
				{
					var maskedImage = originalImage.Copy();
					maskedImage.SetValue(new Bgr(0, 0, 0), maskImage);
					originalImageBox.Source = Util.Image.ToBitmapSource(maskedImage);
				}
				else
					originalImageBox.Source = Util.Image.ToBitmapSource(originalImage);
			}));
		}

		private void OnFrameAnalysedEvent(object sender, FrameAnalysedEventArgs args)
		{
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
				{

					var results = args.results;
					var color = ColorChooser.SelectedValue;
					extractImageBox.Source = Util.Image.ToBitmapSource(results.colorMasks[(int)color].Item2);
					contoursImageBox.Source = Util.Image.ToBitmapSource(Draw.Contours(originalImage, results.contours[(int)color].Item2));
					shapesImageBox.Source = Util.Image.ToBitmapSource(Draw.Shapes(originalImage, results.shapes[(int)color].Item2));
					objectsImageBox.Source = Util.Image.ToBitmapSource(Draw.Objects(originalImage, results.objects[(int)color].Item2));
				}));
		}

		public void StartCalibration(object sender, EventArgs e)
		{
			if (!calibrating)
			{
				ColorChooser.IsEnabled = false;
				Constants.Color color = (Constants.Color)ColorChooser.SelectedValue;

				calibrationList = new List<System.Drawing.Point>();
				calibrating = true;
				maskImage = originalImage.CopyBlank().Convert<Gray, byte>();
			}
		}

		public void FinalizeCalibration(object sender, EventArgs e)
		{
			if (calibrating)
			{
				Constants.Color color = (Constants.Color)ColorChooser.SelectedValue;

				var bgrs = Util.Image.PointsToBgr(ref originalImage, calibrationList.ToArray());
				Constants.UpdateColor(color, originalImage, maskImage);
				calibrating = false;
				originalImageBox.Source = Util.Image.ToBitmapSource(originalImage);

				ColorChooser.IsEnabled = true;
			}
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

		// todo Method is here for shits and giggles, not actually used right now
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

