using WorldProcessing.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldProcessing.ImageAnalysis;

namespace WorldProcessing.Representation
{
	/// <summary>
	/// WorldModel creates and manages a modelled representation of images 
	/// from an InputStream. 
	/// </summary>
	public class WorldModel
	{
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

		}
	}
}
