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
	public class FrameReadyEventArgs : EventArgs
	{
		public DateTime timestamp { get; private set; }
		public Image<Bgr, byte> image { get; private set; }

		public FrameReadyEventArgs(Image<Bgr, byte> image)
		{
			this.timestamp = DateTime.Now;
			this.image = image;
		}
	}

	//Delegate method to handle InputStreamFrameReady events
	public delegate void FrameReadyEventHandler(object sender, FrameReadyEventArgs e);

	public abstract class InputStream
	{
		public event FrameReadyEventHandler FrameReadyEvent = delegate { };
		private Object thisLock = new Object();

		public abstract void Start();
		public abstract void Stop();

		private Image<Bgr, byte> _frame;
		public Image<Bgr, byte> frame
		{
			get
			{
				lock (thisLock)
				{
					return this._frame;
				}
			}
			private set {}
		}

		protected void RaiseFrameReadyEvent(Image<Bgr,byte> image)
		{
			this._frame = image;
			FrameReadyEvent(this, new FrameReadyEventArgs(frame));
		}
	}
}
