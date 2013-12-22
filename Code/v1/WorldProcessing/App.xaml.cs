using System;
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
			// todo: start vision
			imageAnalyser = new ImageAnalyser(inputStream);
			// todo: give analysis to planner

			// todo: give vision, analysis and planner to interface

			new ImagingWindow().Show();
		}
    }
}
