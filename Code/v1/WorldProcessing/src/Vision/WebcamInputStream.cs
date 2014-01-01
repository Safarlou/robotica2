using Emgu.CV;
using Emgu.CV.CvEnum;
using System;

namespace WorldProcessing.Vision
{
	class WebcamInputStream : InputStream
	{
		private Capture capture;

		public WebcamInputStream(int camIndex = 0)
		{
			capture = new Capture(camIndex);

			capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, Constants.FrameWidth);
			capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, Constants.FrameHeight);
			capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_AUTO_EXPOSURE, 0);

			capture.ImageGrabbed += OnGrabEvent;
		}

		public override void Start()
		{
			capture.Start();
		}

		public override void Stop()
		{
			capture.Stop();
		}

		private void OnGrabEvent(object sender, EventArgs args)
		{
			RaiseFrameReadyEvent(capture.RetrieveBgrFrame().Copy()); // need to do a copy to fill the Data property needed in processing
		}
	}
}
