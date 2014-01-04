using System;
using System.Collections.Generic;
using WorldProcessing.ImageAnalysis;
using System.Linq;

namespace WorldProcessing.Representation
{
	public delegate void ModelUpdatedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// WorldModel creates and manages a modelled representation of images 
	/// from an InputStream. 
	/// </summary>
	public class WorldModel
	{
		public event ModelUpdatedEventHandler ModelUpdatedEvent = delegate { };

		public ImageAnalyser imageAnalyser;
		public Polygon Bounds;
		public List<Robot> Robots;
		public List<Object> Objects { get; private set; }

		public WorldModel(ImageAnalyser analyser)
		{
			this.imageAnalyser = analyser;

			imageAnalyser.FrameAnalysedEvent += OnFrameAnalysedEvent;
		}

		private void OnFrameAnalysedEvent(object sender, FrameAnalysedEventArgs args)
		{
			// Worldmodel right now just gathers the incoming objects into one list and passing it to anyone who's listening.
			// The plan being to keep an ActualModel that is updated according to the incoming model, reducing noise (jittery analysis) and accounting for temporarily missing objects, for example
			Objects = (from tup in args.results.objects select tup.Item2).Aggregate((a,b) => a.Concat(b).ToList()).ToList();
			ModelUpdatedEvent(this, new EventArgs());
		}
	}
}
