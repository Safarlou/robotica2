using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
    // Standard values as described by Marein:
    // red in foto1 = new Bgr(73, 55, 206)
    // green in foto1 = new Bgr(106, 169, 74)
    class Constants
	{
		static public Bgr Red { get; set; }
		static public double ThresholdRed { get; set; }

		static public Bgr Green { get; set; }
		static public double ThresholdGreen { get; set; }
        
        static Constants()
        {
        }

        static public void UpdateRed(params Bgr[] args)
        {
			if (args.Length != 0)
			{
				Red = Utility.Average(args);
				ThresholdRed = (from a in args select Utility.ColorDistance(Red, a)).Max()*2;
			}
        }

        static public void UpdateGreen(params Bgr[] args)
        {
			Green = Utility.Average(args);
			ThresholdGreen = (from a in args select Utility.ColorDistance(Green, a)).Max();
        }
    }
}
