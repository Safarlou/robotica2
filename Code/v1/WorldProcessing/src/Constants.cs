using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldProcessing
{
	public static class Constants
	{
		/// <summary>
		/// Desired frame width and height supplied to Vision
		/// </summary>
		public static int FrameWidth = 1024;
		public static int FrameHeight = 768;

		public static double OrientationMargin = (20 * Math.PI) / 180; //degrees

		public static int DesiredFPS = 3;

		/// <summary>
		/// The different object types that will be classified
		/// </summary>
		public enum ObjectType { Wall, Block, Robot, TransportRobot, GuardRobot, Goal };

		/// <summary>
		/// Converting object types enum values to representation object types
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static Type toType(this ObjectType objectType)
		{
			switch (objectType)
			{
				case ObjectType.Wall: return typeof(Representation.Wall); // black
				case ObjectType.Block: return typeof(Representation.Block); // red
				case ObjectType.Robot: return typeof(Representation.Robot); // green
				case ObjectType.TransportRobot: return typeof(Representation.TransportRobot); // yellow
				case ObjectType.GuardRobot: return typeof(Representation.GuardRobot); // purple
				case ObjectType.Goal: return typeof(Representation.Goal); // blue
				default: throw new ArgumentException("Type undefined.");
			}
		}

		/// <summary>
		/// Get a list of the object type enum values
		/// </summary>
		static public List<ObjectType> ObjectTypes { get { return Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>().ToList(); } }

		/// <summary>
		/// Get a list of currently calibrated object type enum values
		/// </summary>
		static public List<ObjectType> CalibratedObjectTypes { get { return ObjectTypes.FindAll(new Predicate<ObjectType>(x => objectTypesCalibrated[ObjectTypes.IndexOf(x)])).ToList(); } }

		/// <summary>
		/// The color info for the different object types
		/// </summary>
		static public Tuple<Bgr, double>[] ColorInfo; // (average,threshold)

		/// <summary>
		/// Whether all object types have been calibrated
		/// </summary>
		static public bool ObjectTypesCalibrated { get { return Util.Func.all(objectTypesCalibrated); } }
		static private bool[] objectTypesCalibrated;

		/// <summary>
		/// The threshold multiplier for color matching. 1.0 = no additional threshold, 2.0 = threshold twice as big as ColorInfo threshold, etc
		/// </summary>
		static private readonly double thresholdMultiplier = 1.0;

		static Constants()
		{
			ColorInfo = new Tuple<Bgr, double>[Enum.GetNames(typeof(ObjectType)).Length];
			objectTypesCalibrated = (from name in Enum.GetNames(typeof(ObjectType)) select false).ToArray();
		}

		static public void UpdateColor(ObjectType objectType, Bgr[] data)
		{
			if (data.Length != 0)
			{
				var average = Util.Color.Average(data);
				var threshold = (from a in data select Util.Color.Distance(average, a)).Max();
				ColorInfo[(int)objectType] = new Tuple<Bgr, double>(average, threshold);

				objectTypesCalibrated[(int)objectType] = true;
			}
		}

		static public void UpdateColor(ObjectType objectType, Image<Bgr, byte> image, Image<Gray, byte> mask)
		{
			List<Bgr> datalist = new List<Bgr>();

			var white = new Gray(255);

			for (int y = image.Rows - 1; y >= 0; y--)
				for (int x = image.Cols - 1; x >= 0; x--)
					if (mask[y, x].Equals(white))
						datalist.Add(image[y, x]);

			UpdateColor(objectType, datalist.ToArray());
		}

		static public Bgr getColor(ObjectType objectType)
		{
			return ColorInfo[(int)objectType].Item1;
		}

		static public double getThreshold(ObjectType objectType)
		{
			return ColorInfo[(int)objectType].Item2 * thresholdMultiplier;
		}

		static public void saveObjectTypeCalibration(String fileName)
		{
			if (ObjectTypesCalibrated)
			{
				System.Xml.Serialization.XmlSerializer writer1 = new System.Xml.Serialization.XmlSerializer(typeof(Bgr[]));
				System.Xml.Serialization.XmlSerializer writer2 = new System.Xml.Serialization.XmlSerializer(typeof(double[]));
				System.IO.StreamWriter file1 = new System.IO.StreamWriter(fileName + "1");
				System.IO.StreamWriter file2 = new System.IO.StreamWriter(fileName + "2");
				writer1.Serialize(file1, (from c in ColorInfo select c.Item1).ToArray());
				writer2.Serialize(file2, (from c in ColorInfo select c.Item2).ToArray());
				file1.Close();
				file2.Close();
			}
		}

		static public void loadObjectTypeCalibration(String fileName)
		{
			System.Xml.Serialization.XmlSerializer reader1 = new System.Xml.Serialization.XmlSerializer(typeof(Bgr[]));
			System.Xml.Serialization.XmlSerializer reader2 = new System.Xml.Serialization.XmlSerializer(typeof(double[]));
			System.IO.StreamReader file1 = new System.IO.StreamReader(fileName + "1");
			System.IO.StreamReader file2 = new System.IO.StreamReader(fileName + "2");
			var bgrs = (Bgr[])reader1.Deserialize(file1);
			var doubles = (double[])reader2.Deserialize(file2);

			for (int i = 0; i < ColorInfo.Length; i++)
				ColorInfo[i] = new Tuple<Bgr, double>(bgrs[i], doubles[i]);

			objectTypesCalibrated = (from name in Enum.GetNames(typeof(ObjectType)) select true).ToArray();
		}
	}
}
