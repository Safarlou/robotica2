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
		public List<Wall> Walls { get; private set; }
		public List<Block> Blocks { get; private set; }
		public TransportRobot TransportRobot { get; private set; }
		public GuardRobot GuardRobot { get; private set; }
		public Goal Goal { get; private set; }

		public WorldModel(ImageAnalyser analyser)
		{
			this.imageAnalyser = analyser;

			imageAnalyser.FrameAnalysedEvent += OnFrameAnalysedEvent;
		}

		private void OnFrameAnalysedEvent(object sender, FrameAnalysedEventArgs args)
		{
			Walls = new List<Wall>();
			Blocks = new List<Block>();

			// Worldmodel right now just gathers the incoming objects into one list and passing it to anyone who's listening.
			// The plan being to keep an ActualModel that is updated according to the incoming model, reducing noise (jittery analysis) and accounting for temporarily missing objects, for example
			foreach (var obj in args.results.objects)
			{
				switch (obj.ObjectType)
				{
					case Constants.ObjectType.Wall:
						Walls.Add((Wall)obj);
						break;
					case Constants.ObjectType.Block:
						Blocks.Add((Block)obj);
						break;
					case Constants.ObjectType.Robot:
						// shouldn't exist on its own
						break;
					case Constants.ObjectType.TransportRobot:
						TransportRobot = (TransportRobot)obj;
						break;
					case Constants.ObjectType.GuardRobot:
						GuardRobot = (GuardRobot)obj;
						break;
					case Constants.ObjectType.Goal:
						Goal = (Goal)obj;
						break;
				}
			}

			ModelUpdatedEvent(this, new EventArgs());
		}
	}
}
