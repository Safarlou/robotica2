using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Vision
{
	public class InputStreamFrameReadyEventArgs : EventArgs
	{
		private DateTime timestamp;

		public InputStreamFrameReadyEventArgs()
		{
			this.timestamp = DateTime.Now;
		}

		public DateTime Timestamp
		{
			get { return this.timestamp; }
		}
	}
}
