using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Vision
{
	//Delegate method to handle InputStreamFrameReady events
	public delegate void InputStreamFrameReadyEventHandler(object sender, InputStreamFrameReadyEventArgs e);

	class InputStreamFrameReadyEventArgs : EventArgs
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
