using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldProcessing.src.Vision;

namespace WorldProcessing.src.ImageAnalysis
{
	public enum AnalysisStep
	{
		OriginalImage,
		ColorMask,
		Contours,
		Shapes,
		Objects
	}

	public class FrameAnalysedEventArgs : EventArgs
	{
		public AnalysisStep step { get; private set; }
		public object data { get; private set; }

		public FrameAnalysedEventArgs(AnalysisStep step, object data)
		{
			this.step = step;
			this.data = data;
		}
	}

	public delegate void FrameAnalysedEventHandler(object sender, FrameAnalysedEventArgs e);

	class ImageAnalyser
	{
		public event FrameAnalysedEventHandler FrameAnalysedEvent = delegate { };

		private InputStream stream;

		public ImageAnalyser(InputStream stream)
		{
			this.stream = stream;
			stream.FrameReadyEvent += OnFrameReadyEvent;
		}

		public Type AnalysisStepType(AnalysisStep step)
		{
			switch (step)
			{
				case AnalysisStep.OriginalImage:
					return typeof(Image<Bgr,byte>);
				case AnalysisStep.ColorMask:
					return typeof(Tuple<Constants.Colors,Image<Gray,byte>>);
				case AnalysisStep.Contours:
					return typeof(Tuple<Constants.Colors,Contour<System.Drawing.Point>>);
				case AnalysisStep.Shapes:
					return typeof(Tuple<Constants.Colors,List<MCvBox2D>>);
				case AnalysisStep.Objects:
					return null;
				default:
					throw new Exception();
			}
		}

		private void OnFrameReadyEvent(object sender, EventArgs args)
		{
			Image<Bgr, byte> streamImage = stream.frame;
			object[] objects = AnalyseImage(streamImage);

			FrameAnalysedEvent(this, new FrameAnalysedEventArgs(AnalysisStep.OriginalImage,streamImage));
		}

		private object[] AnalyseImage(Image<Bgr, byte> image)
		{
			Tuple<Constants.Colors,Image<Gray, byte>>[] colorMasks = ExtractColorMasks(image);
			object[] x = (from mask in colorMasks select AnalyseColorMask(mask.Item2)).ToArray();
			return null;
		}

		private object[] AnalyseColorMask(Image<Gray, byte> image)
		{
			Contour<System.Drawing.Point> contours = ExtractContours(image);
			List<MCvBox2D> shapes = ExtractShapes(contours);
			return null;
		}

		private Tuple<Constants.Colors,Image<Gray, byte>>[] ExtractColorMasks(Image<Bgr, byte> image)
		{
			return Utility.FastColorMask(ref image, (Constants.Colors[])Enum.GetValues(typeof(Constants.Colors)));
		}

		private Contour<System.Drawing.Point> ExtractContours(Image<Gray, byte> image)
		{
			return image.FindContours(
						Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_LINK_RUNS,
						Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, new MemStorage());
		}

		private List<MCvBox2D> ExtractShapes(Contour<System.Drawing.Point> contour)
		{
			return Utility.FindRectangles(contour);
		}

		private void ExtractObjects(object args)
		{

		}
	}
}
