using System;
using System.Windows;
using WorldProcessing.Controller;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Interface;
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
		PlanExecutor planExecutor;

		ImagingWindow imagingWindow;
		RobotMonitor robotMonitor;

		NXTController transport, guard;

		public App()
		{
			//NXTControllers instantiëren
			transport = new NXTController(null, "CONVOI Transport");
			guard = new NXTController(null, "CONVOI Guard");

			inputStream = new WebcamInputStream();
			imageAnalyser = new ImageAnalyser(inputStream);
			worldModel = new WorldModel(imageAnalyser);
			planner = new Planner(worldModel);
			planExecutor = new PlanExecutor(planner, worldModel, transport, guard);

			imagingWindow = new ImagingWindow(inputStream, imageAnalyser, worldModel, planner);
			imagingWindow.Closed += OnImagingWindowClosed;
			imagingWindow.Show();

			robotMonitor = new RobotMonitor(transport, guard);
			robotMonitor.Closed += OnImagingWindowClosed;
			robotMonitor.Show();

			inputStream.Start();
		}

		private void OnImagingWindowClosed(object sender, EventArgs args)
		{
			if (transport.Connected) transport.Stop();
			if (guard.Connected) guard.Stop();
			transport.Brick.Disconnect();
			guard.Brick.Disconnect();
			this.Shutdown();
		}

		public new void Shutdown()
		{
			inputStream.Stop();

			base.Shutdown();
		}
	}
}
