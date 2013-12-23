using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorldProcessing.src.Vision
{
	public class MockInputStream : InputStream
	{
		private const string filename = "images/foto1.png";
		private const double fps = 3;
		private Image<Bgr, byte> image;

		protected Thread workerThread;
		protected volatile bool keepWorking = false;

		public MockInputStream()
		{
			this.image = new Image<Bgr, byte>(filename);
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
		}

	}
}
