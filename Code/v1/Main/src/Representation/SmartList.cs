using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.Representation
{
	public class SmartList
	{
		private List<Representation.Tuple<Object, int>> Data;

		public SmartList()
		{
			this.Data = new List<Tuple<Object, int>>();
		}

		public Tuple<Object, int> this[int index] 
		{
			get
			{
				return Data[index];
			}
		}

		public void Add(Object item)
		{
			// Do smart stuff here
			var index = IndexOf(item);
			if (index == -1) Data.Add(new Tuple<Object, int>(item, 0));
			else
			{
				Data[index].Item1 = item;
				Data[index].Item2 = 0;
			}
		}

		public void Update()
		{
			//RUN COUNTERS
		}

		private bool Differs(Object update, Object current)
		{
			//If block:
			if (update.ObjectType == Constants.ObjectType.Block)
			{
				var distance = Util.Maths.Distance(update.Position, current.Position);
				return distance > Constants.BlockRange;
			}
			//If wall:
			else if (update.ObjectType == Constants.ObjectType.Wall)
			{
				Wall updateWall = (Wall)update;
				Wall currentWall = (Wall)current;
				if (currentWall.Polygon.Points.Count != updateWall.Polygon.Points.Count) return true;
				//Assume amount of points is equal
				for (int i = 0; i < currentWall.Polygon.Points.Count; i++)
				{
					if (Util.Maths.Distance(currentWall.Polygon.Points[i], updateWall.Polygon.Points[i]) 
						> Constants.WallRange)
					{
						return true;
					}
				}
				return false;
			}
			//If goal:
			else if (update.ObjectType == Constants.ObjectType.Goal)
			{
				var distance = Util.Maths.Distance(update.Position, current.Position);
				return distance > Constants.GoalRange;
			}
			//If transport:
			else if (update.ObjectType == Constants.ObjectType.TransportRobot)
			{
				var distance = Util.Maths.Distance(update.Position, current.Position);
				return distance > Constants.RobotRange;
			}
			//If guard:
			else if (update.ObjectType == Constants.ObjectType.GuardRobot)
			{
				var distance = Util.Maths.Distance(update.Position, current.Position);
				return distance > Constants.RobotRange;
			}
			return true;
		}

		public IEnumerable<Object> GetByType(Constants.ObjectType type)
		{
			return from d in (from a in Data select a.Item1) where d.ObjectType == type select d;
		}

		/// <summary>
		/// Method returns -1 if object could not be found.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int GetCounter(Object item)
		{
			foreach (Tuple<Object, int> o in Data)
			{
				if (!Differs(item, o.Item1)) return o.Item2;
			}
			return -1;
		}

		public int IndexOf(Object item)
		{
			var objList = (from d in Data select d.Item1).ToList();
			for (int i = 0; i < objList.Count(); i++)
			{
				if (!Differs(item, objList[i])) return i;
			}
			return -1;
		}
	}
}
