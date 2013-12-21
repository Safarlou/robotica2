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

	}
}
