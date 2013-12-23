﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WorldProcessing.src.ImageAnalysis;
using WorldProcessing.src.Vision;

namespace WorldProcessing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		ImageAnalyser imageAnalyser;
		InputStream inputStream;

		public App()
		{
			inputStream		= new WebcamInputStream();
			imageAnalyser	= new ImageAnalyser(inputStream);
			return;
			// todo: give analyser to planner. planner subscribes to analyser

			// todo: give stream, analyser and planner to interface. interface subscribes all and also communicates back (color calibration, ...)

			new ImagingWindow().Show();
		}
    }
}
