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
	//Delegate method to handle InputStreamFrameReady events
	public delegate void InputStreamFrameReadyEventHandler(object sender, InputStreamFrameReadyEventArgs e);

	public abstract class InputStream
	{
		public event InputStreamFrameReadyEventHandler InputStreamFrameReadyEvent;

		protected Image<Bgr, byte> frame;
		protected Thread workerThread;
		protected volatile bool keepWorking = true;
		protected Object locker = new Object();

		/// <summary>
		/// To be implemented by extending classes. Method will be ran in a separate thread.
		/// </summary>
		protected abstract void CreateFrames();

		public Image<Bgr, byte> Frame
		{
			get
			{
				lock (locker)
				{
					return this.frame;
				}
			}
		}

		public void Start()
		{
			keepWorking = true;
			if (workerThread == null) 
				workerThread = new Thread(new ThreadStart(CreateFrames));
			workerThread.Start();
		}

		public void Stop()
		{
			if (workerThread.IsAlive)
			{
				keepWorking = false;
				workerThread.Abort();
				workerThread.Join();
			}
		}

		protected void OnInputStreamFrameReadyEvent(InputStreamFrameReadyEventArgs e)
		{
			InputStreamFrameReadyEvent(this, e);
		}
	}
}
