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
	/// Window to display any processing steps. Particularly meant to display any information that is derived directly from the inputstream and is displayed as an adaption of the input frame
	/// </summary>
	public partial class ImagingWindow : Window
	{
		private string filename = "not used";

		private List<System.Drawing.Point> calibrationList;
		private bool calibrating = false;

		Image<Bgr, byte> originalImage;
		Image<Gray, byte> maskImage;

		public ImagingWindow(InputStream input, ImageAnalyser analyser, WorldModel model, Planner planner)
		{
			InitializeComponent();

			input.FrameReadyEvent += OnFrameReadyEvent;
			analyser.FrameAnalysedEvent += OnFrameAnalysedEvent;
			model.ModelUpdatedEvent += OnModelUpdatedEvent;
			planner.PathPlannedEvent += OnPathPlannedEvent;

			fileTextBox.Text = filename;

			foreach (Constants.ObjectType color in Enum.GetValues(typeof(Constants.ObjectType))) ColorChooser.Items.Add(color);
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
					setImageBox(originalImageBox, maskedImage);
				}
				else
					setImageBox(originalImageBox, originalImage);
			}));
		}

		private void setImageBox(System.Windows.Controls.Image imagebox, IImage image)
		{
			if (((System.Windows.Controls.TabItem)imagebox.Parent).IsSelected)
				imagebox.Source = Util.Image.ToBitmapSource(image);
		}

		private void OnFrameAnalysedEvent(object sender, FrameAnalysedEventArgs args)
		{
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
				{
					var results = args.results;
					var objectType = (Constants.ObjectType)ColorChooser.SelectedValue;
					if (results.colorMasks.Find(new Predicate<Tuple<Constants.ObjectType, Image<Gray, byte>>>(a => a.Item1 == objectType)) != null)
					{
						setImageBox(extractImageBox, results.colorMasks[(int)objectType].Item2);
						setImageBox(contoursImageBox, Draw.Contours(originalImage, results.contours[(int)objectType].Item2));
						setImageBox(shapesImageBox, Draw.Shapes(originalImage, results.shapes[(int)objectType].Item2));
						setImageBox(objectsImageBox, Draw.Objects(originalImage, results.objects));
					}
				}));
		}

		private void OnModelUpdatedEvent(object sender, EventArgs e)
		{
			// draw model I guess, though this is already kind of done in the previous step
		}

		private void OnPathPlannedEvent(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke((System.Action)(() => // I just want to run the code inside this but then I get a threading-related error, apparently this is one solution, but maybe just subverting bad architecture...
				{
					var path = ((Planner)sender).path;

					var polys = NavMesh.meshdisplayhack;

					var image = Draw.Path(originalImage, path);
					//var image = originalImage.Convert<Bgr, byte>();

					foreach (var poly in polys)
					{
						var c = poly.Edges.Count;

						foreach (var edge in poly.Edges)
						{
							var l = new LineSegment2D(edge.V0.ToDrawingPoint(), edge.V1.ToDrawingPoint());
							image.Draw(l, new Bgr(0, 0, 0), 2);
						}
					}

					setImageBox(objectsImageBox, image);
				}));
		}

		public void StartCalibration(object sender, EventArgs e)
		{
			if (!calibrating)
			{
				ColorChooser.IsEnabled = false;
				Constants.ObjectType color = (Constants.ObjectType)ColorChooser.SelectedValue;

				calibrationList = new List<System.Drawing.Point>();
				calibrating = true;
				maskImage = originalImage.CopyBlank().Convert<Gray, byte>();
			}
		}

		public void FinalizeCalibration(object sender, EventArgs e)
		{
			if (calibrating)
			{
				Constants.ObjectType color = (Constants.ObjectType)ColorChooser.SelectedValue;

				var bgrs = Util.Image.PointsToBgr(ref originalImage, calibrationList.ToArray());
				Constants.UpdateColor(color, originalImage, maskImage);
				calibrating = false;
				setImageBox(originalImageBox, originalImage);

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

		private string objectTypeCalibrationFileName = "objectTypeCalibration";

		private void SaveCalibration(object sender, RoutedEventArgs e)
		{
			Constants.saveObjectTypeCalibration(objectTypeCalibrationFileName);
		}

		private void LoadCalibration(object sender, RoutedEventArgs e)
		{
			Constants.loadObjectTypeCalibration(objectTypeCalibrationFileName);
		}
	}
}

