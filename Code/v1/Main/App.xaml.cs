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
			var x = new Emgu.CV.Seq<System.Drawing.Point>(new Emgu.CV.MemStorage());

			x.Push(new System.Drawing.Point(1, 0));
			x.Push(new System.Drawing.Point(0, 5));
			x.Push(new System.Drawing.Point(2, 10));
			x.Push(new System.Drawing.Point(7, 11));
			x.Push(new System.Drawing.Point(8, 15));
			x.Push(new System.Drawing.Point(12, 13));
			x.Push(new System.Drawing.Point(9, 6));
			x.Push(new System.Drawing.Point(4, 5));

			var y = x.GetMinAreaRect();

			foreach (var v in y.GetVertices())
				Console.WriteLine(v);

			var z = new NavPolygon(y.GetVertices());

			Console.WriteLine(z.Area);

			return;

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
