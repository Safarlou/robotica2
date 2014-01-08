using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Threading;

namespace WorldProcessing.Vision
{
	public class MockInputStream : InputStream
	{
		private const string filename = "images/foto1edit3.png";
		private const double fps = 3;
		private Image<Bgr, byte> image;

		protected Thread workerThread;
		protected volatile bool keepWorking = false;

		public MockInputStream()
		{
			this.image = new Image<Bgr, byte>(filename);

			Constants.FrameWidth = image.Width;
			Constants.FrameHeight = image.Height;
		}

		public override void Start()
		{
			keepWorking = true;
			if (workerThread == null)
				workerThread = new Thread(new ThreadStart(CreateFrames));
			workerThread.Start();
		}

		public override void Stop()
		{
			if (workerThread.IsAlive)
			{
				keepWorking = false;
				workerThread.Abort();
				workerThread.Join();
			}
		}

		private void CreateFrames()
		{
			while (keepWorking)
			{
				System.Threading.Thread.Sleep((Int32)(1000 / fps));
				RaiseFrameReadyEvent(image);
			}
			//int n = 0;
			//while (keepWorking)
			//{
			//	System.Threading.Thread.Sleep((Int32)(1000 / fps));
			//	image = new Image<Bgr, byte>("images/foto1edit" + n + ".png");
			//	RaiseFrameReadyEvent(image);
			//	n = (n + 1) % 2;
			//}
		}

	}
}
