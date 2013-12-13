using EdgeDetectionTest.Properties;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EdgeDetectionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filename = "test3.jpg";
		
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        
        public MainWindow()
        {
            InitializeComponent();
        }

        public void init(object sender, RoutedEventArgs e)
        {
            Image<Bgr,Byte> image = new Image<Bgr, byte>(filename);
			image._EqualizeHist();
			//image = image.SmoothGaussian(3,3,3,3);
			//image._GammaCorrect(1.5);
			Image<Gray, byte> edgesgrayscale = image.Convert<Gray, byte>().PyrDown().PyrUp().Canny(new Gray(180), new Gray(120));

			List<MCvBox2D> rectangles = new List<MCvBox2D>();

			//NESTING GO
			using (MemStorage storage = new MemStorage())
			{
				for (Contour<System.Drawing.Point> contours = edgesgrayscale.FindContours(
					Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
					Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage); contours != null; contours = contours.HNext)
				{
					Contour<System.Drawing.Point> current = contours.ApproxPoly(contours.Perimeter * 0.051, storage);
					if (current.Area >= 50)
					{
						if (current.Total >= 4)
						{
							bool isRect = true;
							System.Drawing.Point[] pts = current.ToArray();
							LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);
							for (int i = 0; i < edges.Length; i++)
							{
								double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
								if (angle < 80 && angle > 100)
								{
									isRect = false;
									break;
								}
							}
							if (isRect) rectangles.Add(current.GetMinAreaRect());
						}
					}
				}
			}

			Image<Bgr, byte> contourResult = image.CopyBlank();
			foreach (MCvBox2D rect in rectangles)
			{
				contourResult.Draw(rect, new Bgr(System.Drawing.Color.Red), 1);
			}

			picturebox.Source = ToBitmapSource(contourResult);
			//picturebox.Source = ToBitmapSource(edgesgrayscale);

			foreach (MCvBox2D rect in rectangles)
			{
				Image<Gray, byte> mask = image.CopyBlank().Convert<Gray, byte>();
				mask.Draw(rect, new Gray(256), -1);
				Bgr avg = image.GetAverage(mask);
				contourResult.Draw(rect, avg, -1);
			}
			picturebox.Source = ToBitmapSource(contourResult);
			//picturebox.Source = ToBitmapSource(image.Convert<Gray,byte>());
			//picturebox.Source = ToBitmapSource(edgesgrayscale);
        }

        private BitmapSource ToBitmapSource(IImage img)
        {
            using (System.Drawing.Bitmap source = img.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptr, 
                    IntPtr.Zero, 
                    Int32Rect.Empty, 
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
