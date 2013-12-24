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
	public class FrameAnalysedEventArgs : EventArgs
	{
		public AnalysisResults results { get; private set; }

		public FrameAnalysedEventArgs(AnalysisResults results)
		{
			this.results = results;
		}
	}

	public struct AnalysisResults
	{
		public Image<Bgr, byte> originalImage { get; private set; }
		public Tuple<Constants.Colors, Image<Gray, byte>>[] colorMasks { get; private set; }
		public Tuple<Constants.Colors, Contour<System.Drawing.Point>>[] contours { get; private set; }
		public Tuple<Constants.Colors, List<MCvBox2D>>[] shapes { get; private set; }
		public Tuple<Constants.Colors, Representation.Object>[] objects { get; private set; }
	}

	public struct ColorMaskAnalysisResults
	{
		public Constants.Colors color;
		public Image<Gray, byte> colorMask;
		public Contour<System.Drawing.Point> contours;
		public List<MCvBox2D> shapes;
		public List<Representation.Object> objects;

		public ColorMaskAnalysisResults(Constants.Colors color, Image<Gray, byte> colorMask, Contour<System.Drawing.Point> contours, List<MCvBox2D> shapes, List<Representation.Object> objects)
		{
			this.color = color;
			this.colorMask = colorMask;
			this.contours = contours;
			this.shapes = shapes;
			this.objects = objects;
		}
	}

	public delegate void FrameAnalysedEventHandler(object sender, FrameAnalysedEventArgs e);

	public class ImageAnalyser
	{
		public event FrameAnalysedEventHandler FrameAnalysedEvent = delegate { };

		private InputStream stream;

		public ImageAnalyser(InputStream stream)
		{
			this.stream = stream;
			stream.FrameReadyEvent += OnFrameReadyEvent;
			stream.Start();
		}

		private void OnFrameReadyEvent(object sender, EventArgs args)
		{
			Image<Bgr, byte> streamImage = stream.Frame;
			Tuple<Constants.Colors, Contour<System.Drawing.Point>, List<MCvBox2D>, Representation.Object>[] result = AnalyseImage(streamImage);

			FrameAnalysedEvent(this, new FrameAnalysedEventArgs(streamImage, result));
		}

		private AnalysisResults AnalyseImage(Image<Bgr, byte> image)
		{
			if (Constants.ColorsCalibrated)
			{
				Tuple<Constants.Colors, Image<Gray, byte>>[] colorMasks = ExtractColorMasks(image);
				Tuple<Constants.Colors, Contour<System.Drawing.Point>, List<MCvBox2D>, Representation.Object>[] shapes = (from mask in colorMasks select AnalyseColorMask(mask)).ToArray();
				return shapes;
			}
			else
			{
				return null;
			}
		}

		private Tuple<Constants.Colors, Image<Gray, byte>>[] ExtractColorMasks(Image<Bgr, byte> image)
		{
			return Utility.FastColorMask(ref image, (Constants.Colors[])Enum.GetValues(typeof(Constants.Colors)));
		}

		private ColorMaskAnalysisResults AnalyseColorMask(Tuple<Constants.Colors, Image<Gray, byte>> mask)
		{
			Contour<System.Drawing.Point> contours = ExtractContours(mask.Item2);
			List<MCvBox2D> shapes = ExtractShapes(contours);
			// extractobjects
			return new ColorMaskAnalysisResults(mask.Item1, mask, contours, shapes, null);
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
