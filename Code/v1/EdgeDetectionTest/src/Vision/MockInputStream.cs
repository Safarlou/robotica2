using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Vision
{
	class MockInputStream : InputStream
	{
		private string filename = "images/foto1.png";
		private double fps = 3;

		public MockInputStream()
		{
			this.frame = new Image<Bgr, byte>(filename);
		}

		protected override void CreateFrames()
		{
			while (keepWorking)
			{
				System.Threading.Thread.Sleep(1000 / 3);
				OnInputStreamFrameReadyEvent(new InputStreamFrameReadyEventArgs());
			}
		}

	}
}
