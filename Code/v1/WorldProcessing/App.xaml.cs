﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Vision;

namespace WorldProcessing
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		ImageAnalyser imageAnalyser;
		InputStream inputStream;

		ImagingWindow imagingWindow;

		public App()
		{
			inputStream = new WebcamInputStream();
			imageAnalyser = new ImageAnalyser(inputStream);
			// todo: give analyser to planner. planner subscribes to analyser

			// todo: give stream, analyser and planner to interface. interface subscribes all and also communicates back (color calibration, ...)

			imagingWindow = new ImagingWindow(inputStream, imageAnalyser);
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
