using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace WorldProcessing.Vision
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
		public Image<Bgr, byte> Frame
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
			FrameReadyEvent(this, new FrameReadyEventArgs(Frame));
		}
	}
}
