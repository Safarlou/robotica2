using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Planning;
using WorldProcessing.Representation;
using WorldProcessing.Vision;

namespace WorldProcessing
{
	public partial class App : Application
	{
		InputStream inputStream;
		ImageAnalyser imageAnalyser;
		WorldModel worldModel;
		Planner planner;

		ImagingWindow imagingWindow;

		public App()
		{
			inputStream = new WebcamInputStream();
			imageAnalyser = new ImageAnalyser(inputStream);
			worldModel = new WorldModel(imageAnalyser);
			planner = new Planner(worldModel);

			imagingWindow = new ImagingWindow(inputStream, imageAnalyser, worldModel, planner);
			imagingWindow.Closed += OnImagingWindowClosed;
			imagingWindow.Show();
		}

		private void OnImagingWindowClosed(object sender, EventArgs args)
		{
			this.Shutdown();
		}

		public new void Shutdown()
		{
			inputStream.Stop();

			base.Shutdown();
		}
	}
}
