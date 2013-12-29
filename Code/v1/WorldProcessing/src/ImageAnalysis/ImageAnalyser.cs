using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using WorldProcessing.ImageAnalysis;
using WorldProcessing.Vision;

namespace WorldProcessing.ImageAnalysis
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
		public Image<Bgr, byte> originalImage;
		public List<Tuple<Constants.Colors, Image<Gray, byte>>> colorMasks;
		public List<Tuple<Constants.Colors, Contour<System.Drawing.Point>>> contours;
		public List<Tuple<Constants.Colors, List<Seq<System.Drawing.Point>>>> shapes;
		public List<Tuple<Constants.Colors, List<Representation.Object>>> objects;

		public AnalysisResults(Image<Bgr, byte> image, ColorMaskAnalysisResults[] results)
		{
			this.originalImage = image;
			this.colorMasks = new List<Tuple<Constants.Colors, Image<Gray, byte>>>();
			this.contours = new List<Tuple<Constants.Colors, Contour<System.Drawing.Point>>>();
			this.shapes = new List<Tuple<Constants.Colors, List<Seq<System.Drawing.Point>>>>();
			this.objects = new List<Tuple<Constants.Colors, List<Representation.Object>>>();

			foreach (ColorMaskAnalysisResults result in results)
			{
				this.colorMasks.Add(new Tuple<Constants.Colors, Image<Gray, byte>>(result.color, result.colorMask));
				this.contours.Add(new Tuple<Constants.Colors, Contour<System.Drawing.Point>>(result.color, result.contours));
				this.shapes.Add(new Tuple<Constants.Colors, List<Seq<System.Drawing.Point>>>(result.color, result.shapes));
				this.objects.Add(new Tuple<Constants.Colors, List<Representation.Object>>(result.color, result.objects));
			}
		}
	}

	public struct ColorMaskAnalysisResults
	{
		public Constants.Colors color;
		public Image<Gray, byte> colorMask;
		public Contour<System.Drawing.Point> contours;
		public List<Seq<System.Drawing.Point>> shapes;
		public List<Representation.Object> objects;

		public ColorMaskAnalysisResults(Constants.Colors color, Image<Gray, byte> colorMask, Contour<System.Drawing.Point> contours, List<Seq<System.Drawing.Point>> shapes, List<Representation.Object> objects)
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
		private bool analysing = false;

		public ImageAnalyser(InputStream stream)
		{
			this.stream = stream;
			stream.FrameReadyEvent += OnFrameReadyEvent;
			stream.Start();
		}

		private void OnFrameReadyEvent(object sender, EventArgs args)
		{
			if (Constants.CalibratedColors.Count() != 0 && !analysing)
			{
				analysing = true;

				Image<Bgr, byte> streamImage = stream.Frame;
				AnalysisResults result = AnalyseImage(streamImage);

				FrameAnalysedEvent(this, new FrameAnalysedEventArgs(result));

				analysing = false;
			}
		}

		private AnalysisResults AnalyseImage(Image<Bgr, byte> image)
		{
			var colorMasks = Extract.ColorMasks(image);
			var results = (from mask in colorMasks select AnalyseColorMask(mask)).ToArray();
			return new AnalysisResults(image, results);
		}

		private ColorMaskAnalysisResults AnalyseColorMask(Tuple<Constants.Colors, Image<Gray, byte>> mask)
		{
			var contours = Extract.Contours(mask.Item2);
			var shapes = Extract.Shapes(contours);
			var objects = Extract.Objects(shapes, mask.Item1);
			return new ColorMaskAnalysisResults(mask.Item1, mask.Item2, contours, shapes, objects);
		}
	}
}
