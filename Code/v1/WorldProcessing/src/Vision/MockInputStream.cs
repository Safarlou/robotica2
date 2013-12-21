using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Vision
{
	public class MockInputStream : InputStream
	{
		private string filename = "images/foto1.png";
		private double fps;

		public MockInputStream()
		{
			this.frame = new Image<Bgr, byte>(filename);
			this.fps = 3;
		}

		protected override void CreateFrames()
		{
			while (keepWorking)
			{
				System.Threading.Thread.Sleep((Int32)(1000 / fps));
				OnInputStreamFrameReadyEvent(new InputStreamFrameReadyEventArgs());
			}
		}

	}
}
