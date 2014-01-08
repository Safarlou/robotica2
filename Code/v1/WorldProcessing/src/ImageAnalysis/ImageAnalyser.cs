﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldProcessing.Vision;

namespace WorldProcessing.ImageAnalysis
{
	/// <summary>
	/// FrameAnalysedEvent is raised when objects have been extracted from an incoming vision frame.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void FrameAnalysedEventHandler(object sender, FrameAnalysedEventArgs e);

	/// <summary>
	/// Contains <see cref="AnalysisResults"/>.
	/// </summary>
	public class FrameAnalysedEventArgs : EventArgs
	{
		public AnalysisResults results { get; private set; }

		public FrameAnalysedEventArgs(AnalysisResults results)
		{
			this.results = results;
		}
	}

	/// <summary>
	/// Contains the objects extracted from an image and the results of all intermediate extractions, as (color,result) tuples.
	/// </summary>
	public struct AnalysisResults
	{
		public Image<Bgr, byte> originalImage;
		public List<Tuple<Constants.ObjectType, Image<Gray, byte>>> colorMasks;
		public List<Tuple<Constants.ObjectType, Contour<System.Drawing.Point>>> contours;
		public List<Tuple<Constants.ObjectType, List<Seq<System.Drawing.Point>>>> shapes;
		public List<Representation.Object> objects;

		public AnalysisResults(Image<Bgr, byte> image, ColorMaskAnalysisResults[] results, List<Representation.Object> objects)
		{
			this.originalImage = image;
			this.colorMasks = new List<Tuple<Constants.ObjectType, Image<Gray, byte>>>();
			this.contours = new List<Tuple<Constants.ObjectType, Contour<System.Drawing.Point>>>();
			this.shapes = new List<Tuple<Constants.ObjectType, List<Seq<System.Drawing.Point>>>>();

			foreach (ColorMaskAnalysisResults result in results)
			{
				this.colorMasks.Add(new Tuple<Constants.ObjectType, Image<Gray, byte>>(result.color, result.colorMask));
				this.contours.Add(new Tuple<Constants.ObjectType, Contour<System.Drawing.Point>>(result.color, result.contours));
				this.shapes.Add(new Tuple<Constants.ObjectType, List<Seq<System.Drawing.Point>>>(result.color, result.shapes));
			}

			this.objects = objects;
		}
	}

	/// <summary>
	/// Contains the results of extractions for a single color mask.
	/// </summary>
	public struct ColorMaskAnalysisResults
	{
		public Constants.ObjectType color;
		public Image<Gray, byte> colorMask;
		public Contour<System.Drawing.Point> contours;
		public List<Seq<System.Drawing.Point>> shapes;

		public ColorMaskAnalysisResults(Constants.ObjectType color, Image<Gray, byte> colorMask, Contour<System.Drawing.Point> contours, List<Seq<System.Drawing.Point>> shapes)
		{
			this.color = color;
			this.colorMask = colorMask;
			this.contours = contours;
			this.shapes = shapes;
		}
	}

	/// <summary>
	/// Subscribes to an <see cref="InputStream"/> and analyses the images it supplised, extracting <see cref="Representation.Object"/>s from the image through a series of extraction procedures.
	/// </summary>
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
			if (Constants.CalibratedObjectTypes.Count() > 0)
			{
				AnalysisResults result = AnalyseImage(stream.Frame);
				FrameAnalysedEvent(this, new FrameAnalysedEventArgs(result));
			}
		}

		private AnalysisResults AnalyseImage(Image<Bgr, byte> image)
		{
			var colorMasks = Extract.ColorMasks(image);

			var results = (from mask in colorMasks select AnalyseColorMask(mask)).ToArray(); // analyse colormasks individually

			var objects = Extract.Objects((from result in results select new Tuple<Constants.ObjectType,List<Seq<System.Drawing.Point>>>(result.color,result.shapes)).ToList());

			return new AnalysisResults(image, results, objects);
		}

		private ColorMaskAnalysisResults AnalyseColorMask(Tuple<Constants.ObjectType, Image<Gray, byte>> mask)
		{
			var contours = Extract.Contours(mask.Item2);
			var shapes = Extract.Shapes(mask.Item1,contours);
			return new ColorMaskAnalysisResults(mask.Item1, mask.Item2, contours, shapes);
		}
	}
}
