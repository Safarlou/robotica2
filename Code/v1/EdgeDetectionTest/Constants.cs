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
        static public Bgr Green { get; set; }
        
        static Constants()
        {
            Red = new Bgr(73, 55, 206);
            Green = new Bgr(106, 169, 74);
        }

        static public void UpdateRed(params Bgr[] args)
        {
            Red = Utility.Average(args);
        }

        static public void UpdateGreen(params Bgr[] args)
        {
            Green = Utility.Average(args);
        }
    }
}
