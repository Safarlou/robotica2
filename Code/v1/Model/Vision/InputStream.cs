using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Vision
{
	abstract class InputStream
	{
		public event InputStreamFrameReadyEventHandler InputStreamFrameReadyEvent;

		protected void OnInputStreamFrameReadyEvent(InputStreamFrameReadyEventArgs e)
		{

		}
	}
}
