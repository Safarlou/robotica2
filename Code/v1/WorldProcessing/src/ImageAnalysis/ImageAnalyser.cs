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
		public Image<Bgr, byte> originalImage { get; private set; }
		public Tuple<Constants.Colors, Image<Gray, byte>>[] colorMasks { get; private set; }
		public Tuple<Constants.Colors, Contour<System.Drawing.Point>>[] contours { get; private set; }
		public Tuple<Constants.Colors, List<MCvBox2D>>[] shapes { get; private set; }
		public Tuple<Constants.Colors, object>[] objects { get; private set; }

		public FrameAnalysedEventArgs(Image<Bgr, byte> originalImage,
			Tuple<Constants.Colors, Image<Gray, byte>>[] colorMasks,
			Tuple<Constants.Colors, Contour<System.Drawing.Point>>[] contours,
			Tuple<Constants.Colors, List<MCvBox2D>>[] shapes,
			Tuple<Constants.Colors, object>[] objects)
		{
			this.originalImage = originalImage;
			this.colorMasks = colorMasks;
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
			Tuple<Constants.Colors,List<MCvBox2D>>[] shapes = AnalyseImage(streamImage);

			FrameAnalysedEvent(this, new FrameAnalysedEventArgs(streamImage, null, null, shapes, null));
		}

		private Tuple<Constants.Colors,List<MCvBox2D>>[] AnalyseImage(Image<Bgr, byte> image)
		{
			return null;
			Tuple<Constants.Colors, Image<Gray, byte>>[] colorMasks = ExtractColorMasks(image);
			Tuple<Constants.Colors, List<MCvBox2D>>[] shapes = (from mask in colorMasks select AnalyseColorMask(mask)).ToArray();
			return shapes;
		}

		private Tuple<Constants.Colors, Image<Gray, byte>>[] ExtractColorMasks(Image<Bgr, byte> image)
		{
			return Utility.FastColorMask(ref image, (Constants.Colors[])Enum.GetValues(typeof(Constants.Colors)));
		}

		private Tuple<Constants.Colors,List<MCvBox2D>> AnalyseColorMask(Tuple<Constants.Colors, Image<Gray, byte>> mask)
		{
			Contour<System.Drawing.Point> contours = ExtractContours(mask.Item2);
			List<MCvBox2D> shapes = ExtractShapes(contours);
			// extractobjects
			return new Tuple<Constants.Colors,List<MCvBox2D>>(mask.Item1,shapes);
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
