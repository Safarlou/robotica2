using WorldProcessing.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Representation
{
	/// <summary>
	/// WorldModel creates and manages a modelled representation of images 
	/// from an InputStream. 
	/// </summary>
	public class WorldModel
	{
		public InputStream Input;
		public Polygon Bounds;
		public List<Robot> Robots;
		public List<Object> Objects { get; private set; }

		public WorldModel(InputStream input)
		{
			Input = input;
		}
	}
}
